# TeamWeave Developer Quick Reference

## 🚀 Quick Start

### Running the Demo
```bash
cd C:\Users\aryan\source\repos\TeamWeave\
dotnet build
dotnet run
```

### Project Structure
```
Models/          → Data entities (Participant, Team, Skill, etc.)
Services/        → Business logic (Clustering, Team Management)
Utilities/       → Helper classes (SkillRepository, DataExporter)
Program.cs       → Console demo application
```

---

## 📖 Common Tasks

### 1. Register a New Participant
```csharp
var participantService = new ParticipantService();

// Register
var participant = participantService.RegisterParticipant(
    "John Doe",
    "john@example.com",
    new List<string> { "React", "JavaScript" }
);

// Add skills with proficiency
var reactSkill = SkillRepository.GetSkillByName("React");
participantService.AddSkillToParticipant(participant.Id, reactSkill, 5);

// Add interests
participantService.AddInterestsToParticipant(
    participant.Id,
    new List<string> { "Web Development", "UI/UX" }
);
```

### 2. Run Clustering Algorithm
```csharp
var clusteringService = new ClusteringService();
var participants = participantService.GetAllParticipants();

// Cluster into teams of 4
var teams = clusteringService.ClusterParticipants(
    participants,
    teamSize: 4,
    maxIterations: 100
);

// Or use constraint-based balancing
var balancedTeams = clusteringService.BalanceTeamsBySkillDiversity(
    participants,
    teamSize: 4,
    requiredSkillCategories: new Dictionary<string, int> 
    {
        { "Frontend", 1 },
        { "Backend", 1 },
        { "Data", 1 }
    }
);
```

### 3. Create Teams from Clusters
```csharp
var teamService = new TeamService();
int eventId = 1;

var createdTeams = new List<Team>();
foreach (var cluster in teams)
{
    var team = teamService.CreateTeam($"Team {createdTeams.Count + 1}", eventId);
    foreach (var participant in cluster)
    {
        teamService.AddParticipantToTeam(team.Id, participant);
    }
    createdTeams.Add(team);
}
```

### 4. Move Participant Between Teams
```csharp
teamService.MoveParticipantBetweenTeams(
    participantId: 1,
    fromTeamId: 1,
    toTeamId: 2
);
```

### 5. Lock a Team
```csharp
// Lock entire team (prevents all modifications)
teamService.LockTeam(teamId: 1, actor: "John (Organizer)");

// Lock individual participant (can't move them)
teamService.LockParticipantInTeam(teamId: 1, participantId: 5);
```

### 6. View Audit Trail
```csharp
var auditLogs = teamService.GetAuditLogs();
foreach (var log in auditLogs)
{
    Console.WriteLine(
        $"[{log.Timestamp:yyyy-MM-dd HH:mm:ss}] " +
        $"{log.ChangeType} by {log.Actor}: " +
        $"{log.OldValue} → {log.NewValue}"
    );
}

// Get audit logs for specific team
var teamLogs = teamService.GetAuditLogsForTeam(teamId: 1);
```

### 7. Export Data
```csharp
var participants = participantService.GetAllParticipants();
var teams = teamService.GetAllTeams();
var auditLogs = teamService.GetAuditLogs();

// Export to CSV
var participantCsv = DataExporter.ExportParticipantsToCSV(participants);
var teamCsv = DataExporter.ExportTeamsToCSV(teams);
var auditCsv = DataExporter.ExportAuditLogsToCSV(auditLogs);

// Save to file
System.IO.File.WriteAllText("participants.csv", participantCsv);
System.IO.File.WriteAllText("teams.csv", teamCsv);
System.IO.File.WriteAllText("audit_log.csv", auditCsv);
```

### 8. Filter Participants by Skill
```csharp
// Get all participants with backend skills
var backendDevelopers = participantService.GetParticipantsBySkillCategory("Backend");

// Get all skills in a category
var frontendSkills = SkillRepository.GetSkillsByCategory("Frontend");

// Get all skill categories
var categories = SkillRepository.GetAllCategories();
```

---

## 🔢 Algorithm Reference

### Skill Vector Encoding
```csharp
// Normalizes skills to 0-1 range based on proficiency
var skillIndex = new Dictionary<string, int> { {"React", 0}, {"Node.js", 1} };
double[] vector = SkillVectorEncoder.EncodeParticipantSkills(
    skills: new List<(string, int)> { ("React", 5), ("Node.js", 3) },
    skillIndex: skillIndex
);
// Result: [1.0, 0.6]  (5/5=1.0, 3/5=0.6)
```

### Euclidean Distance
```csharp
double[] v1 = { 1.0, 0.6, 0.8 };
double[] v2 = { 0.8, 0.9, 0.5 };

double distance = SkillVectorEncoder.EuclideanDistance(v1, v2);
// Used by K-means to find nearest cluster
```

### Cosine Similarity
```csharp
double[] v1 = { 1.0, 0.6 };
double[] v2 = { 1.0, 0.6 };

double similarity = SkillVectorEncoder.CosineSimilarity(v1, v2);
// Result: 1.0 (identical vectors)
```

---

## 📊 Data Models Quick Reference

### Participant
```csharp
public class Participant
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string ProfileStatus { get; set; }  // "Submitted", "Approved"
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<ParticipantSkill> Skills { get; set; }
    public ICollection<TeamMember> Teams { get; set; }
    public ICollection<string> Interests { get; set; }
}
```

### Team
```csharp
public class Team
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int EventId { get; set; }
    public string Status { get; set; }  // "Draft", "Suggested", "Locked"
    public bool IsLocked { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<TeamMember> Members { get; set; }
}
```

### Skill
```csharp
public class Skill
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Category { get; set; }  // "Frontend", "Backend", "Data"
    public DateTime CreatedAt { get; set; }
}
```

### ParticipantSkill
```csharp
public class ParticipantSkill
{
    public int Id { get; set; }
    public int ParticipantId { get; set; }
    public int SkillId { get; set; }
    public int ProficiencyLevel { get; set; }  // 1-5 scale
    public DateTime AddedAt { get; set; }
}
```

### TeamMember
```csharp
public class TeamMember
{
    public int Id { get; set; }
    public int TeamId { get; set; }
    public int ParticipantId { get; set; }
    public bool IsLocked { get; set; }  // Individual lock
    public DateTime AddedAt { get; set; }
}
```

### AuditLog
```csharp
public class AuditLog
{
    public int Id { get; set; }
    public int ParticipantId { get; set; }
    public int? TeamId { get; set; }
    public string ChangeType { get; set; }  // "ParticipantAdded", "TeamLocked"
    public string Actor { get; set; }
    public string OldValue { get; set; }
    public string NewValue { get; set; }
    public DateTime Timestamp { get; set; }
}
```

---

## 🛠️ Error Handling

### Common Exceptions
```csharp
try
{
    participantService.RegisterParticipant("", "email@example.com", skills);
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Validation Error: {ex.Message}");
    // "Name and email are required"
}

try
{
    teamService.MoveParticipantBetweenTeams(1, 1, 2);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Operation Error: {ex.Message}");
    // "Participant not found in source team"
    // "Cannot modify locked team"
    // "Team not found"
}

try
{
    participantService.AddSkillToParticipant(1, skill, 10);
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Validation Error: {ex.Message}");
    // "Proficiency level must be between 1 and 5"
}
```

---

## 📈 Performance Tips

### Clustering Performance
```csharp
// For faster clustering with large participant pools
var teams = clusteringService.ClusterParticipants(
    participants,
    teamSize: 4,
    maxIterations: 50  // Reduce iterations for faster convergence
);

// Balance algorithm is more efficient for constraint-based selection
var balancedTeams = clusteringService.BalanceTeamsBySkillDiversity(
    participants,
    teamSize: 4
);
```

### Memory Optimization
- In-memory services store data in Lists
- For 1000+ participants, consider database implementation
- Vector operations use minimal allocation

### Scale Considerations
- Current implementation: ~1000 participants in <30 seconds
- Database layer needed for persistence
- Consider query optimization for large datasets

---

## 🔌 Extending the System

### Adding a New Skill Category
```csharp
// Simply add to SkillRepository initialization
public static void InitializeDefaultSkills()
{
    // ...existing skills...

    // New category
    AddSkill("Rust", "Systems");
    AddSkill("Go", "Systems");
    AddSkill("C++", "Systems");
}
```

### Adding a New Export Format
```csharp
public static string ExportParticipantsToJSON(List<Participant> participants)
{
    // Use Newtonsoft.Json or System.Text.Json
    return JsonConvert.SerializeObject(participants);
}
```

### Adding Custom Clustering Constraints
```csharp
// Extend ClusteringService
private bool IsTeamGeographicallyBalanced(List<Participant> team, 
    Dictionary<int, string> participantLocations)
{
    var locations = new HashSet<string>();
    foreach (var member in team)
    {
        locations.Add(participantLocations[member.Id]);
    }
    return locations.Count >= 2;  // At least 2 different locations
}
```

---

## 🧪 Testing Examples

### Unit Test Template (NUnit)
```csharp
[TestFixture]
public class ParticipantServiceTests
{
    private ParticipantService _service;

    [SetUp]
    public void Setup()
    {
        _service = new ParticipantService();
    }

    [Test]
    public void RegisterParticipant_ValidInput_ReturnsParticipant()
    {
        var participant = _service.RegisterParticipant(
            "Test User", 
            "test@example.com",
            new List<string> { "React" }
        );

        Assert.That(participant.Name, Is.EqualTo("Test User"));
        Assert.That(participant.ProfileStatus, Is.EqualTo("Submitted"));
    }

    [Test]
    public void RegisterParticipant_MissingEmail_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() =>
            _service.RegisterParticipant("Test User", "", new List<string> { "React" })
        );
    }
}
```

---

## 📚 Available Skills Reference

**Frontend (5 skills)**
- React, Vue.js, Angular, HTML/CSS, JavaScript

**Backend (5 skills)**
- C#, Python, Java, Node.js, PHP

**Data (5 skills)**
- SQL, Machine Learning, Data Analysis, Big Data, Python Data Science

**Design (4 skills)**
- UI/UX Design, Graphic Design, Figma, Web Design

**DevOps (5 skills)**
- Docker, Kubernetes, AWS, Azure, CI/CD

---

## 🐛 Debugging Tips

### Enable Detailed Logging
```csharp
var teams = clusteringService.ClusterParticipants(participants, 4);
Console.WriteLine($"Generated {teams.Count} clusters");
foreach (var cluster in teams)
{
    Console.WriteLine($"Cluster size: {cluster.Count}");
    foreach (var p in cluster)
    {
        Console.WriteLine($"  - {p.Name}: {p.Skills.Count} skills");
    }
}
```

### Inspect Audit Trail
```csharp
var logs = teamService.GetAuditLogs();
Console.WriteLine($"Total audit entries: {logs.Count}");
foreach (var log in logs)
{
    Console.WriteLine($"[{log.Timestamp:HH:mm:ss}] {log.ChangeType}");
}
```

### Verify Skill Encoding
```csharp
var skillIndex = new Dictionary<string, int> { {"React", 0}, {"Node.js", 1} };
var vector = SkillVectorEncoder.EncodeParticipantSkills(
    skills,
    skillIndex
);
Console.WriteLine($"Vector: [{string.Join(", ", vector)}]");
```

---

## 📞 Support & Documentation

- **Main README**: `README.md` - Full feature documentation
- **Implementation Guide**: `IMPLEMENTATION.md` - Architecture and design
- **Source Code**: Well-commented across all files
- **Demo**: Run `Program.cs` for working example

---
