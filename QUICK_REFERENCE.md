# TeamWeave - Quick Reference Guide

## ?? What is TeamWeave?

An intelligent **Hackathon Team Matcher** that uses AI/ML clustering algorithms to automatically form balanced, full-stack teams. It ensures every team collectively spans: Frontend, Backend, Database/APIs, DevOps, Version Control, and Design.

---

## ?? Project Structure

```
TeamWeave/
??? index.html              # Single-page frontend app
??? server.js               # Express.js REST API server
??? app.py                  # Python FastAPI microservice
??? package.json            # Node.js dependencies
??? requirements.txt        # Python dependencies
??? .env.example            # Environment template
?
??? config/
?   ??? db.js              # MongoDB connection (embedded or remote)
?
??? Models/
?   ??? Participant.js     # Participant schema
?   ??? Team.js            # Team schema
?   ??? Skill.js           # Skill catalog schema
?   ??? AuditLog.js        # Audit trail schema
?   ??? ClusteringRun.js   # Algorithm run history
?
??? Services/
?   ??? ParticipantService.js      # Register, list, update participants
?   ??? TeamService.js              # CRUD teams, move members
?   ??? ClusteringService.js        # Main clustering pipeline
?   ??? SkillVectorEncoder.js       # Vector math & pillar detection
?
??? Utilities/
?   ??? AuditLogger.js              # Event logging utility
?
??? data/
?   ??? db/                 # Persistent MongoDB storage (auto-created)
?
??? scripts/
    ??? seed.js            # Demo data seeding
    ??? testDb.js          # Database testing
```

---

## ?? Quick Start (3 Commands)

### **1. Install Dependencies**
```bash
npm install                    # Node packages
pip install -r requirements.txt # Python packages
```

### **2. Start Python Service (Terminal 1)**
```bash
python app.py
# Runs on http://localhost:8000
```

### **3. Start Node.js Server (Terminal 2)**
```bash
npm start
# Runs on http://localhost:3000
```

**Open Browser**: `http://localhost:3000`

---

## ?? Frontend Overview

**File**: `index.html` (Vanilla JS + Tailwind CSS)

### **Navigation Tabs**
| Tab | Purpose |
|-----|---------|
| **Dashboard** | Real-time stats, export buttons |
| **Participants** | Register new, list all, manage profiles |
| **Teams Roster** | View formed teams, move members, lock teams |
| **Audit Trail** | See all changes with timestamps |

### **Key Features**
- ?? Live participant & team statistics
- ?? Skill-based clustering algorithm trigger
- ?? Manual participant registration
- ?? Drag-and-drop team member transfer
- ?? Team lock/unlock controls
- ?? Individual member locking
- ?? CSV export (Participants, Teams, Audit Logs)
- ?? Dark mode by default
- ?? Responsive mobile design

### **Design System**
- **Theme**: Cyber-glassmorphism, dark obsidian background
- **Colors**: Purple primary, Cyan secondary, Emerald tertiary
- **Fonts**: Inter (body), Plus Jakarta Sans (display), JetBrains Mono (code)
- **Effects**: Backdrop blur, gradients, smooth animations

---

## ??? Backend Endpoints

### **Health & Metadata**
```bash
GET /api/health              # Server status
GET /api/skills              # Skill catalog (React, Node.js, Docker, etc.)
```

### **Participant Management**
```bash
GET    /api/participants                    # List all (with optional category filter)
POST   /api/participants                    # Register new
GET    /api/participants/:id                # Get single
PUT    /api/participants/:id/status         # Update status (Submitted/Approved/Rejected)
POST   /api/participants/:id/skills         # Add skill
```

### **Team Management**
```bash
GET    /api/teams                           # List all teams
POST   /api/teams                           # Create team
GET    /api/teams/:id                       # Get single team
POST   /api/teams/:id/move                  # Move participant between teams
POST   /api/teams/:id/lock                  # Toggle team lock
POST   /api/teams/:id/members/:memberId/lock # Toggle individual member lock
```

### **Clustering**
```bash
POST   /api/clustering/run                  # Execute clustering algorithm
GET    /api/clustering/runs                 # Get clustering history
```

### **Analytics & Audit**
```bash
GET    /api/audit-logs                      # View all changes (sorted by date)
GET    /api/statistics                      # Dashboard metrics
```

---

## ?? Participant Model

```json
{
  "_id": "ObjectId",
  "name": "John Doe",
  "email": "john@example.com",
  "profileStatus": "Submitted|Approved|Rejected",
  "skills": [
    {
      "skillName": "React",
      "category": "Frontend",
      "proficiencyLevel": 5,
      "addedAt": "2024-09-01T10:00:00Z"
    }
  ],
  "interests": ["Web Development"],
  "createdAt": "2024-09-01T10:00:00Z",
  "updatedAt": "2024-09-01T10:00:00Z"
}
```

---

## ?? Team Model

```json
{
  "_id": "ObjectId",
  "name": "Alpha Team",
  "eventId": 1,
  "status": "Suggested|Draft|Locked|Finalized",
  "isLocked": false,
  "members": [
    {
      "participantId": "ObjectId",
      "name": "John Doe",
      "email": "john@example.com",
      "skills": [...],
      "isLocked": false,
      "addedAt": "2024-09-01T11:00:00Z"
    }
  ],
  "coverageScore": 0.83,
  "coveredPillars": ["Frontend", "Backend", "Database/APIs"],
  "missingPillars": ["DevOps", "Design"],
  "createdAt": "2024-09-01T11:00:00Z",
  "updatedAt": "2024-09-01T11:15:00Z"
}
```

---

## ?? Clustering Algorithm

### **Full-Stack Pillars** (Priority Order)

| # | Pillar | Example Skills |
|---|--------|-----------------|
| 1?? | Frontend | React, Vue, Angular, HTML, CSS, TypeScript |
| 2?? | Backend | Node.js, Python, Java, C#, Django, FastAPI |
| 3?? | Database/APIs | SQL, MongoDB, GraphQL, REST, PostgreSQL |
| 4?? | DevOps/Deployment | Docker, AWS, CI/CD, GitHub Actions, Kubernetes |
| 5?? | Version Control | Git, GitHub, GitLab, Bitbucket |
| 6?? | Design | Figma, UI/UX, Sketch, Adobe |

### **Three Phases**

**Phase 1: K-Means Pre-Clustering**
- Converts participant skills to vectors (Euclidean space)
- Clusters by skill similarity
- Ensures diversity across initial teams

**Phase 2: Coverage-Maximizing Greedy Assignment**
- Sorts participants by # of unique pillars (multi-pillar people first)
- For each team slot, picks the participant who adds MOST new pillars
- Result: Every team has representatives from multiple disciplines

**Phase 3: Coverage Gap Swap Optimizer**
- Scans all team pairs
- Swaps members if it improves combined coverage
- Runs up to 5 rounds or until convergence

### **Output**
```json
{
  "teams": [
    {
      "_id": "ObjectId",
      "name": "Team 1",
      "members": [...],
      "coverageScore": 0.83,
      "coveredPillars": ["Frontend", "Backend", "Database/APIs", "DevOps"],
      "missingPillars": ["Version Control", "Design"]
    }
  ],
  "averageCoverageScore": 0.81,
  "executionTimeMs": 245
}
```

---

## ?? Database Schema

### **Collections**

**participants** - User profiles with skills
- Indexed: email (unique), skills.category, skills.skillName
- ~2KB per document

**teams** - Team rosters with coverage metadata
- Indexed: eventId, isLocked, members.participantId
- ~3KB per document

**skills** - Skill catalog (24 entries)
- React, Vue, Node.js, Python, Docker, Figma, etc.

**audit_logs** - Complete event trail
- ParticipantRegistered, TeamCreated, ParticipantMoved, TeamLocked, etc.

**clustering_runs** - Algorithm execution history
- Algorithm type, participant count, team count, execution time

---

## ?? Python Service (`app.py`)

### **Endpoint**
```bash
POST /matchmake
Content-Type: application/json

{
  "participants": [
    {
      "user_id": "123",
      "name": "John Doe",
      "email": "john@example.com",
      "skills": [
        {
          "skillName": "React",
          "category": "Frontend",
          "proficiencyLevel": 5
        }
      ]
    }
  ],
  "team_size": 4
}
```

### **Response**
```json
{
  "teams": [
    {
      "name": "Team 1",
      "members": [...],
      "coverage_score": 0.83,
      "covered_pillars": ["Frontend", "Backend", "Database/APIs", "DevOps"],
      "missing_pillars": ["Version Control", "Design"]
    }
  ]
}
```

### **Tech Stack**
- **FastAPI**: Modern async web framework
- **Pydantic**: Type validation & serialization
- **scikit-learn**: Machine learning (optional, for stats)
- **Pandas**: Data manipulation

---

## ?? Service Layer

### **ParticipantService**
```javascript
registerParticipant({name, email, skills, interests})
getAllParticipants(category)
getParticipantById(id)
updateParticipantStatus(id, newStatus, actor)
addSkillToParticipant(id, skillName, category, proficiency)
removeSkillFromParticipant(id, skillName)
```

### **TeamService**
```javascript
getAllTeams(eventId)
getTeamById(id)
createTeam(name, eventId, members)
moveParticipantBetweenTeams(participantId, fromTeamId, toTeamId, actor)
toggleTeamLock(teamId, actor)
toggleParticipantLockInTeam(teamId, participantId, actor)
getAuditLogs(filters)
```

### **ClusteringService**
```javascript
executeClusteringPipeline(participantIds, teamSize, useExternalPython)
clusterParticipants(participants, teamSize, maxIterations)
_coverageGreedyAssign(participants, numClusters, teamSize)
_coverageGapSwap(clusters, teamSize)
```

### **SkillVectorEncoder**
```javascript
buildSkillIndex(participants)               // Create vocabulary
encodeParticipantSkills(skills, skillIndex) // Convert to vector
euclideanDistance(v1, v2)                   // Vector distance
getPillarCoverage(participants)             // Which pillars covered
getMissingPillars(participants)             // What's missing
getCoverageScore(participants)              // 0.0 - 1.0 score
getParticipantPillars(participant)          // Single person's pillars
```

---

## ?? Audit Trail Features

Every action is logged with:
- **changeType**: ParticipantRegistered, TeamCreated, ParticipantMoved, TeamLocked, etc.
- **actor**: Who made the change (Participant, Organizer, System)
- **participantId**: Which participant affected
- **teamId**: Which team affected
- **oldValue**: Previous state
- **newValue**: New state
- **details**: Human-readable description
- **createdAt**: Timestamp (ISO 8601)

### **Query Audit Logs**
```bash
GET /api/audit-logs?limit=50&skip=0&actor=Organizer&changeType=TeamLocked
```

---

## ?? Statistics Endpoint

```bash
GET /api/statistics
```

### **Response**
```json
{
  "totalParticipants": 42,
  "totalTeams": 10,
  "averageCoverageScore": 0.82,
  "skillDiversityIndex": 0.91,
  "lockedTeams": 3,
  "clusteringRuns": 5,
  "skillDistribution": {
    "Frontend": 38,
    "Backend": 35,
    "Database": 28,
    "DevOps": 18,
    "VersionControl": 40,
    "Design": 12
  }
}
```

---

## ?? Integration Flow

### **Without Python Service**
```
Frontend
   ?
Express.js (server.js)
   ?
ClusteringService.clusterParticipants()
   ?? Phase 1: K-Means (JS)
   ?? Phase 2: Greedy Assign (JS)
   ?? Phase 3: Gap Swaps (JS)
   ?
MongoDB (Save teams)
   ?
Frontend (Display results)
```

### **With Python Service**
```
Frontend
   ?
Express.js (server.js)
   ?
ClusteringService
   ?? Try: POST to Python FastAPI
   ?   ?? app.py /matchmake
   ?      ?? Phase 1: Pillar detection
   ?      ?? Phase 2: Greedy assign
   ?      ?? Phase 3: Gap swaps
   ?
   ?? Fallback: JS K-Means if Python unavailable
   ?
MongoDB (Save teams)
   ?
Frontend (Display results)
```

---

## ?? Configuration

### **Environment Variables (.env)**

```bash
# MongoDB Connection (leave empty for embedded)
MONGODB_URI=

# Python Clustering Service (optional)
PYTHON_MATCHMAKER_URL=http://localhost:8000

# Server Configuration
PORT=3000
NODE_ENV=development
```

### **Embedded MongoDB**
- **Default**: Uses persistent embedded MongoDB
- **Storage**: `./data/db/` directory (auto-created)
- **Engine**: WiredTiger (compression + durability)
- **No Setup**: Works out of the box!

### **Remote MongoDB**
```bash
# Set MONGODB_URI to your MongoDB Atlas connection string
MONGODB_URI=mongodb+srv://user:pass@cluster.mongodb.net/teamweave?retryWrites=true&w=majority
```

---

## ?? Example Workflow

### **Step 1: Register Participants**
```bash
curl -X POST http://localhost:3000/api/participants \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Alice Frontend",
    "email": "alice@example.com",
    "skills": [
      {"name": "React", "category": "Frontend", "proficiency": 5},
      {"name": "HTML/CSS", "category": "Frontend", "proficiency": 4}
    ],
    "interests": ["Web Development"]
  }'
```

### **Step 2: Get Skills Catalog**
```bash
curl http://localhost:3000/api/skills
```

### **Step 3: Run Clustering**
```bash
curl -X POST http://localhost:3000/api/clustering/run \
  -H "Content-Type: application/json" \
  -d '{
    "participants": ["participant_id_1", "participant_id_2", ...],
    "teamSize": 4,
    "useExternalPython": true
  }'
```

### **Step 4: View Teams**
```bash
curl http://localhost:3000/api/teams
```

### **Step 5: Move Team Member**
```bash
curl -X POST http://localhost:3000/api/teams/team_id_1/move \
  -H "Content-Type: application/json" \
  -d '{
    "participantId": "participant_id_1",
    "toTeamId": "team_id_2",
    "actor": "Organizer"
  }'
```

### **Step 6: View Audit Trail**
```bash
curl http://localhost:3000/api/audit-logs
```

---

## ?? Key Concepts

### **Coverage Score**
- Fraction of full-stack pillars represented in a team
- Range: 0.0 (no coverage) to 1.0 (all 6 pillars)
- Formula: `coveredPillars.length / FULL_STACK_PILLARS.length`

### **Pillar Detection**
- Dynamic keyword matching (case-insensitive)
- Matches against skill name or category
- Example: "React" ? Frontend, "MongoDB" ? Database/APIs

### **Proficiency Level**
- 1 = Beginner
- 2 = Elementary
- 3 = Intermediate (default)
- 4 = Advanced
- 5 = Expert

### **Team Status**
- **Draft**: Initial state, can be modified
- **Suggested**: Result of clustering algorithm
- **Locked**: Finalized teams, no member transfers
- **Finalized**: Ready for event

---

## ?? Troubleshooting

### **Python Service Won't Connect**
```
ClusteringService automatically falls back to JS algorithm
Check: python app.py is running on http://localhost:8000
```

### **MongoDB Connection Failed**
```
Creates persistent embedded MongoDB in ./data/db/
If MONGODB_URI set, must be reachable within 3 seconds
```

### **Port Already in Use**
```bash
# Change port in .env
PORT=3001

# Or kill existing process
lsof -ti:3000 | xargs kill -9
```

### **Skills Not Appearing**
```bash
# Run seed script to populate skills catalog
npm run seed
```

---

## ?? Documentation Files

| File | Purpose |
|------|---------|
| `README.md` | Project overview & features |
| `PROJECT_DEVELOPER_GUIDE.md` | **? You are here (comprehensive guide)** |
| `ARCHITECTURE_TECH_STACK.md` | System diagrams & tech details |
| `GETTING_STARTED.md` | Quick setup instructions |
| `DEVELOPER_GUIDE.md` | API reference |
| `ARCHITECTURE.md` | Design patterns & flows |
| `FILE_INDEX.md` | Complete file listing |

---

## ? Checklist: First Run

- [ ] Install Node.js v18+
- [ ] Install Python 3.9+
- [ ] `npm install` and `pip install -r requirements.txt`
- [ ] Create `.env` file (or leave empty for defaults)
- [ ] `python app.py` (Terminal 1)
- [ ] `npm start` (Terminal 2)
- [ ] Open `http://localhost:3000` in browser
- [ ] Register a test participant
- [ ] Run clustering algorithm
- [ ] View generated teams
- [ ] Check audit trail

---

## ?? You're Ready!

TeamWeave is now ready to form intelligent, balanced teams! 

**Next Steps:**
1. Read `ARCHITECTURE_TECH_STACK.md` for deep dive
2. Explore the APIs in Postman or cURL
3. Modify clustering parameters in frontend
4. Run clustering with different team sizes
5. Export data to CSV for analysis

Happy team matching! ??

