using Graduate_Project_BackEnd.Models;
using Graduate_Project_BackEnd.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PusherServer;
using System.Security.Claims;

namespace Graduate_Project_BackEnd.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class ChatController : Controller
    {
        DBCONTEXT DB;
        public ChatController(DBCONTEXT dB)
        {
            DB = dB;
        }
        public IActionResult Add([FromBody] ChatVM chat)
        {
            bool vf_dr = DB.Teams.Any(t => t.Id == chat.TeamId && t.ProfID == chat.SenderId);
            bool vf_st = DB.Courses_Students.Any(cs => cs.TeamID == chat.TeamId && cs.StudentID == chat.SenderId);
            if (vf_dr || vf_st)
            {
                ChatModel newchat = new()
                {
                    Message = chat.Message,
                    SenderId = chat.SenderId,
                    TeamID = chat.TeamId,
                    Role = chat.Role,
                    Type = chat.Type
                };
                DB.Chat.Add(newchat);
                DB.SaveChanges();
                return Json(new { state = true, msg = "Success" });
            }
            return Json(new { state = false, msg = "You Not Accessed!" });
        }
        public IActionResult Display([FromForm] int team_id)
        {
            List<ChatModel> chats = DB.Chat.Where(c => c.TeamID == team_id).ToList();
            return Json(new { state = true, msg = "Success", data = chats });
        }

        [Authorize(Roles = "student,proffessor,instructor")]
        [HttpPost]
        public IActionResult GetImg([FromForm] int id)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                return Json(new { state = false, msg = "failed" });
            }
            if (currentUser.Role.Equals("student"))
            {
                var cour_std = DB.Courses_Students.SingleOrDefault(s => s.TeamID == id && s.StudentID == currentUser.Id);
                if (cour_std == null)
                    return Json(new { state = false, msg = "Failed to get data" });
            }
            else if (currentUser.Role.Equals("proffessor"))
            {
                var team = DB.Teams.Where(t => t.Id == id && t.ProfID == currentUser.Id).Select(t => t.Id ).ToList();
                if (team.Count == 0)
                    return Json(new { state = false, msg = "Failed to get data" });
            }
            else
            {
                var team = DB.Teams.Where(t => t.Id == id && t.Course.InstructorID == currentUser.Id).Select(t => t.Id).ToList();
                if (team.Count == 0)
                    return Json(new { state = false, msg = "Failed to get data" });
            }
            var leader = DB.Courses_Students.Where(cs => cs.TeamID == id && cs.Team.LeaderID == cs.StudentID).Select(s =>new {s.Student.Id, s.Student.img});
            var members = DB.Courses_Students.Where(cs => cs.TeamID == id && cs.Team.LeaderID != cs.StudentID).Select(s => new { s.Student.Id, s.Student.img });
            var professor = DB.Teams.Where(t => t.Id == id && t.ProfID != null).Select(p => p.Prof.img).ToList();
            if (members != null)
            {
                return Json(new { state = true, msg = "Success", data = new { leader, members, professor = professor.Count == 0 ? null : professor[0] } });
            }
            return Json(new { state = false, msg = "Failed to get data" });
        }
        [HttpGet]
        public async void Pusher_notifiy()
        {
            var pusher = new Pusher(
            "1555576",
            "6b02096c815db94e569b",
            "33153ac9dce2b548644e",
            new PusherOptions
            {
                Cluster = "eu",
                Encrypted = true,
            });
            await pusher.TriggerAsync("chat-channel", "chat-ev", new { });
        }


        private UserLoginVM GetCurrentUser()
        {
            try
            {
                var identity = HttpContext.User.Identity as ClaimsIdentity;

                if (identity != null)
                {
                    var userClaims = identity.Claims;
                    return new UserLoginVM
                    {
                        Email = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Email).Value,
                        Name = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Name).Value,
                        Role = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Role).Value,
                        Id = int.Parse(userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Sid).Value)
                    };
                }
            }
            catch { return null; }
            return null;
        }
    }
}
