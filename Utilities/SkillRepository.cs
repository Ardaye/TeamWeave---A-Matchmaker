using System;
using System.Collections.Generic;

namespace TeamWeave.Utilities
{
    public class SkillRepository
    {
        private static List<Models.Skill> _skills = new List<Models.Skill>();
        private static int _nextId = 1;

        static SkillRepository()
        {
            InitializeDefaultSkills();
        }

        private static void InitializeDefaultSkills()
        {
            // Frontend skills
            AddSkill("React", "Frontend");
            AddSkill("Vue.js", "Frontend");
            AddSkill("Angular", "Frontend");
            AddSkill("HTML/CSS", "Frontend");
            AddSkill("JavaScript", "Frontend");

            // Backend skills
            AddSkill("C#", "Backend");
            AddSkill("Python", "Backend");
            AddSkill("Java", "Backend");
            AddSkill("Node.js", "Backend");
            AddSkill("PHP", "Backend");

            // Data skills
            AddSkill("SQL", "Data");
            AddSkill("Machine Learning", "Data");
            AddSkill("Data Analysis", "Data");
            AddSkill("Big Data", "Data");
            AddSkill("Python Data Science", "Data");

            // Design skills
            AddSkill("UI/UX Design", "Design");
            AddSkill("Graphic Design", "Design");
            AddSkill("Figma", "Design");
            AddSkill("Web Design", "Design");

            // DevOps/Infrastructure skills
            AddSkill("Docker", "DevOps");
            AddSkill("Kubernetes", "DevOps");
            AddSkill("AWS", "DevOps");
            AddSkill("Azure", "DevOps");
            AddSkill("CI/CD", "DevOps");
        }

        public static void AddSkill(string name, string category)
        {
            var skill = new Models.Skill
            {
                Id = _nextId++,
                Name = name,
                Category = category,
                CreatedAt = DateTime.Now
            };
            _skills.Add(skill);
        }

        public static Models.Skill GetSkillById(int id)
        {
            foreach (var skill in _skills)
            {
                if (skill.Id == id)
                    return skill;
            }
            return null;
        }

        public static Models.Skill GetSkillByName(string name)
        {
            foreach (var skill in _skills)
            {
                if (skill.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return skill;
            }
            return null;
        }

        public static List<Models.Skill> GetAllSkills()
        {
            return new List<Models.Skill>(_skills);
        }

        public static List<Models.Skill> GetSkillsByCategory(string category)
        {
            var result = new List<Models.Skill>();
            foreach (var skill in _skills)
            {
                if (skill.Category == category)
                    result.Add(skill);
            }
            return result;
        }

        public static List<string> GetAllCategories()
        {
            var categories = new HashSet<string>();
            foreach (var skill in _skills)
            {
                categories.Add(skill.Category);
            }
            return new List<string>(categories);
        }
    }
}
