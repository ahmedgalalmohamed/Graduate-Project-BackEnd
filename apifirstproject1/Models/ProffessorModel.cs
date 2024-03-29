﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Graduate_Project_BackEnd.Models
{
    public class ProffessorModel
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
        public int TeamCount { get; set; } = 3;

        public List<ProfNotificationsModel> Notifications { get; set; }
        public ICollection<TeamModel> Teams { get; set; }
    }
}
