/**
 * SkillVectorEncoder converts participant skill proficiencies into normalized numerical vectors
 * and calculates mathematical distances and similarities between participants.
 */
export class SkillVectorEncoder {
  /**
   * Encodes a participant's skills into a normalized vector ([0.0 - 1.0]).
   * @param {Array<{skillName: string, proficiencyLevel: number}>} skills 
   * @param {Map<string, number>} skillIndex 
   * @returns {number[]}
   */
  static encodeParticipantSkills(skills, skillIndex) {
    const vector = new Array(skillIndex.size).fill(0);

    for (const skill of skills) {
      if (skillIndex.has(skill.skillName)) {
        const index = skillIndex.get(skill.skillName);
        // Normalize 1-5 scale to 0.2-1.0
        vector[index] = (skill.proficiencyLevel || 3) / 5.0;
      }
    }

    return vector;
  }

  /**
   * Calculates Euclidean distance between two vectors: sqrt(sum((a_i - b_i)^2))
   * @param {number[]} v1 
   * @param {number[]} v2 
   * @returns {number}
   */
  static euclideanDistance(v1, v2) {
    if (v1.length !== v2.length) {
      throw new Error(`Vector dimensions must match: ${v1.length} vs ${v2.length}`);
    }

    let sum = 0;
    for (let i = 0; i < v1.length; i++) {
      const diff = v1[i] - v2[i];
      sum += diff * diff;
    }

    return Math.sqrt(sum);
  }

  /**
   * Calculates Cosine Similarity between two vectors: (a . b) / (||a|| * ||b||)
   * @param {number[]} v1 
   * @param {number[]} v2 
   * @returns {number}
   */
  static cosineSimilarity(v1, v2) {
    if (v1.length !== v2.length) {
      throw new Error(`Vector dimensions must match: ${v1.length} vs ${v2.length}`);
    }

    let dotProduct = 0;
    let mag1 = 0;
    let mag2 = 0;

    for (let i = 0; i < v1.length; i++) {
      dotProduct += v1[i] * v2[i];
      mag1 += v1[i] * v1[i];
      mag2 += v2[i] * v2[i];
    }

    mag1 = Math.sqrt(mag1);
    mag2 = Math.sqrt(mag2);

    if (mag1 === 0 || mag2 === 0) return 0;
    return dotProduct / (mag1 * mag2);
  }

  /**
   * Builds an index map of all distinct skill names across all participants.
   * @param {Array} participants 
   * @returns {Map<string, number>}
   */
  static buildSkillIndex(participants) {
    const skillIndex = new Map();
    let index = 0;

    for (const participant of participants) {
      for (const skill of participant.skills || []) {
        if (!skillIndex.has(skill.skillName)) {
          skillIndex.set(skill.skillName, index++);
        }
      }
    }

    return skillIndex;
  }
}
