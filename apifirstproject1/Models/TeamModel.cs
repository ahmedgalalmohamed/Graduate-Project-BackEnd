namespace Graduate_Project_BackEnd.Models
{
    public class TeamModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public StudentsModel Leader { get; set; }
        public int LeaderID { get; set; }

        public CourseModel Course { get; set; }
        public int CourseID { get; set; }

        public ProffessorModel? Prof { get; set; }
        public int? ProfID { get; set; }
        
        public bool IsComplete { get; set; } = false;

        public ICollection<Courses_StudentsModel> Courses_Students { get; set; }
        public ICollection<ProjectModel> Projects { get; set; }
        //public ICollection<StudentTeamModel> Students { get; set; }

    }
}
