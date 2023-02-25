namespace Graduate_Project_BackEnd.Models
{
    public class Courses_StudentsModel
    {


        public StudentsModel Student { get; set; }
        public int StudentID { get; set; }

        public CourseModel Course { get; set; }
        public int CourseID { get; set; }
        public TeamModel? Team { get; set; }
        public int? TeamID { get; set; }
    }
}
