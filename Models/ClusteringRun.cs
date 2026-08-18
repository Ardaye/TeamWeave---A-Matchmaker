using System;
using System.Collections.Generic;

namespace TeamWeave.Models
{
    public class ClusteringRun
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public DateTime ExecutedAt { get; set; }
        public int TeamsGenerated { get; set; }
        public string Parameters { get; set; } // JSON serialized clustering parameters
        public string Status { get; set; } // "Completed", "Failed", etc.

        public virtual ICollection<Team> Teams { get; set; }

        public ClusteringRun()
        {
            ExecutedAt = DateTime.Now;
            Teams = new HashSet<Team>();
            Status = "Completed";
        }
    }
}
