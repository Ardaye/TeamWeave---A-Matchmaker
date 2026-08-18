using System;
using System.Collections.Generic;

namespace TeamWeave.Models
{
    public class Skill
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; } // "Frontend", "Backend", "Data", "Design", etc.
        public DateTime CreatedAt { get; set; }

        public virtual ICollection<ParticipantSkill> ParticipantSkills { get; set; }

        public Skill()
        {
            ParticipantSkills = new HashSet<ParticipantSkill>();
            CreatedAt = DateTime.Now;
        }
    }
}
