using System;

namespace TeamWeave.Models
{
    public class TeamMember
    {
        public int Id { get; set; }
        public int TeamId { get; set; }
        public int ParticipantId { get; set; }
        public bool IsLocked { get; set; } // Individual lock flag
        public DateTime AddedAt { get; set; }

        public virtual Team Team { get; set; }
        public virtual Participant Participant { get; set; }

        public TeamMember()
        {
            IsLocked = false;
            AddedAt = DateTime.Now;
        }
    }
}
