namespace Graduate_Project_BackEnd.ViewModel
{
    public class NotificationVM
    {
        public int? Id { get; set; }
        public int TeamId { get; set; }
        public int? CourseId { get; set; }
        public int SenderId { get; set; }
        public int? StudentId { get; set; }
        public string? SenderRole { get; set; }
        public int? ProfId { get; set; }
        public string? SenderName { get; set; }
        public string? SenderEmail { get; set; }
        public string? Content { get; set; } = "";
    }
}
