using System;
using System.Collections.Generic;
using System.Linq;
using TeamWeave.Models;

namespace TeamWeave.Services
{
    public class ClusteringService
    {
        private readonly Random _random = new Random();

        /// <summary>
        /// Performs K-means clustering on participants based on their skill vectors.
        /// </summary>
        public List<List<Participant>> ClusterParticipants(List<Participant> participants, 
            int teamSize, int maxIterations = 100)
        {
            if (participants.Count == 0)
                return new List<List<Participant>>();

            int numClusters = Math.Max(1, participants.Count / teamSize);

            // Initialize centroids by randomly selecting participants
            var centroids = InitializeCentroids(participants, numClusters);
            var assignments = new Dictionary<int, int>(); // participant index -> cluster id

            // K-means iterations
            for (int iteration = 0; iteration < maxIterations; iteration++)
            {
                // Assign participants to nearest centroid
                AssignToClusters(participants, centroids, assignments);

                // Calculate new centroids
                var newCentroids = CalculateNewCentroids(participants, assignments, numClusters);

                // Check for convergence
                if (CentroidsConverged(centroids, newCentroids))
                    break;

                centroids = newCentroids;
            }

            // Group participants by cluster
            return GroupParticipantsByClusters(participants, assignments, numClusters);
        }

        /// <summary>
        /// Performs constraint-based team balancing to ensure diversity.
        /// </summary>
        public List<List<Participant>> BalanceTeamsBySkillDiversity(List<Participant> participants, 
            int teamSize, Dictionary<string, int> requiredSkillCategories = null)
        {
            var teams = new List<List<Participant>>();
            var availableParticipants = new List<Participant>(participants);

            while (availableParticipants.Count >= teamSize)
            {
                var team = SelectBalancedTeam(availableParticipants, teamSize, requiredSkillCategories);
                if (team.Count > 0)
                {
                    teams.Add(team);
                    foreach (var p in team)
                        availableParticipants.Remove(p);
                }
                else
                {
                    break; // Unable to form a balanced team
                }
            }

            // Handle remaining participants (if any)
            if (availableParticipants.Count > 0)
                teams.Add(availableParticipants);

            return teams;
        }

        private List<double[]> InitializeCentroids(List<Participant> participants, int numClusters)
        {
            var centroids = new List<double[]>();
            var skillIndex = BuildSkillIndex(participants);

            for (int i = 0; i < numClusters; i++)
            {
                int randomIndex = _random.Next(participants.Count);
                var skills = participants[randomIndex].Skills
                    .Select(ps => (ps.Skill.Name, ps.ProficiencyLevel))
                    .ToList();

                var vector = SkillVectorEncoder.EncodeParticipantSkills(skills, skillIndex);
                centroids.Add(vector);
            }

            return centroids;
        }

        private void AssignToClusters(List<Participant> participants, List<double[]> centroids, 
            Dictionary<int, int> assignments)
        {
            var skillIndex = BuildSkillIndex(participants);
            assignments.Clear();

            for (int i = 0; i < participants.Count; i++)
            {
                var skills = participants[i].Skills
                    .Select(ps => (ps.Skill.Name, ps.ProficiencyLevel))
                    .ToList();

                var vector = SkillVectorEncoder.EncodeParticipantSkills(skills, skillIndex);

                double minDistance = double.MaxValue;
                int nearestCluster = 0;

                for (int c = 0; c < centroids.Count; c++)
                {
                    double distance = SkillVectorEncoder.EuclideanDistance(vector, centroids[c]);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearestCluster = c;
                    }
                }

                assignments[i] = nearestCluster;
            }
        }

        private List<double[]> CalculateNewCentroids(List<Participant> participants, 
            Dictionary<int, int> assignments, int numClusters)
        {
            var skillIndex = BuildSkillIndex(participants);
            var centroids = new List<double[]>();

            for (int c = 0; c < numClusters; c++)
            {
                var clusterMembers = new List<double[]>();

                for (int i = 0; i < participants.Count; i++)
                {
                    if (assignments.TryGetValue(i, out int cluster) && cluster == c)
                    {
                        var skills = participants[i].Skills
                            .Select(ps => (ps.Skill.Name, ps.ProficiencyLevel))
                            .ToList();

                        var vector = SkillVectorEncoder.EncodeParticipantSkills(skills, skillIndex);
                        clusterMembers.Add(vector);
                    }
                }

                if (clusterMembers.Count > 0)
                {
                    var centroid = CalculateMean(clusterMembers);
                    centroids.Add(centroid);
                }
                else
                {
                    // Empty cluster - reinitialize with random vector
                    centroids.Add(new double[skillIndex.Count]);
                }
            }

            return centroids;
        }

        private double[] CalculateMean(List<double[]> vectors)
        {
            if (vectors.Count == 0)
                return new double[0];

            int dimension = vectors[0].Length;
            double[] mean = new double[dimension];

            foreach (var vector in vectors)
            {
                for (int i = 0; i < dimension; i++)
                {
                    mean[i] += vector[i];
                }
            }

            for (int i = 0; i < dimension; i++)
            {
                mean[i] /= vectors.Count;
            }

            return mean;
        }

        private bool CentroidsConverged(List<double[]> oldCentroids, List<double[]> newCentroids, 
            double threshold = 0.0001)
        {
            if (oldCentroids.Count != newCentroids.Count)
                return false;

            for (int i = 0; i < oldCentroids.Count; i++)
            {
                double distance = SkillVectorEncoder.EuclideanDistance(oldCentroids[i], newCentroids[i]);
                if (distance > threshold)
                    return false;
            }

            return true;
        }

        private List<List<Participant>> GroupParticipantsByClusters(List<Participant> participants, 
            Dictionary<int, int> assignments, int numClusters)
        {
            var groups = new List<List<Participant>>();

            for (int c = 0; c < numClusters; c++)
            {
                var cluster = new List<Participant>();
                for (int i = 0; i < participants.Count; i++)
                {
                    if (assignments.TryGetValue(i, out int clusterAssignment) && clusterAssignment == c)
                        cluster.Add(participants[i]);
                }
                if (cluster.Count > 0)
                    groups.Add(cluster);
            }

            return groups;
        }

        private List<Participant> SelectBalancedTeam(List<Participant> availableParticipants, 
            int teamSize, Dictionary<string, int> requiredSkillCategories)
        {
            var team = new List<Participant>();

            // Sort by skill diversity score (descending)
            var sorted = availableParticipants
                .OrderByDescending(p => CalculateSkillDiversityScore(p))
                .ToList();

            // Greedily select participants ensuring skill balance
            foreach (var participant in sorted)
            {
                if (team.Count < teamSize)
                {
                    team.Add(participant);

                    // Check if team requirements are satisfied
                    if (team.Count == teamSize && IsTeamBalanced(team, requiredSkillCategories))
                        break;
                }
            }

            return team.Count == teamSize ? team : new List<Participant>();
        }

        private double CalculateSkillDiversityScore(Participant participant)
        {
            var categories = new HashSet<string>();
            double totalProficiency = 0;

            foreach (var skill in participant.Skills)
            {
                categories.Add(skill.Skill.Category);
                totalProficiency += skill.ProficiencyLevel;
            }

            // Score = number of categories * average proficiency
            return categories.Count * (totalProficiency / (participant.Skills.Count > 0 ? participant.Skills.Count : 1));
        }

        private bool IsTeamBalanced(List<Participant> team, Dictionary<string, int> requiredSkillCategories)
        {
            if (requiredSkillCategories == null || requiredSkillCategories.Count == 0)
                return true;

            var teamSkillCategories = new HashSet<string>();
            foreach (var member in team)
            {
                foreach (var skill in member.Skills)
                {
                    teamSkillCategories.Add(skill.Skill.Category);
                }
            }

            // Check if team has at least one of each required category
            foreach (var requiredCategory in requiredSkillCategories.Keys)
            {
                if (!teamSkillCategories.Contains(requiredCategory))
                    return false;
            }

            return true;
        }

        private Dictionary<string, int> BuildSkillIndex(List<Participant> participants)
        {
            var skillIndex = new Dictionary<string, int>();
            int index = 0;

            foreach (var participant in participants)
            {
                foreach (var skill in participant.Skills)
                {
                    if (!skillIndex.ContainsKey(skill.Skill.Name))
                    {
                        skillIndex[skill.Skill.Name] = index++;
                    }
                }
            }

            return skillIndex;
        }
    }
}
