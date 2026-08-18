using System;

namespace TeamWeave.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public int ParticipantId { get; set; }
        public int? TeamId { get; set; }
        public string ChangeType { get; set; } // "ParticipantAdded", "ParticipantMoved", "TeamLocked", etc.
        public string Actor { get; set; } // Admin/Organizer name or "System"
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public DateTime Timestamp { get; set; }

        public AuditLog()
        {
            Timestamp = DateTime.Now;
        }
    }
}
