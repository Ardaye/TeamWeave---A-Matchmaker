using System;
using System.Collections.Generic;

namespace TeamWeave.Models
{
    public class Participant
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string ProfileStatus { get; set; } // "Submitted", "Approved", etc.
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public virtual ICollection<ParticipantSkill> Skills { get; set; }
        public virtual ICollection<TeamMember> Teams { get; set; }
        public virtual ICollection<string> Interests { get; set; }

        public Participant()
        {
            Skills = new HashSet<ParticipantSkill>();
            Teams = new HashSet<TeamMember>();
            Interests = new HashSet<string>();
            ProfileStatus = "Submitted";
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
        }
    }
}
