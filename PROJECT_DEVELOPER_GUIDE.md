# TeamWeave - Complete Developer Guide
## A Comprehensive Technical Overview

---

## ?? Table of Contents
1. [Project Overview](#project-overview)
2. [Technology Stack](#technology-stack)
3. [System Architecture](#system-architecture)
4. [Frontend Architecture](#frontend-architecture)
5. [Backend Architecture](#backend-architecture)
6. [Database Architecture](#database-architecture)
7. [Core Algorithms](#core-algorithms)
8. [API Reference](#api-reference)
9. [Code Walkthrough](#code-walkthrough)

---

## ?? Project Overview

**TeamWeave** is an intelligent hackathon team matcher that uses advanced clustering algorithms to automatically form balanced, full-stack-ready teams. It ensures every team collectively spans key engineering disciplines: Frontend, Backend, Database/APIs, DevOps, Version Control, and Design.

### Key Objectives:
- ?? Eliminate unbalanced teams
- ?? Use AI/ML algorithms for intelligent matching
- ?? Allow manual team adjustments with audit trails
- ?? Provide analytics and CSV exports
- ?? Offer an interactive web portal

---

## ??? Technology Stack

### **Frontend**
| Technology | Purpose | Version |
|------------|---------|---------|
| **HTML5/CSS3** | Markup & styling | Standard |
| **Tailwind CSS** | Utility-first CSS framework | v3 (CDN) |
| **JavaScript (Vanilla)** | Client-side logic | ES6+ |
| **Google Material Symbols** | Icons & UI elements | Latest |
| **Google Fonts** | Inter, Plus Jakarta Sans, JetBrains Mono | Latest |

### **Backend (Node.js)**
| Technology | Purpose | Version |
|------------|---------|---------|
| **Express.js** | Web framework & REST API | ^4.19.2 |
| **Node.js** | JavaScript runtime | v18+ |
| **CORS** | Cross-origin resource sharing | ^2.8.5 |
| **dotenv** | Environment variable management | ^16.4.5 |

### **Database**
| Technology | Purpose | Version |
|------------|---------|---------|
| **MongoDB** | NoSQL document database | 4.4+ |
| **Mongoose** | MongoDB object mapper (ODM) | ^8.5.2 |
| **mongodb-memory-server** | Persistent embedded MongoDB | ^9.4.0 |

### **Python Microservice**
| Technology | Purpose | Version |
|------------|---------|---------|
| **FastAPI** | Python web framework | >=0.111.0 |
| **Uvicorn** | ASGI server | >=0.29.0 |
| **Pydantic** | Data validation | >=2.7.0 |
| **scikit-learn** | Machine learning library | >=1.4.0 |
| **Pandas** | Data manipulation | >=2.2.0 |

---

## ??? System Architecture

```
???????????????????????????????????????????????????????????????????????
?                         CLIENT BROWSER                               ?
?                        (index.html - SPA)                            ?
?  - Dashboard with real-time stats                                    ?
?  - Participant registration form                                     ?
?  - Team clustering interface                                         ?
?  - Manual team management                                            ?
?  - Audit trail viewer                                                ?
?????????????????????????????????????????????????????????????????????????
                             ? HTTP REST API (JSON)
                             ?
???????????????????????????????????????????????????????????????????????
?              EXPRESS.JS SERVER (server.js, port 3000)                ?
?                                                                       ?
?  Routes:                                                              ?
?  ?? GET /api/health                  ? Health check                  ?
?  ?? GET /api/skills                  ? Skill catalog                 ?
?  ?? GET/POST /api/participants       ? Participant CRUD              ?
?  ?? GET/POST /api/teams              ? Team CRUD                     ?
?  ?? POST /api/clustering/run         ? Clustering pipeline           ?
?  ?? POST /api/teams/:id/move         ? Member transfer               ?
?  ?? POST /api/teams/:id/lock         ? Lock management               ?
?  ?? GET /api/audit-logs              ? Audit trail                   ?
????????????????????????????????????????????????????????????????????????
       ?            ?                         ?
       ?            ?                         ?
  ???????????   ???????????         ????????????????????
  ?  Skill  ?   ?  Audit  ?         ?  Clustering      ?
  ?  Mgmt   ?   ?  Logger ?         ?  Service         ?
  ???????????   ???????????         ????????????????????
       ?            ?                         ?
       ?            ?        ??????????????????
       ?            ?        ?
       ?            ?        ??? K-Means Clustering (JS fallback)
       ?            ?        ?   - SkillVectorEncoder
       ?            ?        ?   - Euclidean distance
       ?            ?        ?
       ?            ?        ??? Python FastAPI Microservice (primary)
       ?            ?            (POST http://localhost:8000/matchmake)
       ?            ?            - Coverage-Maximizing Greedy
       ?            ?            - Coverage Gap Swap Optimizer
       ?            ?            - Pillar detection
       ?            ?
       ???????????????????????????????????????????
                                                  ?
                    ???????????????????????????????????????????
                    ?      MONGODB (Port 27017)               ?
                    ?                                          ?
                    ?  Collections:                            ?
                    ?  ?? participants    (participant docs)  ?
                    ?  ?? teams           (team rosters)      ?
                    ?  ?? skills          (skill catalog)     ?
                    ?  ?? audit_logs      (event trail)       ?
                    ?  ?? clustering_runs (algorithm logs)    ?
                    ?                                          ?
                    ?  Storage:                                ?
                    ?  ?? Local: /data/db (persistent WiredTiger)
                    ?  ?? Remote: MongoDB Atlas (if MONGODB_URI)
                    ?  ?? Memory: In-memory fallback           ?
                    ???????????????????????????????????????????
```

---

## ?? Frontend Architecture

### **Technology: Vanilla JavaScript + Tailwind CSS**

#### **File: `index.html`**

A **single-page application (SPA)** built with vanilla JavaScript and Tailwind CSS. Features:

**Design System:**
- **Dark Mode**: Obsidian (#0a0e18) base color
- **Cyber-Glassmorphism**: Frosted glass panels with backdrop blur
- **Color Palette**:
  - Primary: Purple gradient (#8b5cf6)
  - Secondary: Cyan (#06b6d4)
  - Tertiary: Emerald (#10b981)
- **Typography**: Inter (body), Plus Jakarta Sans (display), JetBrains Mono (code)

**Key UI Sections:**

1. **Navigation Header**
   - Brand logo with animated gradient
   - Tab navigation (Dashboard, Participants, Teams, Audit)
   - Quick action buttons (Register, Run Clustering)
   - Live status indicator

2. **Dashboard Tab**
   - Real-time statistics cards:
     - Total participants
     - Formed teams
     - Coverage score
     - Skill diversity index
   - Export buttons (CSV export)
   - Clustering controls

3. **Participants Tab**
   - Participant registration form
   - Skill selector with proficiency levels
   - Participant list with filters
   - Edit/delete capabilities

4. **Teams Tab**
   - Team roster viewer
   - Member transfer between teams
   - Team lock/unlock controls
   - Individual member locking
   - Coverage visualization

5. **Audit Trail Tab**
   - Timestamped event log
   - Change type indicators
   - Actor identification
   - Detailed change descriptions

**JavaScript Features (Embedded in HTML):**

```javascript
// API Communication
async function fetchData(endpoint, options = {})
// GET/POST requests to backend

// Tab Switching
function switchTab(tabName)
// Smooth tab navigation

// Modal Management
function openRegisterModal()
function openClusteringModal()
// Modal lifecycle

// Data Export
function exportData(type)
// CSV export functionality

// Real-time Updates
async function refreshDashboard()
// Polls API for live stats

// Clustering Trigger
async function runClustering()
// Calls /api/clustering/run endpoint
```

**Styling Highlights:**
- Responsive grid layouts (Tailwind breakpoints)
- Smooth transitions and animations
- Glass panel hover effects
- Gradient text and buttons
- Custom scrollbar styling

---

## ??? Backend Architecture

### **Main Server: `server.js`**

Express.js application providing REST API endpoints.

#### **Key Features:**

1. **Middleware Setup**
   ```javascript
   app.use(cors());              // Enable cross-origin requests
   app.use(express.json());      // Parse JSON bodies
   app.use(express.static());    // Serve static files (index.html)
   ```

2. **API Routes Structure**

   **Health & Configuration**
   ```
   GET /api/health              Returns server status & timestamp
   GET /api/skills              Returns all skills catalog
   ```

   **Participant Management** (ParticipantService)
   ```
   GET  /api/participants              Get all participants (with optional category filter)
   POST /api/participants              Register new participant
   GET  /api/participants/:id          Get single participant by ID
   PUT  /api/participants/:id/status   Update participant status (Submitted/Approved/Rejected)
   POST /api/participants/:id/skills   Add skill to participant
   ```

   **Team Management** (TeamService)
   ```
   GET  /api/teams                     Get all teams (filtered by eventId)
   POST /api/teams                     Create new team
   GET  /api/teams/:id                 Get single team
   POST /api/teams/:id/move            Move participant between teams
   POST /api/teams/:id/lock            Toggle team lock status
   POST /api/teams/:teamId/members/:participantId/lock
        Toggle individual member lock
   ```

   **Clustering & Matching** (ClusteringService)
   ```
   POST /api/clustering/run            Execute clustering algorithm
   GET  /api/clustering/runs           Get clustering history
   ```

   **Analytics & Audit**
   ```
   GET /api/audit-logs                 Get all audit logs
   GET /api/statistics                 Get dashboard statistics
   ```

#### **Service Architecture**

##### **1. ParticipantService (`Services/ParticipantService.js`)**

Handles participant lifecycle management.

```javascript
class ParticipantService {
  // Registration with skill validation
  static async registerParticipant({ name, email, skills, interests })

  // Retrieval with filters
  static async getAllParticipants(category)
  static async getParticipantById(id)

  // Status management (Submitted ? Approved ? Rejected)
  static async updateParticipantStatus(id, newStatus, actor)

  // Skill management
  static async addSkillToParticipant(id, skillName, category, proficiency)
  static async removeSkillFromParticipant(id, skillName)

  // Auditing
  // All changes logged to AuditLog collection
}
```

**Key Logic:**
- Email uniqueness validation
- Skill proficiency clamping (1-5)
- Automatic audit trail creation
- Timezone-aware timestamps

##### **2. TeamService (`Services/TeamService.js`)**

Manages team creation and manipulation.

```javascript
class TeamService {
  // CRUD operations
  static async getAllTeams(eventId)
  static async getTeamById(id)
  static async createTeam(name, eventId, members)

  // Member rebalancing
  static async moveParticipantBetweenTeams(fromTeamId, toTeamId, participantId, actor)

  // Locking mechanisms
  static async toggleTeamLock(teamId, actor)
  static async toggleParticipantLockInTeam(teamId, participantId, actor)

  // Audit trail
  static async getAuditLogs(filters)
}
```

**Key Logic:**
- Prevents moving from/to locked teams
- Preserves participant skill data during transfers
- Detailed audit logging for compliance
- Event-based team filtering

##### **3. ClusteringService (`Services/ClusteringService.js`)**

Core intelligent matching algorithm.

```javascript
class ClusteringService {
  // Main entry point
  static async executeClusteringPipeline(
    participants,
    teamSize = 4,
    useExternalPythonService = true
  )

  // Fallback JS algorithm
  static clusterParticipants(participants, teamSize, maxIterations)

  // Greedy assembly phase
  static _coverageGreedyAssign(participants, numClusters, teamSize)

  // Optimization phase
  static _coverageGapSwap(clusters, teamSize)
}
```

**Algorithm Phases:**
1. **K-Means Pre-clustering**: Distributes by skill vectors
2. **Coverage-Maximizing Greedy**: Assigns to fill pillar gaps
3. **Coverage Gap Swap Optimizer**: Improves coverage via member swaps

---

## ?? Database Architecture

### **Technology: MongoDB + Mongoose ODM**

#### **Database Connection (`config/db.js`)**

```javascript
// Priority 1: External MongoDB (MONGODB_URI env variable)
if (process.env.MONGODB_URI) {
  await mongoose.connect(uri)
}

// Priority 2: Persistent Embedded MongoDB (Default)
// - Stores data in `/data/db` directory
// - Uses WiredTiger storage engine
// - Survives server restarts
// - No external installation needed

// Priority 3: Fallback in-memory MongoDB
```

#### **Mongoose Models/Schemas**

##### **1. Participant Model (`Models/Participant.js`)**

```javascript
{
  _id: ObjectId,
  name: String (required, trimmed),
  email: String (required, unique, lowercase, trimmed),
  profileStatus: Enum ('Submitted', 'Approved', 'Rejected'),

  // Embedded sub-document array
  skills: [{
    skillName: String,
    category: String ('Frontend', 'Backend', 'Data', 'Design', 'DevOps'),
    proficiencyLevel: Number (1-5, default 3),
    addedAt: Date
  }],

  interests: [String],
  createdAt: Date (default: now),
  updatedAt: Date (auto-updated on save)
}

// Indexes for fast queries:
// - skills.category: 1
// - skills.skillName: 1
```

**Sample Document:**
```json
{
  "_id": ObjectId("507f1f77bcf86cd799439011"),
  "name": "John Developer",
  "email": "john@example.com",
  "profileStatus": "Approved",
  "skills": [
    {
      "skillName": "React",
      "category": "Frontend",
      "proficiencyLevel": 5,
      "addedAt": "2024-09-01T10:30:00Z"
    },
    {
      "skillName": "Node.js",
      "category": "Backend",
      "proficiencyLevel": 4,
      "addedAt": "2024-09-01T10:30:00Z"
    }
  ],
  "interests": ["Web Development", "AI"],
  "createdAt": "2024-09-01T10:00:00Z",
  "updatedAt": "2024-09-01T10:30:00Z"
}
```

##### **2. Team Model (`Models/Team.js`)**

```javascript
{
  _id: ObjectId,
  name: String (required, trimmed),
  eventId: Number (default 1),
  status: Enum ('Draft', 'Suggested', 'Locked', 'Finalized'),
  isLocked: Boolean (default false),

  // Team members with embedded participant data
  members: [{
    participantId: ObjectId (ref: Participant),
    name: String,
    email: String,
    skills: [{ skillName, category, proficiencyLevel }],
    isLocked: Boolean (default false),
    addedAt: Date
  }],

  // Coverage metadata from clustering algorithm
  coverageScore: Number (0.0 - 1.0),
  coveredPillars: [String],
  missingPillars: [String],

  createdAt: Date,
  updatedAt: Date
}

// Indexes:
// - eventId: 1
// - isLocked: 1
// - members.participantId: 1
```

**Sample Document:**
```json
{
  "_id": ObjectId("507f1f77bcf86cd799439012"),
  "name": "Alpha Team",
  "eventId": 1,
  "status": "Suggested",
  "isLocked": false,
  "members": [
    {
      "participantId": ObjectId("507f1f77bcf86cd799439011"),
      "name": "John Developer",
      "email": "john@example.com",
      "skills": [
        { "skillName": "React", "category": "Frontend", "proficiencyLevel": 5 },
        { "skillName": "Node.js", "category": "Backend", "proficiencyLevel": 4 }
      ],
      "isLocked": false,
      "addedAt": "2024-09-01T11:00:00Z"
    }
  ],
  "coverageScore": 0.8,
  "coveredPillars": ["Frontend", "Backend", "Database/APIs", "DevOps"],
  "missingPillars": ["Version Control", "Design"],
  "createdAt": "2024-09-01T11:00:00Z",
  "updatedAt": "2024-09-01T11:15:00Z"
}
```

##### **3. Skill Model (`Models/Skill.js`)**

```javascript
{
  _id: ObjectId,
  name: String (required, unique, trimmed),
  category: Enum ('Frontend', 'Backend', 'Data', 'Design', 'DevOps'),
  createdAt: Date
}
```

##### **4. AuditLog Model (`Models/AuditLog.js`)**

```javascript
{
  _id: ObjectId,
  changeType: String ('ParticipantRegistered', 'TeamCreated', 'ParticipantMoved', etc.),
  participantId: ObjectId (optional),
  teamId: ObjectId (optional),
  actor: String ('Participant', 'Organizer', 'System'),
  oldValue: Mixed,
  newValue: Mixed,
  details: String (description),
  createdAt: Date (default: now)
}
```

##### **5. ClusteringRun Model (`Models/ClusteringRun.js`)**

```javascript
{
  _id: ObjectId,
  eventId: Number,
  teamSize: Number,
  algorithm: String ('K-Means', 'Python-FastAPI', etc.),
  participantCount: Number,
  teamCount: Number,
  executionTimeMs: Number,
  averageCoverageScore: Number,
  teams: ObjectId[] (ref: Team),
  createdAt: Date
}
```

#### **Collection Statistics**

After running demo with 6 participants and clustering:

| Collection | Documents | Size | Purpose |
|-----------|-----------|------|---------|
| participants | ~6 | ~2KB | User profiles with skills |
| teams | ~2 | ~3KB | Team rosters + coverage |
| skills | ~24 | ~1.5KB | Skill catalog |
| audit_logs | ~20+ | ~2KB | Event trail |
| clustering_runs | ~3 | ~1KB | Algorithm execution history |

---

## ?? Core Algorithms

### **1. Skill Vector Encoding (`Services/SkillVectorEncoder.js`)**

**Purpose**: Convert participant skills into normalized numerical vectors for distance calculations.

```javascript
// Encodes skills into vector space
encodeParticipantSkills(skills, skillIndex) {
  // 1. Build skill vocabulary from all participants
  // 2. Map each skill to a dimension
  // 3. Normalize proficiency: 1-5 ? 0.2-1.0
  // Output: Array of floats [0.0, 1.0]
}

// Euclidean distance between vectors
euclideanDistance(v1, v2) {
  return sqrt(sum((v1[i] - v2[i])²))
}
```

**Example:**
```
Participant: { skills: [
  { skillName: "React", proficiency: 5 },
  { skillName: "Python", proficiency: 3 }
]}

Skill Index: { "React": 0, "Python": 1, "Docker": 2, ... }

Vector: [1.0, 0.6, 0.0, ...]  // Normalized to 0-1 range
```

### **2. K-Means Clustering (Phase 1)**

**Purpose**: Initial skill-based grouping using Euclidean distance.

**Algorithm:**
```
1. Initialize K centroids randomly from participant vectors
2. For each iteration (max 100):
   a. Assign each participant to nearest centroid
   b. Recalculate centroids as mean of assigned vectors
   c. Stop if no changes (convergence)
3. Return K clusters
```

**Result**: Initial diversity-based grouping that prevents all low-skill participants in one team.

### **3. Coverage-Maximizing Greedy Assignment (Phase 2)**

**Purpose**: Redistribute participants to maximize full-stack pillar coverage.

**Full-Stack Pillars** (in priority order):
1. **Frontend** - React, Vue, Angular, HTML/CSS, TypeScript
2. **Backend** - Node.js, Python, Java, C#, Django, FastAPI
3. **Database/APIs** - SQL, MongoDB, GraphQL, REST, PostgreSQL
4. **DevOps/Deployment** - Docker, AWS, CI/CD, GitHub Actions, Terraform
5. **Version Control** - Git, GitHub, GitLab, Bitbucket
6. **Design** - Figma, UI/UX, Sketch, Adobe

**Algorithm:**
```
Input: Unassigned participants, empty teams
Output: Teams with participants, maximized coverage

1. Sort participants by: # unique pillars (DESC), then total proficiency (DESC)
   ? Multi-pillar participants become "anchors"

2. While unassigned participants exist:
   a. Find team with lowest coverage score
   b. From unassigned pool, select participant who adds MOST new pillars
      (tiebreak: highest total proficiency)
   c. Assign participant to team
   d. Update team's covered/missing pillars

3. Return balanced teams
```

**Example:**
```
Team = [Frontend Dev (React), Backend Dev (Python, MongoDB)]
Covered: [Frontend, Backend, Database/APIs]
Missing: [DevOps, Version Control, Design]

Unassigned:
- DevOps Expert (Docker, AWS, CI/CD): adds 1 pillar ? SELECTED
- Junior Frontend (React): adds 0 new pillars
- Designer (Figma, Sketch): adds 1 pillar

Result: Team gains DevOps expert
```

### **4. Coverage Gap Swap Optimizer (Phase 3)**

**Purpose**: Fine-tune teams by swapping members to improve coverage.

**Algorithm:**
```
For each pair of teams (i, j) {
  For each member in team i {
    For each member in team j {
      Swap = member_i ? member_j

      If (Swap improves combined coverage AND
          doesn't reduce either team below baseline) {
        Execute swap
        Break to next pair (one swap per round)
      }
    }
  }
}

Bounded to min(5, numTeams) rounds for performance
```

**Example:**
```
Before:
Team A: [Frontend, Backend, Database]  ? Coverage: 3/6
Team B: [Frontend, Design, DevOps]     ? Coverage: 3/6
Combined: 4/6 pillars

Swap: Backend expert (A) ? Frontend junior (B)

After:
Team A: [Frontend, Database, Design, DevOps]  ? Coverage: 4/6
Team B: [Frontend, Backend, DevOps]            ? Coverage: 3/6
Combined: 5/6 pillars (improved!)
```

---

## ?? Python Microservice (`app.py`)

**FastAPI** implementation of coverage-maximizing matching.

### **Architecture**

```python
@app.post("/matchmake")
async def matchmake(request: MatchmakeRequest):
    """
    Receives: List of participants from Node.js backend
    Returns: Clustered teams with coverage metadata
    """

    # Phase 1: Dynamic pillar detection from skills
    for participant in participants:
        participant.covered_pillars = detect_pillars(participant.skills)

    # Phase 2: Greedy team assembly
    teams = coverage_greedy_assign(participants, numTeams)

    # Phase 3: Coverage gap swap optimization
    teams = coverage_gap_swap(teams)

    # Return with coverage metadata
    return {
        "teams": [
            {
                "name": "Team 1",
                "members": [...],
                "coverage_score": 0.83,
                "covered_pillars": ["Frontend", "Backend", ...],
                "missing_pillars": [...]
            },
            ...
        ]
    }
```

### **Pydantic Models**

```python
class ParticipantSkill(BaseModel):
    skillName: str
    category: str
    proficiencyLevel: int = 3  # 1-5

class Participant(BaseModel):
    user_id: str
    name: str
    email: Optional[str]
    skills: List[ParticipantSkill]

    def covered_pillars(self) -> Set[str]:
        """Returns set of full-stack pillars this participant covers"""

class MatchmakeRequest(BaseModel):
    participants: List[Participant]
    team_size: int = 4
```

### **Integration with Node.js**

In `ClusteringService.js`:

```javascript
if (PYTHON_MATCHMAKER_URL) {
  try {
    // Call Python service
    const response = await fetch(
      `${PYTHON_MATCHMAKER_URL}/matchmake`,
      { method: 'POST', body: JSON.stringify({ participants, teamSize }) }
    );

    if (response.ok) {
      return response.json();
    }
  } catch (err) {
    logger.warn('Python service unavailable, using JS fallback');
  }
}

// Fallback to JS algorithm
return this.clusterParticipants(participants, teamSize);
```

---

## ?? API Reference

### **Base URL**: `http://localhost:3000/api`

### **1. Health Check**
```
GET /health
Response: { status: 'ok', time: '2024-09-01T10:30:00Z' }
```

### **2. Skills Catalog**
```
GET /skills
Response: [
  { _id: ObjectId, name: 'React', category: 'Frontend' },
  { _id: ObjectId, name: 'Node.js', category: 'Backend' },
  ...
]
```

### **3. Participants**

**List All**
```
GET /participants?category=Frontend
Response: [{ _id, name, email, skills[], profileStatus, createdAt }, ...]
```

**Register New**
```
POST /participants
Body: {
  name: "John Doe",
  email: "john@example.com",
  skills: [
    { name: "React", category: "Frontend", proficiency: 5 },
    { name: "Node.js", category: "Backend", proficiency: 4 }
  ],
  interests: ["Web Development"]
}
Response: { _id, name, email, skills[], createdAt }
```

**Get Single**
```
GET /participants/:id
Response: { _id, name, email, skills[], profileStatus, createdAt }
```

**Update Status**
```
PUT /participants/:id/status
Body: { newStatus: "Approved" }
Response: { _id, name, profileStatus: "Approved", updatedAt }
```

### **4. Teams**

**List All**
```
GET /teams?eventId=1
Response: [
  {
    _id, name, eventId, status, isLocked,
    members: [{ participantId, name, email, skills[], isLocked }],
    coverageScore, coveredPillars, missingPillars,
    createdAt
  },
  ...
]
```

**Create Team**
```
POST /teams
Body: { name: "Team Alpha", eventId: 1, members: [] }
Response: { _id, name, eventId, status: "Draft", members: [], createdAt }
```

**Move Participant**
```
POST /teams/:fromTeamId/move
Body: {
  participantId: ObjectId,
  toTeamId: ObjectId,
  actor: "Organizer"
}
Response: { fromTeam: {...}, toTeam: {...} }
```

**Lock Team**
```
POST /teams/:id/lock
Body: { actor: "Organizer" }
Response: { _id, name, isLocked: true, status: "Locked" }
```

### **5. Clustering**

**Run Clustering**
```
POST /clustering/run
Body: {
  participants: [ObjectId, ...],  // Participant IDs to cluster
  teamSize: 4,
  useExternalPython: true  // Use Python service if available
}
Response: {
  clusteringRunId: ObjectId,
  teams: [{
    _id, name, members, coverageScore,
    coveredPillars, missingPillars
  }, ...],
  averageCoverageScore: 0.78,
  executionTimeMs: 234
}
```

### **6. Audit Logs**

**List Audit Trail**
```
GET /audit-logs?limit=50&skip=0
Response: [{
  _id, changeType, participantId, teamId, actor,
  oldValue, newValue, details, createdAt
}, ...]
```

### **7. Statistics**

**Dashboard Stats**
```
GET /statistics
Response: {
  totalParticipants: 42,
  totalTeams: 10,
  averageCoverageScore: 0.82,
  skillDiversityIndex: 0.91,
  lockedTeams: 3,
  clusteringRuns: 5
}
```

---

## ?? Code Walkthrough

### **Frontend Flow**

**User Registration:**
```javascript
// 1. User fills registration form in index.html
// 2. Form validates skills and email

// 3. JavaScript event handler:
async function submitRegistration() {
  const formData = {
    name: document.getElementById('name').value,
    email: document.getElementById('email').value,
    skills: selectedSkills,  // Array of {name, category, proficiency}
    interests: selectedInterests
  };

  // 4. POST to backend
  const response = await fetch('/api/participants', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(formData)
  });

  const participant = await response.json();

  // 5. Show success toast
  showToast(`Registered: ${participant.name}`, 'success');

  // 6. Refresh participant list
  await refreshParticipantsList();
}
```

### **Backend Flow**

**Registration Handler:**
```javascript
// server.js
app.post('/api/participants', async (req, res) => {
  try {
    const { name, email, skills, interests } = req.body;

    // Call service
    const participant = await ParticipantService.registerParticipant({
      name, email, skills, interests
    });

    res.status(201).json(participant);
  } catch (err) {
    res.status(400).json({ error: err.message });
  }
});

// ParticipantService.registerParticipant()
static async registerParticipant({ name, email, skills, interests }) {
  // 1. Validate inputs
  if (!name || !email) throw new Error('Name and email required');

  // 2. Check duplicate email
  const existing = await Participant.findOne({ email: email.toLowerCase() });
  if (existing) throw new Error('Email already registered');

  // 3. Create MongoDB document
  const participant = new Participant({
    name: name.trim(),
    email: email.toLowerCase().trim(),
    skills: skills.map(s => ({
      skillName: s.name || s.skillName,
      category: s.category || 'Frontend',
      proficiencyLevel: Number(s.proficiency || 3)
    })),
    interests: interests || [],
    profileStatus: 'Submitted'
  });

  // 4. Save to MongoDB
  const saved = await participant.save();

  // 5. Create audit log
  await AuditLog.create({
    changeType: 'ParticipantRegistered',
    participantId: saved._id,
    actor: 'Participant',
    details: `Registered '${saved.name}' with ${saved.skills.length} skills`
  });

  return saved;
}
```

**Database Persistence:**
```javascript
// MongoDB stores document:
{
  _id: ObjectId("507f..."),
  name: "John Doe",
  email: "john@example.com",
  skills: [
    { skillName: "React", category: "Frontend", proficiencyLevel: 5, addedAt: 2024-09-01T... },
    { skillName: "Node.js", category: "Backend", proficiencyLevel: 4, addedAt: 2024-09-01T... }
  ],
  interests: ["Web Development"],
  profileStatus: "Submitted",
  createdAt: 2024-09-01T10:00:00Z,
  updatedAt: 2024-09-01T10:00:00Z
}

// AuditLog entry:
{
  changeType: "ParticipantRegistered",
  participantId: ObjectId("507f..."),
  actor: "Participant",
  details: "Registered 'John Doe' with 2 skills",
  createdAt: 2024-09-01T10:00:00Z
}
```

### **Clustering Flow**

**Trigger from Frontend:**
```javascript
async function runClustering() {
  const unassignedParticipants = await fetchUnassignedParticipants();

  const response = await fetch('/api/clustering/run', {
    method: 'POST',
    body: JSON.stringify({
      participants: unassignedParticipants.map(p => p._id),
      teamSize: 4,
      useExternalPython: true
    })
  });

  const result = await response.json();

  // result.teams[] contains formed teams with coverage metadata
  displayTeamsWithCoverage(result.teams);

  showToast(`Formed ${result.teams.length} teams`, 'success');

  await refreshDashboard();
}
```

**Backend Clustering Pipeline:**
```javascript
// server.js
app.post('/api/clustering/run', async (req, res) => {
  try {
    const { participants, teamSize } = req.body;

    const result = await ClusteringService.executeClusteringPipeline(
      participants,
      teamSize,
      true  // useExternalPythonService
    );

    res.json(result);
  } catch (err) {
    res.status(400).json({ error: err.message });
  }
});

// ClusteringService.executeClusteringPipeline()
static async executeClusteringPipeline(
  participantIds,
  teamSize = 4,
  useExternalPython = true
) {
  // 1. Fetch participants from MongoDB
  const participants = await Participant.find({
    _id: { $in: participantIds }
  });

  let teams;

  // 2. Try Python service first
  if (useExternalPython && PYTHON_MATCHMAKER_URL) {
    try {
      const response = await fetch(`${PYTHON_MATCHMAKER_URL}/matchmake`, {
        method: 'POST',
        body: JSON.stringify({
          participants: participants,
          team_size: teamSize
        })
      });

      if (response.ok) {
        teams = (await response.json()).teams;
      }
    } catch (err) {
      logger.warn('Python service unavailable, using JS fallback');
      teams = this.clusterParticipants(participants, teamSize);
    }
  } else {
    // 3. Use JS K-Means fallback
    teams = this.clusterParticipants(participants, teamSize);
  }

  // 4. Create Team documents in MongoDB
  const savedTeams = [];
  for (const teamCluster of teams) {
    const team = new Team({
      name: `Team ${savedTeams.length + 1}`,
      eventId: 1,
      members: teamCluster.map(p => ({
        participantId: p._id,
        name: p.name,
        email: p.email,
        skills: p.skills
      })),
      coverageScore: getCoverageScore(teamCluster),
      coveredPillars: getPillarCoverage(teamCluster),
      missingPillars: getMissingPillars(teamCluster),
      status: 'Suggested'
    });

    await team.save();
    savedTeams.push(team);
  }

  // 5. Log clustering run
  await ClusteringRun.create({
    eventId: 1,
    teamSize,
    algorithm: useExternalPython ? 'Python-FastAPI' : 'JS-KMeans',
    participantCount: participants.length,
    teamCount: savedTeams.length,
    executionTimeMs: Date.now() - startTime,
    teams: savedTeams.map(t => t._id)
  });

  return { teams: savedTeams };
}
```

**K-Means Clustering (JS Fallback):**
```javascript
// Phase 1: K-Means pre-clustering
const numClusters = Math.ceil(participants.length / teamSize);
const skillIndex = SkillVectorEncoder.buildSkillIndex(participants);

// Convert skills to vectors
const vectors = participants.map(p =>
  SkillVectorEncoder.encodeParticipantSkills(p.skills, skillIndex)
);

// Initialize K random centroids
let centroids = this._initCentroids(vectors, numClusters);

// Iterate until convergence
for (let iter = 0; iter < maxIterations; iter++) {
  // Assign each participant to nearest centroid
  const assignments = vectors.map(v => {
    let minDist = Infinity, cluster = 0;
    for (let c = 0; c < centroids.length; c++) {
      const dist = SkillVectorEncoder.euclideanDistance(v, centroids[c]);
      if (dist < minDist) { minDist = dist; cluster = c; }
    }
    return cluster;
  });

  // Recalculate centroids
  for (let c = 0; c < numClusters; c++) {
    const members = vectors.filter((_, i) => assignments[i] === c);
    const newCentroid = new Array(skillIndex.size).fill(0);
    members.forEach(m => {
      for (let d = 0; d < skillIndex.size; d++) newCentroid[d] += m[d];
    });
    for (let d = 0; d < skillIndex.size; d++)
      newCentroid[d] /= members.length;
    centroids[c] = newCentroid;
  }

  // Check convergence
  if (!changed) break;
}

// Phase 2: Coverage-Maximizing Greedy Assignment
const greedyTeams = this._coverageGreedyAssign(participants, numClusters, teamSize);

// Phase 3: Coverage Gap Swap Optimization
return this._coverageGapSwap(greedyTeams, teamSize);
```

---

## ?? Data Flow Diagrams

### **Participant Registration**
```
????????????????
? User Browser ?
????????????????
       ? Form Input
       ?
????????????????????????????
? index.html               ?
? - Form validation        ?
? - Skill selection        ?
? - Proficiency rating     ?
????????????????????????????
       ? POST /api/participants
       ? { name, email, skills[] }
       ?
????????????????????????????
? server.js (Express)      ?
? /api/participants route  ?
????????????????????????????
       ? Call service
       ?
????????????????????????????????
? ParticipantService           ?
? - Validate inputs            ?
? - Check duplicate email      ?
? - Map skill data             ?
??????????????????????????????????
       ? Save document
       ?
????????????????????????????
? MongoDB                  ?
? participants collection  ?
? + index on email         ?
????????????????????????????
       ? Audit
       ?
????????????????????????????
? MongoDB                  ?
? audit_logs collection    ?
????????????????????????????
       ?
       ??? Response JSON
           back to browser
```

### **Clustering Execution**
```
????????????????
? Frontend     ?
? "Run         ?
?  Clustering" ?
????????????????
       ? POST /api/clustering/run
       ? { participantIds[], teamSize }
       ?
??????????????????????????????
? server.js                  ?
? /api/clustering/run        ?
??????????????????????????????
       ? Fetch participants
       ? from MongoDB
       ?
??????????????????????????????
? MongoDB                    ?
? Participant.find({})       ?
??????????????????????????????
       ? Return docs
       ?
??????????????????????????????
? ClusteringService          ?
? .executeClusteringPipeline ?
??????????????????????????????
       ?
       ??? Try Python Service
       ?   POST /matchmake
       ?   ?
       ?   ????????????????????
       ?   ? app.py (FastAPI) ?
       ?   ? Coverage Greedy  ?
       ?   ? + Gap Swaps      ?
       ?   ????????????????????
       ?   ? Success?
       ?   ??? No: Use JS fallback
       ?
       ??? JS K-Means + Coverage
           Phase 1: K-Means
           Phase 2: Greedy Assignment
           Phase 3: Swap Optimizer
           ?
       ??????????????????????????
       ? teams[] with coverage  ?
       ? - coverageScore        ?
       ? - coveredPillars       ?
       ? - missingPillars       ?
       ??????????????????????????
                ? Save to MongoDB
                ?
       ??????????????????????????
       ? MongoDB                ?
       ? teams collection       ?
       ? clustering_runs        ?
       ??????????????????????????
                ? Audit log
                ?
       ??????????????????????????
       ? MongoDB                ?
       ? audit_logs collection  ?
       ??????????????????????????
                ?
                ??? Response to Frontend
                    { teams[], average coverage }
```

---

## ?? Environment Variables

**File: `.env`**

```bash
# MongoDB Connection
# Leave empty to use persistent embedded MongoDB
MONGODB_URI=

# Python Clustering Service
# Set to enable external Python FastAPI service
PYTHON_MATCHMAKER_URL=http://localhost:8000

# Server Port
PORT=3000

# Node Environment
NODE_ENV=development
```

---

## ?? Running the Project

### **Start All Components**

```bash
# Terminal 1: Python FastAPI Microservice
python app.py
# Runs on http://localhost:8000
# Provides: POST /matchmake

# Terminal 2: Node.js Express Server
npm start
# Runs on http://localhost:3000
# Serves: index.html + REST API

# Terminal 3 (optional): MongoDB Monitoring
mongo --version  # Verify installation
```

### **Access the Application**

Open browser: `http://localhost:3000`

**Features Available:**
1. **Dashboard**: Real-time stats
2. **Participant Registration**: Add participants with skills
3. **Clustering**: Run intelligent team matching
4. **Team Management**: Move members, lock teams
5. **Audit Trail**: View all changes
6. **CSV Export**: Export participants, teams, audit logs

---

## ?? Summary

**TeamWeave** is a sophisticated full-stack application combining:

- **Frontend**: Interactive SPA with Tailwind CSS styling
- **Backend**: Express.js REST API with microservice architecture
- **Database**: MongoDB with persistent storage
- **Algorithms**: K-Means clustering + Coverage-Maximizing Greedy + Swap Optimizer
- **Python Integration**: FastAPI microservice for optional external clustering

All components work together to intelligently form balanced, full-stack-ready teams for hackathons and competitions!

