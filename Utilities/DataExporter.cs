using System;
using System.Collections.Generic;
using System.Text;

namespace TeamWeave.Utilities
{
    public class DataExporter
    {
        /// <summary>
        /// Exports participant and team data to CSV format.
        /// </summary>
        public static string ExportParticipantsToCSV(List<Models.Participant> participants)
        {
            var csv = new StringBuilder();
            csv.AppendLine("ID,Name,Email,Profile Status,Skills,Created At");

            foreach (var participant in participants)
            {
                var skills = string.Join(";", participant.Skills);
                csv.AppendLine($"{participant.Id},{participant.Name},{participant.Email},{participant.ProfileStatus},{skills},{participant.CreatedAt:yyyy-MM-dd HH:mm:ss}");
            }

            return csv.ToString();
        }

        /// <summary>
        /// Exports team assignments to CSV format.
        /// </summary>
        public static string ExportTeamsToCSV(List<Models.Team> teams)
        {
            var csv = new StringBuilder();
            csv.AppendLine("Team ID,Team Name,Status,Is Locked,Members Count,Created At");

            foreach (var team in teams)
            {
                csv.AppendLine($"{team.Id},{team.Name},{team.Status},{team.IsLocked},{team.Members.Count},{team.CreatedAt:yyyy-MM-dd HH:mm:ss}");
            }

            return csv.ToString();
        }

        /// <summary>
        /// Exports audit logs to CSV format.
        /// </summary>
        public static string ExportAuditLogsToCSV(List<Models.AuditLog> auditLogs)
        {
            var csv = new StringBuilder();
            csv.AppendLine("ID,Change Type,Team ID,Participant ID,Old Value,New Value,Actor,Timestamp");

            foreach (var log in auditLogs)
            {
                csv.AppendLine($"{log.Id},{log.ChangeType},{log.TeamId},{log.ParticipantId},{log.OldValue},{log.NewValue},{log.Actor},{log.Timestamp:yyyy-MM-dd HH:mm:ss}");
            }

            return csv.ToString();
        }
    }
}
