namespace Graduate_Project_BackEnd.ViewModel
{
    public class CourseVM
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string? NewName { get; set; }
        public string Desciption { get; set; }
        public int InstructorID { get; set; }
		public bool IsGraduate { get; set; } 
        public string? InstructorName { get; set; }
    }
}
