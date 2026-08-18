using System;
using System.Collections.Generic;

namespace TeamWeave.Models
{
    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int EventId { get; set; }
        public string Status { get; set; } // "Draft", "Suggested", "Locked", "Finalized"
        public bool IsLocked { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public virtual ICollection<TeamMember> Members { get; set; }

        public Team()
        {
            Members = new HashSet<TeamMember>();
            Status = "Draft";
            IsLocked = false;
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
        }
    }
}
