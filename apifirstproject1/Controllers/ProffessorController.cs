using Graduate_Project_BackEnd.Models;
using Graduate_Project_BackEnd.ViewModel;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Graduate_Project_BackEnd.Controllers
{
    //[Authorize(Roles = "admin")]
    [Route("[controller]/[action]")]
    [ApiController]
    public class ProffessorController : Controller
    {
        DBCONTEXT DB;
        public ProffessorController(DBCONTEXT dB)
        {
            DB = dB;
        }

        [HttpGet]
        public IActionResult Display()
        {
            var Proffessors = DB.Proffessors.OrderBy(p => p.Name).ToList();
            if (Proffessors != null)
                return Json(new { state = true, msg = "Success", data = Proffessors });
            return Json(new { state = false, msg = "failed", data = Proffessors });
        }
        [HttpPost]
        public IActionResult Add([FromBody] ProffessorVM proffessor)
        {

            ProffessorModel found = DB.Proffessors.SingleOrDefault(p => p.Email.Equals(proffessor.Email));
            if (found != null)
                return Json(new { state = false, msg = "Proffessor found" });
            ProffessorModel newProffessor = new ProffessorModel();
            newProffessor.Name = proffessor.Name;
            newProffessor.Email = proffessor.Email;
            newProffessor.Password = proffessor.Password;
            DB.Proffessors.Add(newProffessor);
            DB.SaveChanges();
            return Json(new { state = true, msg = "Success" });
        }
        [HttpPut]
        public IActionResult AdminEdit([FromBody] ProffessorVM proffessor)
        {

            ProffessorModel oldProffesser = DB.Proffessors.SingleOrDefault(i => i.Email.Equals(proffessor.Email));
            if (oldProffesser == null)
                return Json(new { state = false, msg = "Not Found" });
            oldProffesser.Name = proffessor.Name;
            oldProffesser.Password = proffessor.Password;
            DB.Proffessors.Update(oldProffesser);
            DB.SaveChanges();
            return Display();
        }
        [HttpDelete]
        public IActionResult Delete([FromForm] string email)
        {

            ProffessorModel found = DB.Proffessors.SingleOrDefault(p => p.Email.Equals(email));
            if (found == null)
                return Json(new { state = false, msg = "proffessor found" });
            DB.Proffessors.Remove(found);
            DB.SaveChanges();
            return Display();
        }
        [HttpPost]
        public IActionResult AddCSV([FromForm] IFormFile readexcel)
        {
            int cnt = 0;
            var reader = new StreamReader(readexcel.OpenReadStream());
            var header = (reader.ReadLine().ToLower()).Split(',').ToList();
            while (reader.Peek() >= 0)
            {
                var prof = (reader.ReadLine()).Split(',').ToList();
                var found = DB.Proffessors.SingleOrDefault(s => s.Email.Equals(prof[header.IndexOf("email")]));
                if (found == null)
                {
                    cnt++;
                    DB.Proffessors.Add(new ProffessorModel() { Name = prof[header.IndexOf("name")], Email = prof[header.IndexOf("email")], Password = prof[header.IndexOf("password")] });
                    DB.SaveChanges();
                }
            }
            return Json(new { msg = $"{cnt}", state = cnt > 0 ? true : false });
        }

        //[Authorize(Roles = "proffessor")]
        [HttpGet]
        public IActionResult MyTeams()
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null || currentUser.Id == null)
            {
                return Json(new { state = false, msg = "failed" });
            }
            var profs_teams = DB.Proffessors.Where(p => p.Id == currentUser.Id).SelectMany(p => p.Teams, (prof, teams) => new { teams.Name, teams.Id });
            return Json(new { state = true, msg = "Success", data = profs_teams });
        }

        [HttpPost]
        public IActionResult GetAvailableProfs()
        {
            var Profs = DB.Proffessors.Where(p => p.TeamCount > DB.Teams.Where(t => t.ProfID == p.Id).Select(t => t.Name).ToList().Count).Select(p => new { p.Name, p.Email, p.Id });

            return Json(new { state = true, msg = "Success", data = Profs });

        }

        private UserLoginVM GetCurrentUser()
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
            return null;
        }

        [HttpPost]
        public IActionResult sendJoinRequest([FromBody] NotificationVM notification)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                return Json(new { state = false, msg = "failed" });
            }
            if (currentUser.Id == notification.SenderId)
            {
                var prof = DB.Proffessors.SingleOrDefault(s => s.Id == notification.ProfId);
                if (prof == null)
                    return Json(new { state = false, msg = "Professor not found" });
                var teamCount = DB.Teams.Where(p => p.ProfID == notification.ProfId).Select(t => t.Id).Count();
                if (teamCount >= prof.TeamCount)
                    return Json(new { state = false, msg = "Teams is complete" });
                var team = DB.Teams.Where(t => t.Id == notification.TeamId).Select(t => new { t.LeaderID }).ToList();
                if (team.Count > 0)
                {
                    var std = DB.Students.SingleOrDefault(s => s.Id == notification.SenderId);
                    if (std == null)
                        return Json(new { state = false, msg = "failed to send request" });
                    var notifications = DB.profNotifications.Where(n => n.TeamId == notification.TeamId && n.SenderId == currentUser.Id && n.Content.Contains("Request")).ToList();
                    var stdnotific = DB.profNotifications.Where(n => n.TeamId == notification.TeamId && n.SenderId == currentUser.Id && n.ProfId == notification.ProfId && n.Content.ToLower().Contains("request")).ToList();
                    if (notifications.Count == 0 || (team[0].LeaderID == currentUser.Id && stdnotific.Count == 0))
                    {
                        DB.profNotifications.Add(new ProfNotificationsModel()
                        {
                            SenderId = notification.SenderId,
                            ProfId = (int)notification.ProfId,
                            TeamId = notification.TeamId,
                            Content = "Request ",
                        });
                        DB.SaveChanges();
                        return Json(new { state = true, msg = "Send Successfully" });
                    }
                    return Json(new { state = false, msg = "Request is already sent" });
                }
            }
            return Json(new { state = false, msg = "failed" });
        }

        [HttpGet]
        public IActionResult getNotification()
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                return Json(new { state = false, msg = "failed" });
            }
            var prof = DB.Proffessors.SingleOrDefault(s => s.Id == currentUser.Id);
            if (prof == null)
                return Json(new { state = false, msg = "failed to get Professor data" });
            var notifications = DB.profNotifications.Where(n => n.ProfId == prof.Id && n.IsReaded == false).Select(n => new { n.Id, n.SenderId, n.Content, n.TeamId }).ToList();
            List<NotificationVM> senders = new List<NotificationVM>();
            foreach (var notification in notifications)
            {
                var tmp = DB.Students.Where(s => s.Id == notification.SenderId).Select(s => new { s.Name, s.Email }).ToList();
                senders.Add(new NotificationVM()
                {
                    SenderName = tmp[0].Name,
                    SenderEmail = tmp[0].Email,
                    Id = notification.Id,
                    Content = notification.Content,
                    SenderId = notification.SenderId,
                    SenderRole = "student",
                });
            }

            return Json(new { state = true, msg = "Success", data = new { senders, count = senders.Count } });
        }

        [HttpPost]
        public IActionResult notificationRespond([FromForm] int n_id, [FromForm] bool accept)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                return Json(new { state = false, msg = "failed" });
            }
            var notification = DB.profNotifications.SingleOrDefault(n => n.ProfId == currentUser.Id && n.Id == n_id);
            if (notification != null && notification.Content.ToLower().Contains("request"))
            {
                var team = DB.Teams.Where(t => t.Id == notification.TeamId).Select(t => new { t.Id, t.Name, t.LeaderID, t.CourseID }).ToList();
                if (team.Count == 0)
                    return Json(new { state = false, msg = "Team not found", });
                notification.IsReaded = true;
                DB.profNotifications.Update(notification);
                DB.SaveChanges();
                var std = DB.Students.SingleOrDefault(s => s.Id == notification.SenderId);
                if (std != null)
                {
                    var prof = DB.Proffessors.SingleOrDefault(s => s.Id == currentUser.Id);
                    var teamCount = DB.Teams.Where(p => p.ProfID == currentUser.Id).Select(t => t.Id).ToList().Count();

                    NotificationModel newNotification = new NotificationModel()
                    {
                        SenderId = (int)currentUser.Id,
                        StudentId = std.Id,
                        TeamId = notification.TeamId,
                        Content = (!accept || teamCount >= prof.TeamCount) ? "Rejected" : "Accepted",
                        SenderRole = currentUser.Role,
                    };
                    DB.Notifications.Add(newNotification);
                    DB.SaveChanges();
                    if (accept && teamCount < prof.TeamCount)
                    {
                        DB.Teams.Update(new TeamModel()
                        {
                            Id = team[0].Id,
                            ProfID = currentUser.Id,
                            Name = team[0].Name,
                            CourseID = team[0].CourseID,
                            LeaderID = team[0].LeaderID,
                        });
                        DB.SaveChanges();
                    }
                }
                return getNotification();
            }
            return Json(new { state = false, msg = "Failed" });
        }

    }
}
