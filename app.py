"""
TeamWeave Matchmaking Microservice
====================================
A FastAPI Python microservice that receives participant data from the
TeamWeave Node.js/MongoDB backend (server.js → ClusteringService.js)
and returns balanced, cross-functional team assignments.

Integration Point
-----------------
The Node.js backend calls POST /matchmake with the list of MongoDB
Participant documents serialised to JSON. Each participant's skills
are an array of objects matching the MongoDB participantSkillSchema:
  { skillName: str, category: str, proficiencyLevel: 1-5 }

The `category` field (e.g. "Frontend", "Backend", "Data", "Design")
is used as the `primary_role` for team balancing.

Algorithm: Role-Stratified Round-Robin with Jaccard-Distance Swap
-----------------------------------------------------------------
PHASE 1 – Role Stratification:
  Participants are bucketed by their dominant skill category (the
  category with the highest average proficiencyLevel). Buckets are
  ordered by size descending so the most-represented role drives the
  round-robin cadence.

PHASE 2 – Round-Robin Interleave:
  Buckets are dealt round-robin (like a card dealer) to produce an
  ordered pool where adjacent participants come from different roles.
  Slicing this pool into equal-ish chunks naturally gives each team
  at least one member from each common role.

PHASE 3 – Jaccard Swap Optimisation:
  After initial assignment, pairwise Jaccard distances between skill
  sets are computed. Members are swapped between teams when the swap
  increases total intra-team diversity without violating size bounds.

  Jaccard distance:  D(A, B) = 1 − |A∩B| / |A∪B|
  Range: 0 (identical) → 1 (completely disjoint).
"""

import uuid
import math
import logging
from collections import defaultdict
from typing import List, Optional

import uvicorn
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel, field_validator, model_validator

# ---------------------------------------------------------------------------
# Logging
# ---------------------------------------------------------------------------
logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s [%(levelname)s] %(name)s – %(message)s",
)
logger = logging.getLogger("teamweave")

# ---------------------------------------------------------------------------
# Constants
# ---------------------------------------------------------------------------
MIN_TEAM_SIZE = 3
MAX_TEAM_SIZE = 5

# ---------------------------------------------------------------------------
# Pydantic Data Models — mirroring the MongoDB participantSkillSchema
# ---------------------------------------------------------------------------

class ParticipantSkill(BaseModel):
    """
    Mirrors the MongoDB participantSkillSchema embedded document:
      skillName  : str   (e.g. "React", "PostgreSQL")
      category   : str   (e.g. "Frontend", "Backend", "Data", "Design")
      proficiencyLevel : int 1-5 (default 3)
    """
    skillName: str
    category: str
    proficiencyLevel: int = 3

    @field_validator("proficiencyLevel")
    @classmethod
    def clamp_proficiency(cls, v: int) -> int:
        return max(1, min(5, v))

    @field_validator("category", "skillName")
    @classmethod
    def strip_and_title(cls, v: str) -> str:
        return v.strip()


class Participant(BaseModel):
    """
    Matches the MongoDB Participant document structure sent by the
    Node.js backend. The `_id` field from MongoDB is accepted as
    `user_id` via an alias, or you can pass `user_id` directly.
    """
    user_id: str                    # MongoDB _id stringified by Node backend
    name: str
    email: Optional[str] = None     # present in MongoDB, optional for matching
    skills: List[ParticipantSkill] = []

    @field_validator("skills")
    @classmethod
    def at_least_one_skill(cls, v: List[ParticipantSkill]) -> List[ParticipantSkill]:
        # Participants with zero skills are still accepted; they are placed
        # into teams based solely on round-robin position.
        return v

    def primary_role(self) -> str:
        """
        Derives the participant's primary role from their skill categories.
        Picks the category with the highest total proficiency weighting.
        Falls back to "General" when no skills are present.
        """
        if not self.skills:
            return "General"
        # Accumulate proficiency weights per category
        category_weight: dict[str, float] = defaultdict(float)
        for skill in self.skills:
            category_weight[skill.category] += skill.proficiencyLevel
        return max(category_weight, key=lambda c: category_weight[c])

    def skill_name_set(self) -> set:
        """Returns a lowercase set of skill names for Jaccard comparison."""
        return {s.skillName.lower() for s in self.skills}


class Team(BaseModel):
    """A single formed team returned to the Node.js backend."""
    team_id: str
    user_ids: List[str]
    member_names: List[str]     # convenience field for logging / UI
    roles_represented: List[str]


class TeamResponse(BaseModel):
    """Response payload returned to the Node.js ClusteringService."""
    teams: List[Team]
    total_participants: int
    total_teams: int
    algorithm: str = "RoleStratifiedRoundRobin+JaccardSwap"


# ---------------------------------------------------------------------------
# Helper: Jaccard Distance Between Two Skill-Name Sets
# ---------------------------------------------------------------------------

def jaccard_distance(a: set, b: set) -> float:
    """
    D(A, B) = 1 − |A∩B| / |A∪B|
    Returns 0.0 when one or both sets are empty (neutral, no diversity info).
    """
    if not a or not b:
        return 0.0
    return 1.0 - len(a & b) / len(a | b)


def team_diversity_score(team: List[Participant]) -> float:
    """Average pairwise Jaccard distance across all member pairs in a team."""
    if len(team) < 2:
        return 0.0
    total, count = 0.0, 0
    for i in range(len(team)):
        for j in range(i + 1, len(team)):
            total += jaccard_distance(team[i].skill_name_set(), team[j].skill_name_set())
            count += 1
    return total / count if count else 0.0


# ---------------------------------------------------------------------------
# Core Matching Algorithm
# ---------------------------------------------------------------------------

def build_balanced_teams(participants: List[Participant]) -> List[List[Participant]]:
    """
    Assigns participants to cross-functional teams using a role-stratified
    round-robin algorithm with a Jaccard-distance swap optimiser.

    Returns
    -------
    List[List[Participant]]
        A list of teams, each being a list of Participant objects.
    """
    n = len(participants)
    logger.info("Starting matchmaking for %d participants.", n)

    if n < MIN_TEAM_SIZE:
        raise ValueError(
            f"Need at least {MIN_TEAM_SIZE} participants to form a team, got {n}."
        )

    # ------------------------------------------------------------------
    # PHASE 1 — Bucket participants by primary_role
    # Each participant's role is derived from their highest-weighted
    # skill category (see Participant.primary_role()).
    # Within each bucket, sort by total proficiency (desc) so that the
    # most-skilled participants are seeded first as team anchors.
    # ------------------------------------------------------------------
    role_buckets: dict[str, List[Participant]] = defaultdict(list)
    for p in participants:
        role_buckets[p.primary_role()].append(p)

    for role in role_buckets:
        role_buckets[role].sort(
            key=lambda p: sum(s.proficiencyLevel for s in p.skills),
            reverse=True,
        )

    logger.info(
        "Role distribution: %s",
        {role: len(members) for role, members in role_buckets.items()},
    )

    # ------------------------------------------------------------------
    # PHASE 2 — Round-Robin Interleave Across Role Buckets
    # Sort buckets by size desc so the most populous role seeds the most
    # slots first, preventing any single team from being all one role.
    # ------------------------------------------------------------------
    bucket_lists = sorted(role_buckets.values(), key=len, reverse=True)
    max_bucket_size = max(len(b) for b in bucket_lists)

    ordered_pool: List[Participant] = []
    for round_idx in range(max_bucket_size):
        for bucket in bucket_lists:
            if round_idx < len(bucket):
                ordered_pool.append(bucket[round_idx])

    # ------------------------------------------------------------------
    # PHASE 3 — Slice the ordered pool into teams of balanced size
    # ------------------------------------------------------------------
    # Ideal number of teams: at least 1, never exceeding n // MIN_TEAM_SIZE
    # and never fewer than ceil(n / MAX_TEAM_SIZE)
    ideal_team_count = max(
        math.ceil(n / MAX_TEAM_SIZE),   # need enough teams to keep each ≤ MAX
        1,
    )
    # Ensure every team can reach MIN_TEAM_SIZE
    ideal_team_count = min(ideal_team_count, n // MIN_TEAM_SIZE)
    if ideal_team_count == 0:
        ideal_team_count = 1

    target_size = n // ideal_team_count
    remainder = n % ideal_team_count   # first `remainder` teams get +1

    teams: List[List[Participant]] = []
    idx = 0
    for t_idx in range(ideal_team_count):
        size = target_size + (1 if t_idx < remainder else 0)
        teams.append(ordered_pool[idx : idx + size])
        idx += size

    # ------------------------------------------------------------------
    # PHASE 4 — Jaccard Swap Optimiser (bounded to O(teams²) rounds)
    # For each pair of teams (i, j), attempt swapping one member from i
    # with one from j. Accept if total diversity score improves.
    # We cap to MAX_SWAP_ROUNDS full passes to keep latency bounded.
    # ------------------------------------------------------------------
    MAX_SWAP_ROUNDS = min(4, ideal_team_count)
    for _ in range(MAX_SWAP_ROUNDS):
        improved = False
        for i in range(len(teams)):
            for j in range(i + 1, len(teams)):
                baseline = team_diversity_score(teams[i]) + team_diversity_score(teams[j])
                best_gain, best_swap = 0.0, None

                for mi, member_i in enumerate(teams[i]):
                    for mj, member_j in enumerate(teams[j]):
                        candidate_i = teams[i][:mi] + [member_j] + teams[i][mi + 1:]
                        candidate_j = teams[j][:mj] + [member_i] + teams[j][mj + 1:]
                        gain = (
                            team_diversity_score(candidate_i)
                            + team_diversity_score(candidate_j)
                            - baseline
                        )
                        if gain > best_gain:
                            best_gain = gain
                            best_swap = (candidate_i, candidate_j)

                if best_swap:
                    teams[i], teams[j] = best_swap
                    improved = True

        if not improved:
            break   # Converged — no swap improved diversity

    logger.info(
        "Matchmaking complete. %d teams formed. Diversity scores: %s",
        len(teams),
        [round(team_diversity_score(t), 3) for t in teams],
    )
    return teams


# ---------------------------------------------------------------------------
# FastAPI Application
# ---------------------------------------------------------------------------

app = FastAPI(
    title="TeamWeave Matchmaking Microservice",
    description=(
        "Receives participant lists from the TeamWeave Node.js backend and "
        "returns cross-functional, skill-diverse team assignments. "
        "Skills are matched against MongoDB's participantSkillSchema shape."
    ),
    version="1.1.0",
)


@app.get("/health", tags=["Health"])
def health_check() -> dict:
    """Liveness probe used by the Node.js backend before delegating clustering."""
    return {"status": "ok", "service": "teamweave-matchmaking", "version": "1.1.0"}


@app.post("/matchmake", response_model=TeamResponse, tags=["Matchmaking"])
def matchmake(participants: List[Participant]) -> TeamResponse:
    """
    Accepts a list of MongoDB Participant documents (JSON-serialised) and
    returns balanced, cross-functional team assignments.

    Called by ClusteringService.js in the Node.js backend when
    `PYTHON_MATCHMAKING=true` is set in the environment.

    Raises:
        400: Empty list or too few participants.
        422: Pydantic validation failure (malformed payload).
    """
    logger.info("POST /matchmake — %d participants received.", len(participants))

    if not participants:
        raise HTTPException(status_code=400, detail="Participant list is empty.")

    try:
        raw_teams = build_balanced_teams(participants)
    except ValueError as exc:
        raise HTTPException(status_code=400, detail=str(exc)) from exc

    formed_teams: List[Team] = [
        Team(
            team_id=str(uuid.uuid4()),
            user_ids=[m.user_id for m in members],
            member_names=[m.name for m in members],
            roles_represented=list({m.primary_role() for m in members}),
        )
        for members in raw_teams
    ]

    return TeamResponse(
        teams=formed_teams,
        total_participants=len(participants),
        total_teams=len(formed_teams),
    )


# ---------------------------------------------------------------------------
# Entry Point — `python app.py` starts the dev server on port 8000
# ---------------------------------------------------------------------------

if __name__ == "__main__":
    uvicorn.run(
        "app:app",
        host="0.0.0.0",
        port=8000,
        reload=True,
        log_level="info",
    )
