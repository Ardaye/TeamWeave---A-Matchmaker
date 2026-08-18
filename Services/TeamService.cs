using System;
using System.Collections.Generic;
using System.Linq;
using TeamWeave.Models;

namespace TeamWeave.Services
{
    public class TeamService
    {
        private readonly List<Team> _teams = new List<Team>();
        private readonly List<AuditLog> _auditLogs = new List<AuditLog>();
        private int _nextTeamId = 1;
        private int _nextAuditLogId = 1;

        /// <summary>
        /// Creates a new team.
        /// </summary>
        public Team CreateTeam(string name, int eventId)
        {
            var team = new Team
            {
                Id = _nextTeamId++,
                Name = name,
                EventId = eventId,
                Status = "Draft",
                IsLocked = false
            };

            _teams.Add(team);
            return team;
        }

        /// <summary>
        /// Adds a participant to a team.
        /// </summary>
        public void AddParticipantToTeam(int teamId, Participant participant)
        {
            var team = GetTeamById(teamId);
            if (team == null)
                throw new InvalidOperationException("Team not found");

            if (team.IsLocked)
                throw new InvalidOperationException("Cannot modify locked team");

            var teamMember = new TeamMember
            {
                TeamId = teamId,
                ParticipantId = participant.Id,
                IsLocked = false,
                Participant = participant,
                Team = team
            };

            team.Members.Add(teamMember);
            participant.Teams.Add(teamMember);
            team.UpdatedAt = DateTime.Now;

            // Log the action
            LogAuditEvent("ParticipantAdded", teamId, participant.Id, null, participant.Name, "System");
        }

        /// <summary>
        /// Moves a participant from one team to another.
        /// </summary>
        public void MoveParticipantBetweenTeams(int participantId, int fromTeamId, int toTeamId)
        {
            var fromTeam = GetTeamById(fromTeamId);
            var toTeam = GetTeamById(toTeamId);

            if (fromTeam == null || toTeam == null)
                throw new InvalidOperationException("One or both teams not found");

            if (fromTeam.IsLocked || toTeam.IsLocked)
                throw new InvalidOperationException("Cannot modify locked teams");

            var memberToMove = fromTeam.Members.FirstOrDefault(m => m.ParticipantId == participantId);
            if (memberToMove == null)
                throw new InvalidOperationException("Participant not found in source team");

            // Remove from source team
            fromTeam.Members.Remove(memberToMove);
            fromTeam.UpdatedAt = DateTime.Now;

            // Add to destination team
            var newMember = new TeamMember
            {
                TeamId = toTeamId,
                ParticipantId = participantId,
                IsLocked = false,
                Participant = memberToMove.Participant,
                Team = toTeam
            };

            toTeam.Members.Add(newMember);
            toTeam.UpdatedAt = DateTime.Now;

            // Log the action
            LogAuditEvent("ParticipantMoved", toTeamId, participantId, 
                fromTeam.Name, toTeam.Name, "System");
        }

        /// <summary>
        /// Locks a team to prevent further modifications.
        /// </summary>
        public void LockTeam(int teamId, string actor = "System")
        {
            var team = GetTeamById(teamId);
            if (team == null)
                throw new InvalidOperationException("Team not found");

            team.IsLocked = true;
            team.Status = "Locked";
            team.UpdatedAt = DateTime.Now;

            LogAuditEvent("TeamLocked", teamId, null, "false", "true", actor);
        }

        /// <summary>
        /// Unlocks a team to allow modifications.
        /// </summary>
        public void UnlockTeam(int teamId, string actor = "System")
        {
            var team = GetTeamById(teamId);
            if (team == null)
                throw new InvalidOperationException("Team not found");

            team.IsLocked = false;
            team.Status = "Draft";
            team.UpdatedAt = DateTime.Now;

            LogAuditEvent("TeamUnlocked", teamId, null, "true", "false", actor);
        }

        /// <summary>
        /// Locks a specific participant in a team.
        /// </summary>
        public void LockParticipantInTeam(int teamId, int participantId)
        {
            var team = GetTeamById(teamId);
            if (team == null)
                throw new InvalidOperationException("Team not found");

            var member = team.Members.FirstOrDefault(m => m.ParticipantId == participantId);
            if (member == null)
                throw new InvalidOperationException("Participant not found in team");

            member.IsLocked = true;
            team.UpdatedAt = DateTime.Now;

            LogAuditEvent("ParticipantLocked", teamId, participantId, "false", "true", "System");
        }

        /// <summary>
        /// Retrieves a team by ID.
        /// </summary>
        public Team GetTeamById(int id)
        {
            foreach (var team in _teams)
            {
                if (team.Id == id)
                    return team;
            }
            return null;
        }

        /// <summary>
        /// Retrieves all teams.
        /// </summary>
        public List<Team> GetAllTeams()
        {
            return new List<Team>(_teams);
        }

        /// <summary>
        /// Retrieves all audit logs.
        /// </summary>
        public List<AuditLog> GetAuditLogs()
        {
            return new List<AuditLog>(_auditLogs);
        }

        /// <summary>
        /// Retrieves audit logs for a specific team.
        /// </summary>
        public List<AuditLog> GetAuditLogsForTeam(int teamId)
        {
            return _auditLogs.Where(log => log.TeamId == teamId).ToList();
        }

        private void LogAuditEvent(string changeType, int? teamId, int? participantId, 
            string oldValue, string newValue, string actor)
        {
            var auditLog = new AuditLog
            {
                Id = _nextAuditLogId++,
                ChangeType = changeType,
                TeamId = teamId,
                ParticipantId = participantId ?? 0,
                OldValue = oldValue,
                NewValue = newValue,
                Actor = actor,
                Timestamp = DateTime.Now
            };

            _auditLogs.Add(auditLog);
        }
    }
}
