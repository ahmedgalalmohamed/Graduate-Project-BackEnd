namespace Graduate_Project_BackEnd.Models
{
    public class ProfNotificationsModel
    {
        public int Id { get; set; }
        public ProffessorModel professor { get; set; }
        public int ProfId { get; set; }
        public int SenderId { get; set; }
        public string? Content { get; set; } = "";
        public int TeamId { get; set; }
        public bool IsReaded { get; set; } = false;
    }
}
