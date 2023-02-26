using Graduate_Project_BackEnd.Models;
using Graduate_Project_BackEnd.ViewModel;
using Microsoft.AspNetCore.Mvc;
using PusherServer;
using System.Security.Claims;

namespace Graduate_Project_BackEnd.Controllers
{

    [Route("[controller]/[action]")]
    [ApiController]
    public class NotificationController : Controller
    {
        DBCONTEXT DB;
        public NotificationController(DBCONTEXT dB)
        {
            DB = dB;
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

        #region  send request to team leader
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
                var team = DB.Teams.Where(t => t.Id == notification.TeamId && t.CourseID == notification.CourseId).Select(t => new { t.LeaderID }).ToList();
                if (team.Count > 0)
                {
                    var course_std = DB.Courses_Students.SingleOrDefault(cs => cs.Student.Email == currentUser.Email && cs.CourseID == notification.CourseId && cs.TeamID == null);
                    if (course_std != null)
                    {
                        var std = DB.Students.SingleOrDefault(s => s.Id == notification.SenderId);
                        if (std == null)
                            return Json(new { state = false, msg = "failed to send request" });
                        var notifications = DB.Notifications.Where(n => n.TeamId == notification.TeamId && n.SenderId == currentUser.Id && n.Content.Equals("Request")).ToList();
                        if (notifications.Count == 0)
                        {
                            DB.Notifications.Add(new NotificationModel()
                            {
                                SenderId = notification.SenderId,
                                StudentId = team[0].LeaderID,
                                TeamId = notification.TeamId,
                                Content = "Request",
                                SenderRole = currentUser.Role,
                            });
                            DB.SaveChanges();
                            return getNotification();
                        }
                        return Json(new { state = false, msg = "Request is already sent" });
                    }
                    return Json(new { state = false, msg = "You are already in team" });
                }
            }
            return Json(new { state = false, msg = "failed" });
        }
        #endregion

        #region get user notification
        [HttpPost]
        public IActionResult getNotification()
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                return Json(new { state = false, msg = "failed" });

            }

            var std = DB.Students.SingleOrDefault(s => s.Email.Equals(currentUser.Email));
            if (std == null)
                return Json(new { state = false, msg = "failed to get student data" });
            var notifications = DB.Notifications.Where(n => n.StudentId == std.Id && n.IsReaded == false).Select(n => new { n.Id, n.SenderId, n.Content, n.SenderRole }).ToList();
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
                    SenderRole = notification.SenderRole,
                    SenderId = notification.SenderId
                });
            }

            return Json(new { state = true, msg = "Success", data = new { senders, count = senders.Count } });
        }
        #endregion

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
            await pusher.TriggerAsync("my-channel", "my-event",new{});
        }

        #region  team leader respond to request
        [HttpPost]
        public IActionResult notificationRespond([FromForm] int n_id, [FromForm] bool accept)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                return Json(new { state = false, msg = "failed" });
            }
            var notification = DB.Notifications.SingleOrDefault(n => n.Student.Email.Equals(currentUser.Email) && n.Id == n_id);
            if (notification != null)
            {
                notification.IsReaded = true;
                DB.Notifications.Update(notification);
                DB.SaveChanges();
                var std = DB.Students.SingleOrDefault(s => s.Id == notification.SenderId);
                if (std != null)
                {
                    NotificationModel newNotification = new NotificationModel()
                    {
                        SenderId = (int)currentUser.Id,
                        StudentId = std.Id,
                        TeamId = notification.TeamId,
                        Content = accept ? "Accepted" : "Rejected",
                        SenderRole = currentUser.Role,
                    };
                    DB.Notifications.Add(newNotification);
                    DB.SaveChanges();
                    if (accept)
                    {
                        var team = DB.Teams.Where(t => t.Id == notification.TeamId).Select(t => t.CourseID).ToList();
                        if (team.Count > 0)
                        {
                            var cour_Std = DB.Courses_Students.SingleOrDefault(cs => cs.StudentID == std.Id && cs.CourseID == team[0]);
                            if (cour_Std != null)
                            {
                                cour_Std.TeamID = notification.TeamId;
                                DB.Courses_Students.Update(cour_Std);
                                DB.SaveChanges();
                            }
                        }
                    }
                }
                return getNotification();
            }
            return Json(new { state = false, msg = "Failed" });
        }

        #endregion

        #region        User delete notification
        [HttpPost]
        public IActionResult delete([FromForm] int id)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                return Json(new { state = false, msg = "failed" });

            }
            var notfication = DB.Notifications.SingleOrDefault(n => n.Id == id && n.Student.Email.Equals(currentUser.Email));
            if (notfication != null)
            {
                DB.Notifications.Remove(notfication);
                DB.SaveChanges();
                return getNotification();
            }
            return Json(new { state = false, msg = "Failed" });
        }
        #endregion
    }
}
