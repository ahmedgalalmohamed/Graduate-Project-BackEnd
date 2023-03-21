namespace Graduate_Project_BackEnd.Models
{
    public class StudentTeamModel
    {
        public StudentsModel Student { get; set; }
        public int StudentID { get; set; }
        public TeamModel Team { get; set; }
        public int TeamID { get; set; }
    }
}
