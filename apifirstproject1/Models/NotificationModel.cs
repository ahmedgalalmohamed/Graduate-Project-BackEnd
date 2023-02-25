namespace Graduate_Project_BackEnd.Models
{
    public class NotificationModel
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public string SenderRole { get; set; }
        public string? Content { get; set; } = "";
        public int TeamId { get; set; }
        public bool IsReaded { get; set; } = false;
        public StudentsModel Student { get; set; }
        public int StudentId { get; set; }
    }
}
