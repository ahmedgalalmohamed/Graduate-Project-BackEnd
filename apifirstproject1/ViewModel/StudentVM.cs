namespace Graduate_Project_BackEnd.ViewModel
{
    public class StudentVM
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int Semester { get; set; }
        public List<int> CoursesID {get;set;}
    }
}
