using System;
using System.Collections.Generic;
using System.Linq;

namespace TeamWeave.Services
{
    public class SkillVectorEncoder
    {
        /// <summary>
        /// Encodes a participant's skills into a numerical vector representation.
        /// </summary>
        public static double[] EncodeParticipantSkills(List<(string skillName, int proficiency)> skills, 
            Dictionary<string, int> skillIndex)
        {
            double[] vector = new double[skillIndex.Count];

            foreach (var (skillName, proficiency) in skills)
            {
                if (skillIndex.TryGetValue(skillName, out int index))
                {
                    vector[index] = proficiency / 5.0; // Normalize to 0-1 range
                }
            }

            return vector;
        }

        /// <summary>
        /// Calculates Euclidean distance between two skill vectors.
        /// </summary>
        public static double EuclideanDistance(double[] vector1, double[] vector2)
        {
            if (vector1.Length != vector2.Length)
                throw new ArgumentException("Vectors must have the same dimension");

            double sum = 0;
            for (int i = 0; i < vector1.Length; i++)
            {
                sum += Math.Pow(vector1[i] - vector2[i], 2);
            }

            return Math.Sqrt(sum);
        }

        /// <summary>
        /// Calculates cosine similarity between two skill vectors.
        /// </summary>
        public static double CosineSimilarity(double[] vector1, double[] vector2)
        {
            if (vector1.Length != vector2.Length)
                throw new ArgumentException("Vectors must have the same dimension");

            double dotProduct = 0;
            double magnitude1 = 0;
            double magnitude2 = 0;

            for (int i = 0; i < vector1.Length; i++)
            {
                dotProduct += vector1[i] * vector2[i];
                magnitude1 += Math.Pow(vector1[i], 2);
                magnitude2 += Math.Pow(vector2[i], 2);
            }

            magnitude1 = Math.Sqrt(magnitude1);
            magnitude2 = Math.Sqrt(magnitude2);

            if (magnitude1 == 0 || magnitude2 == 0)
                return 0;

            return dotProduct / (magnitude1 * magnitude2);
        }

        /// <summary>
        /// Builds a skill category distribution (e.g., how many frontend, backend, data skills).
        /// </summary>
        public static Dictionary<string, int> GetSkillCategoryDistribution(
            List<(string skillName, string category, int proficiency)> skills)
        {
            var distribution = new Dictionary<string, int>();

            foreach (var (_, category, proficiency) in skills)
            {
                if (!distribution.ContainsKey(category))
                    distribution[category] = 0;

                distribution[category] += proficiency;
            }

            return distribution;
        }
    }
}
