using System;
using System.Collections.Generic;
using TeamWeave.Models;

namespace TeamWeave.Services
{
    public class ParticipantService
    {
        private readonly List<Participant> _participants = new List<Participant>();
        private int _nextId = 1;

        /// <summary>
        /// Registers a new participant.
        /// </summary>
        public Participant RegisterParticipant(string name, string email, List<string> skillNames)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Name and email are required");

            if (skillNames == null || skillNames.Count == 0)
                throw new ArgumentException("At least one skill is required");

            var participant = new Participant
            {
                Id = _nextId++,
                Name = name,
                Email = email,
                ProfileStatus = "Submitted"
            };

            _participants.Add(participant);
            return participant;
        }

        /// <summary>
        /// Adds a skill to a participant.
        /// </summary>
        public void AddSkillToParticipant(int participantId, Skill skill, int proficiencyLevel)
        {
            var participant = GetParticipantById(participantId);
            if (participant == null)
                throw new InvalidOperationException("Participant not found");

            if (proficiencyLevel < 1 || proficiencyLevel > 5)
                throw new ArgumentException("Proficiency level must be between 1 and 5");

            var participantSkill = new ParticipantSkill
            {
                ParticipantId = participantId,
                SkillId = skill.Id,
                ProficiencyLevel = proficiencyLevel,
                Skill = skill,
                Participant = participant
            };

            participant.Skills.Add(participantSkill);
            participant.UpdatedAt = DateTime.Now;
        }

        /// <summary>
        /// Adds interests to a participant.
        /// </summary>
        public void AddInterestsToParticipant(int participantId, List<string> interests)
        {
            var participant = GetParticipantById(participantId);
            if (participant == null)
                throw new InvalidOperationException("Participant not found");

            foreach (var interest in interests)
            {
                participant.Interests.Add(interest);
            }

            participant.UpdatedAt = DateTime.Now;
        }

        /// <summary>
        /// Retrieves a participant by ID.
        /// </summary>
        public Participant GetParticipantById(int id)
        {
            foreach (var participant in _participants)
            {
                if (participant.Id == id)
                    return participant;
            }
            return null;
        }

        /// <summary>
        /// Retrieves all participants.
        /// </summary>
        public List<Participant> GetAllParticipants()
        {
            return new List<Participant>(_participants);
        }

        /// <summary>
        /// Retrieves participants by skill category.
        /// </summary>
        public List<Participant> GetParticipantsBySkillCategory(string category)
        {
            var result = new List<Participant>();

            foreach (var participant in _participants)
            {
                foreach (var skill in participant.Skills)
                {
                    if (skill.Skill.Category == category)
                    {
                        result.Add(participant);
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Updates participant profile status.
        /// </summary>
        public void UpdateParticipantStatus(int participantId, string newStatus)
        {
            var participant = GetParticipantById(participantId);
            if (participant == null)
                throw new InvalidOperationException("Participant not found");

            participant.ProfileStatus = newStatus;
            participant.UpdatedAt = DateTime.Now;
        }
    }
}
