using System;

namespace TeamWeave.Models
{
    public class ParticipantSkill
    {
        public int Id { get; set; }
        public int ParticipantId { get; set; }
        public int SkillId { get; set; }
        public int ProficiencyLevel { get; set; } // 1-5 scale
        public DateTime AddedAt { get; set; }

        public virtual Participant Participant { get; set; }
        public virtual Skill Skill { get; set; }

        public ParticipantSkill()
        {
            AddedAt = DateTime.Now;
            ProficiencyLevel = 3; // Default to medium proficiency
        }
    }
}
