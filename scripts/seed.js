import { connectDB, disconnectDB } from '../config/db.js';
import { Skill } from '../Models/Skill.js';
import { Participant } from '../Models/Participant.js';
import { ParticipantService } from '../Services/ParticipantService.js';
import { Team } from '../Models/Team.js';
import { AuditLog } from '../Models/AuditLog.js';
import { ClusteringRun } from '../Models/ClusteringRun.js';

export const defaultSkills = [
  // Frontend
  { name: 'React', category: 'Frontend' },
  { name: 'Vue.js', category: 'Frontend' },
  { name: 'Angular', category: 'Frontend' },
  { name: 'JavaScript', category: 'Frontend' },
  { name: 'HTML/CSS', category: 'Frontend' },
  // Backend
  { name: 'C#', category: 'Backend' },
  { name: 'Python', category: 'Backend' },
  { name: 'Java', category: 'Backend' },
  { name: 'Node.js', category: 'Backend' },
  { name: 'PHP', category: 'Backend' },
  // Data
  { name: 'SQL', category: 'Data' },
  { name: 'Machine Learning', category: 'Data' },
  { name: 'Data Analysis', category: 'Data' },
  { name: 'Big Data', category: 'Data' },
  { name: 'Python Data Science', category: 'Data' },
  // Design
  { name: 'UI/UX Design', category: 'Design' },
  { name: 'Figma', category: 'Design' },
  { name: 'Graphic Design', category: 'Design' },
  { name: 'Web Design', category: 'Design' },
  // DevOps
  { name: 'Docker', category: 'DevOps' },
  { name: 'Kubernetes', category: 'DevOps' },
  { name: 'AWS', category: 'DevOps' },
  { name: 'Azure', category: 'DevOps' },
  { name: 'CI/CD', category: 'DevOps' }
];

export const demoParticipants = [
  {
    name: 'Alice Johnson',
    email: 'alice@example.com',
    skills: [
      { skillName: 'React', category: 'Frontend', proficiencyLevel: 5 },
      { skillName: 'JavaScript', category: 'Frontend', proficiencyLevel: 4 }
    ],
    interests: ['Web Applications', 'UI/UX']
  },
  {
    name: 'Bob Smith',
    email: 'bob@example.com',
    skills: [
      { skillName: 'C#', category: 'Backend', proficiencyLevel: 5 },
      { skillName: 'Python', category: 'Backend', proficiencyLevel: 3 }
    ],
    interests: ['APIs', 'Databases']
  },
  {
    name: 'Carol White',
    email: 'carol@example.com',
    skills: [
      { skillName: 'Python', category: 'Backend', proficiencyLevel: 4 },
      { skillName: 'SQL', category: 'Data', proficiencyLevel: 5 }
    ],
    interests: ['Machine Learning', 'Analytics']
  },
  {
    name: 'David Lee',
    email: 'david@example.com',
    skills: [
      { skillName: 'JavaScript', category: 'Frontend', proficiencyLevel: 4 },
      { skillName: 'Node.js', category: 'Backend', proficiencyLevel: 4 }
    ],
    interests: ['Web Development']
  },
  {
    name: 'Emma Davis',
    email: 'emma@example.com',
    skills: [
      { skillName: 'UI/UX Design', category: 'Design', proficiencyLevel: 5 },
      { skillName: 'Figma', category: 'Design', proficiencyLevel: 4 }
    ],
    interests: ['User Experience', 'Brand Design']
  },
  {
    name: 'Frank Miller',
    email: 'frank@example.com',
    skills: [
      { skillName: 'Docker', category: 'DevOps', proficiencyLevel: 4 },
      { skillName: 'AWS', category: 'DevOps', proficiencyLevel: 4 }
    ],
    interests: ['Cloud Infrastructure']
  }
];

export async function seedDatabase(forceClean = false) {
  console.log('Seeding MongoDB...');

  if (forceClean) {
    await Skill.deleteMany({});
    await Participant.deleteMany({});
    await Team.deleteMany({});
    await AuditLog.deleteMany({});
    await ClusteringRun.deleteMany({});
    console.log('Cleared existing collections.');
  }

  // Seed Skills
  const existingSkillsCount = await Skill.countDocuments();
  if (existingSkillsCount === 0) {
    await Skill.insertMany(defaultSkills);
    console.log(`Seeded ${defaultSkills.length} default skills.`);
  }

  // Seed Demo Participants
  const existingParticipantsCount = await Participant.countDocuments();
  if (existingParticipantsCount === 0) {
    for (const p of demoParticipants) {
      await ParticipantService.registerParticipant({
        name: p.name,
        email: p.email,
        skills: p.skills,
        interests: p.interests
      });
    }
    console.log(`Seeded ${demoParticipants.length} demo participants.`);
  }
}

// If run directly as a script
if (process.argv[1]?.endsWith('seed.js')) {
  (async () => {
    await connectDB();
    await seedDatabase(true);
    console.log('Seed completed successfully!');
    await disconnectDB();
    process.exit(0);
  })();
}
