using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Graduate_Project_BackEnd.Models
{
    public class StudentsModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [DataType(DataType.EmailAddress, ErrorMessage = "Enter Right Email")]
        public string Email { get; set; }
        [RegularExpression("^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$", ErrorMessage = "at least 8 characters")]
        public string Password { get; set; }
        [NotMapped]
        [Compare("Password", ErrorMessage = "Not matched")]
        public string ConfirmPassword { get; set; }
        [NotMapped]
        [RegularExpression("^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$", ErrorMessage = "at least 8 characters")]
        public string OldPass { get; set; }
        public string? Phone { get; set; }
        public string Address { get; set; } = "";
        public string Desciption { get; set; } = "";
        public string? img { get; set; } = "";
        public List<TeamModel> TeamLeader { get; set; }
        public List<Courses_StudentsModel> Courses { get; set; }
        public List<NotificationModel> Notifications { get; set; }
        public List<SkilsModel> Skils { get; set; }
        public int Semester { get; set; }

        internal object Include()
        {
            throw new NotImplementedException();
        }
    }
}
