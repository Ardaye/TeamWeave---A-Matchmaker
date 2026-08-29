import { SkillVectorEncoder } from './SkillVectorEncoder.js';
import { Team } from '../Models/Team.js';
import { ClusteringRun } from '../Models/ClusteringRun.js';
import { AuditLog } from '../Models/AuditLog.js';

/**
 * URL of the Python FastAPI matchmaking microservice.
 * Set PYTHON_MATCHMAKER_URL in your environment to enable it.
 * Falls back to the built-in JS K-Means when unset or unreachable.
 *
 * Example .env entry:
 *   PYTHON_MATCHMAKER_URL=http://localhost:8000
 */
const PYTHON_MATCHMAKER_URL = process.env.PYTHON_MATCHMAKER_URL || null;

export class ClusteringService {
  /**
   * Performs K-Means clustering on participant skill vectors.
   * Used as the local fallback when the Python microservice is unavailable.
   *
   * @param {Array} participants
   * @param {number} teamSize
   * @param {number} maxIterations
   * @returns {Array<Array<Object>>} Array of team clusters
   */
  static clusterParticipants(participants, teamSize = 4, maxIterations = 100) {
    if (!participants || participants.length === 0) {
      return [];
    }

    const numClusters = Math.max(1, Math.ceil(participants.length / teamSize));
    const skillIndex = SkillVectorEncoder.buildSkillIndex(participants);

    if (skillIndex.size === 0) {
      // Fallback simple grouping if no skills are defined
      return this._chunkArray(participants, teamSize);
    }

    // Convert participants to vectors
    const participantVectors = participants.map(p =>
      SkillVectorEncoder.encodeParticipantSkills(p.skills || [], skillIndex)
    );

    // Initialize Centroids randomly from participant vectors
    const centroids = [];
    const usedIndices = new Set();
    for (let i = 0; i < numClusters; i++) {
      let idx;
      do {
        idx = Math.floor(Math.random() * participants.length);
      } while (usedIndices.has(idx) && usedIndices.size < participants.length);
      usedIndices.add(idx);
      centroids.push([...participantVectors[idx]]);
    }

    let assignments = new Array(participants.length).fill(0);

    // K-Means iterations
    for (let iter = 0; iter < maxIterations; iter++) {
      let changed = false;

      // Assign each participant to nearest centroid
      for (let i = 0; i < participants.length; i++) {
        let minDistance = Infinity;
        let nearestCluster = 0;

        for (let c = 0; c < centroids.length; c++) {
          const dist = SkillVectorEncoder.euclideanDistance(participantVectors[i], centroids[c]);
          if (dist < minDistance) {
            minDistance = dist;
            nearestCluster = c;
          }
        }

        if (assignments[i] !== nearestCluster) {
          assignments[i] = nearestCluster;
          changed = true;
        }
      }

      if (!changed) break; // Converged

      // Recalculate centroids
      for (let c = 0; c < numClusters; c++) {
        const clusterMembers = [];
        for (let i = 0; i < participants.length; i++) {
          if (assignments[i] === c) {
            clusterMembers.push(participantVectors[i]);
          }
        }

        if (clusterMembers.length > 0) {
          const newCentroid = new Array(skillIndex.size).fill(0);
          for (const vec of clusterMembers) {
            for (let d = 0; d < skillIndex.size; d++) {
              newCentroid[d] += vec[d];
            }
          }
          for (let d = 0; d < skillIndex.size; d++) {
            newCentroid[d] /= clusterMembers.length;
          }
          centroids[c] = newCentroid;
        }
      }
    }

    // Group participants by cluster
    const clusters = Array.from({ length: numClusters }, () => []);
    for (let i = 0; i < participants.length; i++) {
      clusters[assignments[i]].push(participants[i]);
    }

    // Rebalance clusters to respect team size constraints
    return this._balanceClusterSizes(clusters.filter(c => c.length > 0), teamSize);
  }

  /**
   * Calls the Python FastAPI matchmaking microservice (app.py).
   *
   * The Python service accepts the same MongoDB Participant shape used
   * internally, mapped as:
   *   user_id  ← participant._id.toString()
   *   name     ← participant.name
   *   email    ← participant.email
   *   skills   ← participant.skills (array of { skillName, category, proficiencyLevel })
   *
   * Returns an array of participant arrays (same shape as clusterParticipants),
   * or null if the service is unreachable so the caller can fall back.
   *
   * @param {Array} participants  MongoDB Participant documents
   * @param {Map}   idMap         Map from user_id string → Participant document
   * @returns {Array<Array<Object>>|null}
   */
  static async _callPythonMatchmaker(participants, idMap) {
    const payload = participants.map(p => ({
      user_id: p._id.toString(),
      name: p.name,
      email: p.email || '',
      skills: (p.skills || []).map(s => ({
        skillName: s.skillName,
        category: s.category,
        proficiencyLevel: s.proficiencyLevel ?? 3,
      })),
    }));

    const url = `${PYTHON_MATCHMAKER_URL}/matchmake`;
    console.log(`[ClusteringService] Calling Python matchmaker at ${url} …`);

    try {
      const response = await fetch(url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload),
        signal: AbortSignal.timeout(10_000), // 10 s timeout
      });

      if (!response.ok) {
        const err = await response.text();
        throw new Error(`Python matchmaker responded ${response.status}: ${err}`);
      }

      const data = await response.json();

      // Reconstruct participant arrays from the returned user_ids
      return data.teams.map(team =>
        team.user_ids.map(uid => idMap.get(uid)).filter(Boolean)
      );
    } catch (err) {
      console.warn(
        `[ClusteringService] Python matchmaker unavailable (${err.message}). ` +
        'Falling back to JS K-Means.'
      );
      return null;
    }
  }

  /**
   * Helper to ensure no single cluster exceeds teamSize or stays empty.
   */
  static _balanceClusterSizes(clusters, teamSize) {
    const flat = clusters.flat();
    const result = [];
    for (let i = 0; i < flat.length; i += teamSize) {
      result.push(flat.slice(i, i + teamSize));
    }
    return result;
  }

  static _chunkArray(array, size) {
    const result = [];
    for (let i = 0; i < array.length; i += size) {
      result.push(array.slice(i, i + size));
    }
    return result;
  }

  /**
   * Runs the full clustering pipeline, wipes unfinalized/draft teams,
   * creates new Team documents, and records a ClusteringRun in MongoDB.
   *
   * Strategy:
   *   1. If PYTHON_MATCHMAKER_URL is set, delegate to the Python
   *      microservice (role-stratified round-robin + Jaccard swap).
   *   2. Otherwise fall back to the local JS K-Means implementation.
   */
  static async executeAndPersistClustering(participants, teamSize = 4, eventId = 1) {
    const startTime = Date.now();

    // Build a lookup map for reconstructing Participant docs from user_id strings
    const idMap = new Map(participants.map(p => [p._id.toString(), p]));

    // 1. Choose algorithm: Python microservice or local K-Means fallback
    let clusters = null;
    let algorithmUsed = 'KMeans_SkillVector';

    if (PYTHON_MATCHMAKER_URL) {
      clusters = await this._callPythonMatchmaker(participants, idMap);
      if (clusters) {
        algorithmUsed = 'RoleStratifiedRoundRobin+JaccardSwap';
        console.log(
          `[ClusteringService] Python matchmaker returned ${clusters.length} teams.`
        );
      }
    }

    // Fallback to local K-Means if Python service was not used / unreachable
    if (!clusters) {
      clusters = this.clusterParticipants(participants, teamSize);
    }

    // 2. Remove previous unlocked/draft teams for this event
    await Team.deleteMany({ eventId, isLocked: false });

    // 3. Persist new teams in MongoDB
    const createdTeams = [];
    for (let i = 0; i < clusters.length; i++) {
      const cluster = clusters[i];
      const members = cluster.map(p => ({
        participantId: p._id,
        name: p.name,
        email: p.email,
        skills: p.skills || [],
        isLocked: false,
        addedAt: new Date(),
      }));

      const team = new Team({
        name: `Team ${i + 1}`,
        eventId: eventId,
        status: 'Draft',
        isLocked: false,
        members: members,
      });

      const savedTeam = await team.save();
      createdTeams.push(savedTeam);
    }

    const durationMs = Date.now() - startTime;

    // 4. Save ClusteringRun document in MongoDB
    const clusteringRun = new ClusteringRun({
      eventId,
      teamsGenerated: createdTeams.length,
      parameters: {
        targetTeamSize: teamSize,
        maxIterations: 100,
        algorithm: algorithmUsed,
      },
      teamIds: createdTeams.map(t => t._id),
      status: 'Completed',
      durationMs,
    });
    await clusteringRun.save();

    // 5. Create AuditLog in MongoDB
    await AuditLog.create({
      changeType: 'ClusteringExecuted',
      actor: 'Organizer',
      details: `Generated ${createdTeams.length} teams via ${algorithmUsed} in ${durationMs}ms`,
    });

    return {
      teams: createdTeams,
      clusteringRun,
    };
  }
}
