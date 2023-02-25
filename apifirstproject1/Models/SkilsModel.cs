namespace Graduate_Project_BackEnd.Models
{
    public class SkilsModel
    {
        public int Id { get; set; }
        public StudentsModel Student { get; set; }
        public int StudentID { get; set; }
        public string? skil { get; set; }
    }
}
