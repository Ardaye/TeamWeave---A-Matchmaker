# TeamWeave - Intelligent Hackathon Team Matcher

[![Node.js](https://img.shields.io/badge/Node.js-v18%2B-green?logo=node.js)](https://nodejs.org/)
[![Python](https://img.shields.io/badge/Python-3.9%2B-blue?logo=python)](https://www.python.org/)
[![MongoDB](https://img.shields.io/badge/MongoDB-4.4%2B-brightgreen?logo=mongodb)](https://www.mongodb.com/)
[![Express.js](https://img.shields.io/badge/Express.js-4.19-blue?logo=express)](https://expressjs.com/)
[![FastAPI](https://img.shields.io/badge/FastAPI-0.111-teal?logo=fastapi)](https://fastapi.tiangolo.com/)
[![License](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

**TeamWeave** is an algorithmic team formation and recruitment portal built for hackathons, design sprints, and academic competitions. It eliminates unbalanced teams by applying a **Coverage-Maximizing Greedy algorithm** and a **Coverage Gap Swap Optimiser** to ensure every formed team collectively spans the full engineering stack — Frontend, Backend, Database/APIs, DevOps, Version Control, and Design. All participant and team data is persisted to **MongoDB**.

---

## Table of Contents

- [Key Features](#key-features)
- [System Architecture](#system-architecture)
- [Matchmaking Algorithm](#matchmaking-algorithm)
- [Database Schema (MongoDB)](#database-schema-mongodb)
- [REST API Reference](#rest-api-reference)
- [Quick Start and Setup](#quick-start-and-setup)
- [Configuration (.env)](#configuration-env)
- [Python Matchmaking Microservice](#python-matchmaking-microservice)
- [Running Automated Tests](#running-automated-tests)
- [Project Directory Structure](#project-directory-structure)

---

## Key Features

- **Coverage-Maximizing Greedy Team Formation**: Assembles teams so each team collectively spans as many full-stack engineering pillars as possible. Multi-pillar participants are seeded into teams first as anchors, then remaining participants are assigned to fill the most critical gaps.
- **Coverage Gap Swap Optimiser**: After initial assignment, scans all team pairs and executes member swaps that increase combined pillar coverage without regressing either team. Bounded to a fixed number of rounds for performance.
- **Python Matchmaking Microservice**: An optional FastAPI service (`app.py`) implementing the same greedy + swap algorithm in Python. The Node.js backend automatically delegates to it when `PYTHON_MATCHMAKER_URL` is set, with graceful fallback to the built-in JS engine if the service is unavailable.
- **K-Means Pre-clustering (JS fallback)**: When the Python service is not used, the JS engine runs K-Means++ clustering as a pre-pass before the greedy coverage assignment to maintain skill-vector diversity across teams.
- **Full MongoDB Persistence**: Built with Mongoose models supporting local MongoDB, remote MongoDB Atlas clusters, or an automatic zero-configuration embedded in-memory MongoDB instance (via `mongodb-memory-server`).
- **Persistent Embedded Database**: When no external MongoDB URI is configured, TeamWeave spins up a persistent embedded MongoDB instance that stores data on disk under `data/db`, so data survives server restarts without any external installation.
- **Team Locking and Member Rebalancing**: Organizers can lock finalized teams or transfer participants between teams. Locked teams are excluded from re-clustering runs.
- **Per-member Locking**: Individual members within a team can be locked independently of the team lock state.
- **Complete Audit Trail**: Automatically timestamps and records all administrative events including registrations, member moves, team locks, and clustering executions.
- **Per-team Coverage Reports**: Every clustering run returns a coverage report for each team showing `coverageScore` (0-1 fraction of pillars covered), `coveredPillars`, and `missingPillars`. This metadata is also stored on the Team document in MongoDB.
- **Analytics Dashboard and CSV Exports**: Live statistics on participants and team distributions with one-click CSV export for participants, teams, and audit logs.
- **Interactive Web Portal**: Responsive single-page UI served by the Express server, with real-time statistics, registration forms, clustering controls, and team management views.

---

## System Architecture

```
Client Browser (index.html)
        |
        | REST API / JSON
        v
Express.js Server (server.js, port 3000)
        |
        +-- ParticipantService    --> MongoDB (participants collection)
        +-- TeamService           --> MongoDB (teams, audit_logs collections)
        +-- ClusteringService
              |
              +-- [Primary] HTTP POST /matchmake
              |         |
              |         v
              |   Python FastAPI Microservice (app.py, port 8000)
              |   Coverage-Maximizing Greedy + Coverage Gap Swap
              |
              +-- [Fallback] JS Coverage-Maximizing K-Means
                        |
                        v
                  SkillVectorEncoder (vector math, Euclidean distance)

MongoDB Collections:
  participants    -- participant profiles and embedded skill ratings
  teams           -- formed teams with member rosters and coverage metadata
  skills          -- technology skill catalog (24 entries)
  audit_logs      -- timestamped event trail
  clustering_runs -- algorithm execution history
```

---

## Matchmaking Algorithm

TeamWeave uses a three-phase algorithm to produce full-stack-ready teams.

**Full-stack pillars (in priority order):**

| Priority | Pillar | Example Skills |
| :---: | :--- | :--- |
| 1 | Frontend | React, Vue, HTML/CSS, TypeScript, Angular |
| 2 | Backend | Node.js, Python, Java, C#, Django, FastAPI |
| 3 | Database/APIs | SQL, MongoDB, GraphQL, REST, PostgreSQL |
| 4 | DevOps/Deployment | Docker, AWS, CI/CD, GitHub Actions, Terraform |
| 5 | Version Control | Git, GitHub, GitLab, Bitbucket |
| 6 | Design | Figma, UI/UX, Sketch, Adobe |

**Phase 1 — Coverage-Maximizing Greedy Assembly:**
Participants covering more unique pillars are seeded as team anchors first. The algorithm then iterates through all unassigned participants, picking for each team slot the participant who adds the most new pillar coverage to the team that needs help the most (lowest current coverage score). Proficiency sum is used as a tiebreaker.

**Phase 2 — Coverage Gap Swap Optimiser:**
Scans all pairs of teams and attempts member swaps. A swap is accepted when it increases combined pillar coverage without reducing either team's coverage below its pre-swap baseline. Runs for up to `min(5, numTeams)` rounds or until no improvement is found.

**JS-only path — K-Means Pre-clustering (Phase 0):**
When running the built-in JS engine, a K-Means++ pre-clustering step runs first to produce an initial grouping that respects skill-vector diversity before the greedy coverage reassignment takes over.

---

## Database Schema (MongoDB)

### `participants` Collection

```json
{
  "_id": "ObjectId",
  "name": "Alice Johnson",
  "email": "alice@example.com",
  "profileStatus": "Submitted",
  "skills": [
    { "skillName": "React", "category": "Frontend", "proficiencyLevel": 5 },
    { "skillName": "JavaScript", "category": "Frontend", "proficiencyLevel": 4 }
  ],
  "interests": ["Web Applications", "UI/UX"],
  "createdAt": "ISODate",
  "updatedAt": "ISODate"
}
```

### `teams` Collection

```json
{
  "_id": "ObjectId",
  "name": "Team 1",
  "eventId": 1,
  "status": "Draft",
  "isLocked": false,
  "coverageScore": 0.83,
  "coveredPillars": ["Frontend", "Backend", "Database/APIs", "Version Control", "Design"],
  "missingPillars": ["DevOps/Deployment"],
  "members": [
    {
      "participantId": "ObjectId",
      "name": "Alice Johnson",
      "email": "alice@example.com",
      "skills": [],
      "isLocked": false,
      "addedAt": "ISODate"
    }
  ],
  "createdAt": "ISODate"
}
```

### `audit_logs` Collection

```json
{
  "_id": "ObjectId",
  "changeType": "ParticipantMoved",
  "actor": "Organizer",
  "oldValue": "Team 1",
  "newValue": "Team 2",
  "details": "Moved 'Alice Johnson' from 'Team 1' to 'Team 2'",
  "timestamp": "ISODate"
}
```

### `clustering_runs` Collection

```json
{
  "_id": "ObjectId",
  "eventId": 1,
  "teamsGenerated": 2,
  "parameters": {
    "targetTeamSize": 4,
    "algorithm": "CoverageMaximizingGreedy+CoverageGapSwap",
    "avgCoverageScore": 0.83
  },
  "teamIds": ["ObjectId"],
  "status": "Completed",
  "durationMs": 12,
  "executedAt": "ISODate"
}
```

---

## REST API Reference

| Method | Endpoint | Description |
| :--- | :--- | :--- |
| `GET` | `/api/health` | Service health status |
| `GET` | `/api/stats` | Dashboard metrics (participant count, team count, average size) |
| `GET` | `/api/skills` | Fetch all 24 skills in the catalog |
| `GET` | `/api/participants` | List participants (supports `?category=Frontend`, etc.) |
| `GET` | `/api/participants/:id` | Fetch a single participant by MongoDB ID |
| `POST` | `/api/participants` | Register a new participant and log to `audit_logs` |
| `GET` | `/api/teams` | List all teams with their members (supports `?eventId=1`) |
| `POST` | `/api/teams` | Create a team manually |
| `POST` | `/api/clustering/run` | Execute the matching algorithm and persist teams to MongoDB |
| `POST` | `/api/teams/move` | Move a member between teams and log to audit trail |
| `PATCH` | `/api/teams/:id/lock` | Toggle lock state of a team |
| `PATCH` | `/api/teams/:id/member/:participantId/lock` | Toggle lock state of an individual team member |
| `GET` | `/api/audit-logs` | Retrieve chronological audit trail (supports `?limit=N`) |
| `GET` | `/api/export/:type` | Download CSV for `participants`, `teams`, or `audit` |

### `POST /api/clustering/run` — Response Shape

```json
{
  "teams": [...],
  "clusteringRun": { "teamsGenerated": 2, "durationMs": 12, "status": "Completed" },
  "coverageReport": [
    {
      "coverageScore": 0.83,
      "coveredPillars": ["Frontend", "Backend", "Database/APIs", "Version Control", "Design"],
      "missingPillars": ["DevOps/Deployment"]
    }
  ],
  "summary": {
    "totalTeams": 2,
    "avgCoverageScore": 0.83
  }
}
```

---

## Quick Start and Setup

### Prerequisites

- [Node.js](https://nodejs.org/) v18 or higher
- [Python](https://www.python.org/) 3.9 or higher (required for the Python matchmaking microservice)
- MongoDB is optional. If not installed, TeamWeave automatically starts a persistent embedded MongoDB instance.

### 1. Clone and Install Dependencies

```bash
git clone https://github.com/anshikagarg12/TeamWeave.git
cd TeamWeave
npm install
pip install -r requirements.txt
```

### 2. Configure Environment

Copy `.env.example` to `.env`:

```bash
cp .env.example .env
```

Leave `MONGODB_URI` commented out to use the built-in embedded database.

### 3. Start the Python Matchmaking Microservice

```bash
python app.py
```

This starts the FastAPI service on `http://localhost:8000`. The Node.js backend will delegate team formation to this service automatically because `PYTHON_MATCHMAKER_URL=http://localhost:8000` is set in `.env`.

### 4. Start the Node.js Server

In a separate terminal:

```bash
npm start
```

For development with auto-reload on file changes:

```bash
npm run dev
```

### 5. Open in Browser

Visit [np](http://localhost:3000).

---

## Configuration (.env)

| Variable | Default | Description |
| :--- | :--- | :--- |
| `PORT` | `3000` | Port for the Express server |
| `MONGODB_URI` | *(unset)* | MongoDB connection string. Leave unset to use the embedded persistent instance. |
| `PYTHON_MATCHMAKER_URL` | `http://localhost:8000` | URL of the Python FastAPI microservice. Remove or comment out to use the JS fallback. |

---

## Python Matchmaking Microservice

`app.py` is a standalone FastAPI service that implements the Coverage-Maximizing Greedy algorithm with Coverage Gap Swap Optimiser in Python.

**Endpoints:**

| Method | Endpoint | Description |
| :--- | :--- | :--- |
| `GET` | `/health` | Liveness probe |
| `GET` | `/pillars` | Returns the full-stack pillar definitions and their keyword lists |
| `POST` | `/matchmake` | Accepts a participant list and team size, returns formed teams with coverage metadata |

**Request body for `POST /matchmake`:**

```json
{
  "participants": [
    {
      "user_id": "mongo-object-id-string",
      "name": "Alice Johnson",
      "email": "alice@example.com",
      "skills": [
        { "skillName": "React", "category": "Frontend", "proficiencyLevel": 5 }
      ]
    }
  ],
  "team_size": 4
}
```

**Response:**

```json
{
  "teams": [
    {
      "team_id": "uuid",
      "user_ids": ["..."],
      "member_names": ["Alice Johnson"],
      "coverage_score": 0.833,
      "covered_pillars": ["Frontend", "Backend", "Database/APIs", "Version Control", "Design"],
      "missing_pillars": ["DevOps/Deployment"]
    }
  ],
  "total_participants": 6,
  "total_teams": 2,
  "avg_coverage_score": 0.833,
  "algorithm": "CoverageMaximizingGreedy+CoverageGapSwap"
}
```

The Node.js `ClusteringService` automatically calls this endpoint. If the service is unavailable or `PYTHON_MATCHMAKER_URL` is not set, the request falls back to the built-in JS engine transparently.

Interactive API documentation is available at [http://localhost:8000/docs](http://localhost:8000/docs) when the Python service is running.

---

## Running Automated Tests

TeamWeave includes an end-to-end integration test suite verifying MongoDB connections, participant registration, team formation, member rebalancing, team locking, and audit trail generation:

```bash
npm run test:db
```

---

## Project Directory Structure

```
TeamWeave/
├── config/
│   └── db.js                  # MongoDB connection with embedded memory-server fallback
├── Models/
│   ├── Participant.js         # Participant schema with embedded skill ratings
│   ├── Team.js                # Team schema with member rosters and coverage metadata
│   ├── Skill.js               # Technology skill catalog schema
│   ├── AuditLog.js            # Audit trail schema
│   └── ClusteringRun.js       # Algorithm execution history schema
├── Services/
│   ├── SkillVectorEncoder.js  # Vector math, pillar detection, Euclidean distance
│   ├── ClusteringService.js   # Greedy + swap clustering engine and Python bridge
│   ├── ParticipantService.js  # Participant CRUD and MongoDB queries
│   └── TeamService.js         # Team locking, member moves, CSV exports, audit logs
├── scripts/
│   ├── seed.js                # Database seeder (24 skills and demo participants)
│   └── testDb.js              # End-to-end integration test runner
├── data/
│   └── db/                    # Persistent embedded MongoDB storage directory
├── app.py                     # Python FastAPI matchmaking microservice
├── requirements.txt           # Python dependencies (fastapi, uvicorn, pydantic)
├── server.js                  # Express API server and route definitions
├── index.html                 # Frontend portal (served by Express)
├── package.json               # Node.js manifest and npm scripts
├── .env / .env.example        # Environment variable configuration
└── README.md                  # Project documentation
```

---

## License

This project is open-source under the [MIT License](LICENSE).
