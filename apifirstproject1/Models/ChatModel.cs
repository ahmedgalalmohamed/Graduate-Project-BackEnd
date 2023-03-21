namespace Graduate_Project_BackEnd.Models
{
    public class ChatModel
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public int SenderId { get; set; }
        public string Role { get; set; }
        public TeamModel Team { get; set; }
        public int TeamID { get; set; }
    }
}
