using Graduate_Project_BackEnd.ViewModel;
using Graduate_Project_BackEnd.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Graduate_Project_BackEnd.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class TeamController : Controller
    {
        DBCONTEXT DB;
        public TeamController(DBCONTEXT dB)
        {
            DB = dB;
        }
        [Authorize(Roles = "student")]
        [HttpPost]
        public IActionResult AddTeam([FromBody] TeamVM team)
        {
            var std = DB.Students.SingleOrDefault(s => s.Email.Equals(team.LeaderEmail));
            if (std != null)
            {
                var courseStudent = DB.Courses_Students.SingleOrDefault(cs => cs.StudentID == std.Id && cs.CourseID == team.CourseId);
                if (courseStudent == null)
                    return Json(new { state = false, msg = "Failed Not found student in this course" });
                if (courseStudent.TeamID != null)
                    return Json(new { state = false, msg = "Failed The Student is already in team" });
                var teamfound = DB.Teams.Where(s => s.Name.Equals(team.TeamName) && s.CourseID == team.CourseId).Select(t => new { t.Name, t.Id }).ToList();
                if (teamfound.Count > 0)
                    return Json(new { state = false, msg = "Team Name is already founded" });
                DB.Teams.Add(new TeamModel()
                {
                    Name = team.TeamName,
                    CourseID = team.CourseId,
                    LeaderID = std.Id,
                });
                DB.SaveChanges();
                teamfound = DB.Teams.Where(s => s.Name.Equals(team.TeamName) && s.CourseID == team.CourseId).Select(t => new { t.Name, t.Id }).ToList();
                DB.Projects.Add(new ProjectModel()
                {
                    Name = team.ProName,
                    Desciption = team.ProDescription,
                    TeamID = teamfound[0].Id,
                });
                DB.SaveChanges();

                courseStudent.TeamID = teamfound[0].Id;
                DB.Courses_Students.Update(courseStudent);
                DB.SaveChanges();
                return Json(new { state = true, msg = "Added Successfully" });
            }
            return Json(new { state = false, msg = "Failed to Add" });
        }
        [Authorize(Roles = "student")]
        [HttpPost]
        public IActionResult GetAvailableTeams([FromForm] int id, [FromForm] string email)
        {
            var course = DB.Courses.SingleOrDefault(c => c.Id == id);
            var std = DB.Students.SingleOrDefault(s => s.Email.Equals(email));
            if (course != null && std != null)
            {
                var std_course = DB.Courses_Students.SingleOrDefault(cs => cs.CourseID == course.Id && cs.StudentID == std.Id);
                if (std_course != null)
                {
                    var teams = DB.Teams.Where(t => t.CourseID == course.Id && !t.IsComplete && t.Id != std_course.TeamID).Select(t => new { TeamID = t.Id, t.Name, leader = DB.Students.SingleOrDefault(s => s.Id == t.LeaderID).Name });

                    //DB.Courses_Students.Where(cs => cs.CourseID == course.Id && cs.Team.IsComplete == false && cs.TeamID != std_course.TeamID && cs.StudentID == cs.Team.LeaderID).Select(cs => new { cs.TeamID, cs.Team.Name, leader = cs.Student.Name }).ToList();
                    return Json(new { state = true, msg = "Success", data = teams });
                }
                return Json(new { state = false, msg = "failed" });
            }
            return Json(new { state = false, msg = "failed" });
        }
        [Authorize(Roles = "student,proffessor,instructor")]
        [HttpPost]
        public IActionResult getMyTeam([FromForm] int id)
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
                var team = DB.Teams.Where(t => t.Id == id && t.ProfID == currentUser.Id).Select(t => t.Id).ToList();
                if (team.Count == 0)
                    return Json(new { state = false, msg = "Failed to get data" });
            }
            else
            {
                var team = DB.Teams.Where(t => t.Id == id && t.Course.InstructorID == currentUser.Id).Select(t => t.Id).ToList();
                if (team.Count == 0)
                    return Json(new { state = false, msg = "Failed to get data" });
            }
            var leader = DB.Courses_Students.Where(cs => cs.TeamID == id && cs.Team.LeaderID == cs.StudentID).Select(s => new { s.StudentID, s.Student.Name, s.Student.Email, teamId = s.TeamID, s.CourseID, course = s.Course.Name, team = s.Team.Name, s.Team.IsComplete, role = "student", img = "" });
            var members = DB.Courses_Students.Where(cs => cs.TeamID == id && cs.Team.LeaderID != cs.StudentID).Select(s => new { s.StudentID, s.Student.Name, s.Student.Email, role = "student", img = "" });
            var professor = DB.Teams.Where(t => t.Id == id && t.ProfID != null).Select(p => new { p.Prof.Id, p.Prof.Name, p.Prof.Email, role = "proffessor", img = "" }).ToList();
            var instructor = DB.Courses.Where(c => c.Id == leader.ToList()[0].CourseID).Include(c => c.Instructor).Select(c => new {c.Instructor.Id, c.Instructor.Name, c.Instructor.Email, role = "instructor", img = ""}).ToList();
            if (members != null)
            {
                return Json(new { state = true, msg = "Success", data = new { leader, members, professor = professor.Count == 0 ? null : professor[0], instructor = instructor[0] } });
            }
            return Json(new { state = false, msg = "Failed to get data" });
        }

        [Authorize(Roles = "student")]
        [HttpPost]
        public IActionResult leaveTeam([FromForm] int t_id, [FromForm] int id)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                return Json(new { state = false, msg = "failed" });
            }
            if (currentUser.Id == id)
            {
                var team = DB.Teams.Where(t => t.Id == t_id).Select(t => new { t.Id, t.Name, t.LeaderID, t.CourseID, t.IsComplete }).ToList();
                if (team.Count > 0)
                {
                    var std = DB.Courses_Students.Single(s => s.StudentID == currentUser.Id && s.CourseID == team[0].CourseID);
                    if (std == null)
                        return Json(new { state = false, msg = "can not found student in this course" });
                    std.TeamID = null;
                    DB.Courses_Students.Update(std);
                    DB.SaveChanges();
                    var notifications = DB.Notifications.Where(n => n.TeamId == t_id && (n.SenderId == currentUser.Id || n.StudentId == currentUser.Id) && n.Content.ToLower().Contains("request")).ToList();
                    if (notifications.Count > 0)
                    {
                        DB.Notifications.RemoveRange(notifications);
                        DB.SaveChanges();
                    }
                    DB.Teams.Update(new TeamModel()
                    {
                        Id = team[0].Id,
                        Name = team[0].Name,
                        CourseID = team[0].CourseID,
                        LeaderID = team[0].LeaderID,
                        IsComplete = false,
                    });
                    DB.SaveChanges();
                    return Json(new { state = true, msg = "Done" });
                }
            }
            return Json(new { state = false, msg = "Failed to get data" });
        }

        [HttpPost]
        [Authorize(Roles = "student")]
        public IActionResult SetComplete([FromForm] int id, [FromForm] bool complete)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                return Json(new { state = false, msg = "failed" });
            }
            var team = DB.Teams.Where(t => t.Id == id && t.LeaderID == currentUser.Id).Select(t => new { t.Id, t.Name, t.LeaderID, t.CourseID, t.IsComplete }).ToList();
            if (team.Count > 0)
            {

                var cour_std = DB.Courses_Students.Where(s => s.TeamID == id).Select(c => new { c.Course.MaxStd, c.Course.MinStd }).ToList();
                if ((complete == true && cour_std.Count < cour_std[0].MinStd) || (!complete && cour_std.Count == cour_std[0].MaxStd))
                {
                    return Json(new { state = false, msg = "Unable to change stat", data = team[0].IsComplete });
                }
                DB.Teams.Update(new TeamModel()
                {
                    Id = team[0].Id,
                    Name = team[0].Name,
                    CourseID = team[0].CourseID,
                    LeaderID = team[0].LeaderID,
                    IsComplete = complete,
                });
                DB.SaveChanges();
                return Json(new { state = true, msg = "Done", data = complete });
            }
            return Json(new { state = false, msg = "Can not change stat", data = team[0].IsComplete });
        }

        [HttpPost]
        [Authorize(Roles = "instructor")]
        public IActionResult SetGrade([FromForm] int t_id, [FromForm] int grade)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                return Json(new { state = false, msg = "failed" });
            }
            var auth = DB.Courses_Students.Where(cs => cs.TeamID == t_id && cs.Course.Instructor.Id == currentUser.Id).ToList();
            if (auth.Count != 0)
            {
                var team = DB.Teams.SingleOrDefault(t => t.Id == t_id);
                team.Grade = grade;
                DB.SaveChanges();
                return Json(new { state = true, msg = "Done" });

            }
            return Json(new { state = false, msg = "Failed" });
        }
        [HttpPost]
        [Authorize(Roles = "student,proffessor,instructor")]
        public IActionResult GetGrade([FromForm] int t_id)
        {
            int? grade = null;
            var currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                return Json(new { state = false, msg = "failed" });
            }
            if (currentUser.Role.Equals("proffessor"))
            {
                var team = DB.Teams.Where(t => t.Id == t_id && t.ProfID == currentUser.Id).ToList();
                if (team.Count != 0)
                    grade = team[0].Grade;

            }
            else if (currentUser.Role.Equals("instructor"))
            {
                var team = DB.Teams.Where(t => t.Id == t_id && t.Course.InstructorID == currentUser.Id).ToList();
                if (team.Count != 0)
                    grade = team[0].Grade;
            }
            else
            {
                var team = DB.Courses_Students.Where(cs => cs.StudentID == currentUser.Id && cs.TeamID == t_id).Include(t => t.Team).ToList();
                if (team.Count != 0)
                {
                    grade = team[0].Team.Grade;
                }
            }
            return Json(new { state = true, msg = "Done", data = grade });
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
