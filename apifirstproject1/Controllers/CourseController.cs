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
    public class CourseController : Controller
    {
        DBCONTEXT DB;
        public CourseController(DBCONTEXT dB)
        {
            DB = dB;
        }
        [Authorize(Roles = "admin")]
        [HttpPost]
        public IActionResult Add([FromBody] CourseVM course)
        {
            List<string> data = new List<string>();
            var found = DB.Courses.SingleOrDefault(c => c.Name.Equals(course.Name));
            if (found != null)
            {
                return Json(new { state = false, msg = "Found", data = data });
            }
            CourseModel newcourse = new CourseModel()
            {
                Name = course.Name,
                Desciption = course.Desciption,
                InstructorID = course.InstructorID,
                IsGraduate = course.IsGraduate,
            };
            DB.Courses.Add(newcourse);
            DB.SaveChanges();
            return Json(new { state = true, msg = "Success" });
        }

        //[HttpGet]
        //public IActionResult Display()
        //{
        //    var Courses = DB.Courses.OrderBy(c => c.Name).ToList();
        //    if (Courses != null)
        //        return Json(new { state = true, msg = "Success", data = Courses });
        //    return Json(new { state = false, msg = "failed", data = Courses });
        //}

        [Authorize(Roles = "student,instructor")]
        [HttpPost]
        public IActionResult Display()
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null || currentUser.Id == null)
            {
                return Json(new { state = false, msg = "failed" });
            }
            if (currentUser.Role.Equals("student"))
            {
                var std = DB.Students.SingleOrDefault(s => s.Id == currentUser.Id);
                if (std != null)
                {
                    var Courses = DB.Courses_Students.Where(cs => cs.StudentID == std.Id).Include(c => c.Course).Select(cs => new { cs.Course.Id, cs.Course.Name, cs.Course.Desciption }).OrderBy(c => c.Name).ToList();
                    return Json(new { state = true, msg = "Success", data = Courses });
                }
            }
            else
            {
                var courses = DB.Courses.Where(c => c.InstructorID == currentUser.Id).Select(c => new { c.Id, c.Name, c.Desciption }).OrderBy(c => c.Name).ToList();
                return Json(new { state = true, msg = "Success", data = courses });
            }
            return Json(new { state = false, msg = "failed" });
        }

        [Authorize(Roles = "admin")]
        [HttpGet]

        public IActionResult DisplayAll()
        {
            var Courses = DB.Instructors.SelectMany(i => i.Courses, (inst, cours) => new { cours.Name, InstructorID = inst.Id, cours.Desciption, InstructorName = inst.Name }).ToList();
            if (Courses != null)
                return Json(new { state = true, msg = "Success", data = Courses });
            return Json(new { state = false, msg = "failed", data = Courses });
        }

        [Authorize(Roles = "student,instructor")]
        [HttpPost]
        public IActionResult GetCourse([FromForm] int id)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null || currentUser.Id == null)
            {
                return Json(new { state = false, msg = "failed" });
            }
            var course = DB.Courses.SingleOrDefault(c => c.Id == id);
            if (course != null)
            {
                if (currentUser.Role.Equals("student"))
                    GetStdCourse(id, currentUser);
                else
                    GetInsCourse(id, currentUser);
            }
            return Json(new { state = false, msg = "failed" });
        }

        private IActionResult GetStdCourse(int id, UserLoginVM currentUser)
        {
            var course = DB.Courses.SingleOrDefault(c => c.Id == id);
            var std = DB.Students.SingleOrDefault(s => s.Id == currentUser.Id);
            if (course != null && std != null)
            {
                var std_course = DB.Courses_Students.SingleOrDefault(cs => cs.CourseID == course.Id && cs.StudentID == std.Id);
                if (std_course != null)
                {
                    var availableTeams = DB.Teams.Where(t => t.CourseID == id && !t.IsComplete && t.Id != std_course.TeamID).Select(t => new { t.CourseID, t.IsComplete }).ToList();
                    var availableStd = DB.Courses_Students.Where(cs => cs.StudentID != currentUser.Id && cs.CourseID == course.Id && cs.TeamID == null).Select(t => new { t.CourseID }).ToList();
                    var availablePro = DB.Proffessors.Where(p => p.TeamCount > DB.Teams.Where(t => t.ProfID == p.Id).Select(t => t.Name).ToList().Count).ToList().Count;

                    var myTeam = DB.Courses_Students.Where(cs => cs.StudentID == std.Id && cs.CourseID == course.Id && cs.TeamID != null).Select(t => new { t.TeamID, t.Team.LeaderID, t.Team.ProfID }).ToList();
                    CourseStudentVM studentVM = new()
                    {
                        AvailableTeams = availableTeams == null ? 0 : availableTeams.Count,
                        AvailableStudents = availableStd == null ? 0 : availableStd.Count,
                        MyTeam = myTeam.Count == 0 ? null : myTeam[0].TeamID,
                        TeamLeader = myTeam.Count == 0 ? null : myTeam[0].LeaderID,
                        Name = course.Name,
                        AvailableProffessors = availablePro,
                        Description = course.Desciption,
                        IsGraduate = course.IsGraduate,
                        ProId = myTeam.Count == 0 ? null : myTeam[0].ProfID,
                    };
                    return Json(new { state = true, msg = "Success", data = studentVM });
                }
                return Json(new { state = false, msg = "Failed can not found student in this course" });
            }
            return Json(new { state = false, msg = "failed" });
        }

        private IActionResult GetInsCourse(int id, UserLoginVM currentUser)
        {
            var course = DB.Courses.SingleOrDefault(c => c.Id == id && c.InstructorID == currentUser.Id);
            var ins = DB.Instructors.SingleOrDefault(i => i.Id == currentUser.Id);
            if (course != null)
            {
                var availableTeams = DB.Teams.Where(t => t.CourseID == id).Select(t => new { t.CourseID, t.IsComplete }).ToList();
                var availableStd = DB.Courses_Students.Where(cs => cs.CourseID == course.Id && cs.TeamID == null).Select(t => new { t.CourseID }).ToList();
                CourseStudentVM studentVM = new()
                {
                    AvailableTeams = availableTeams == null ? 0 : availableTeams.Count,
                    AvailableStudents = availableStd == null ? 0 : availableStd.Count,
                    Name = course.Name,
                    Description = course.Desciption,
                    IsGraduate = course.IsGraduate,
                    MinStd = course.MinStd,
                    MaxStd = course.MaxStd,
                };
                return Json(new { state = true, msg = "Success", data = studentVM });
            }
            return Json(new { state = false, msg = "failed" });
        }


        [Authorize(Roles = "admin")]

        [HttpPost]
        public IActionResult Delete([FromForm] string name)
        {
            CourseModel course = DB.Courses.SingleOrDefault(c => c.Name.Equals(name));
            if (course == null)
            {
                return Json(new { state = false, msg = "Not Found" });
            }
            DB.Courses.Remove(course);
            DB.SaveChanges();
            return DisplayAll();
        }
        [Authorize(Roles = "admin")]

        [HttpPut]
        public IActionResult AdminEdit([FromBody] CourseVM course)
        {
            CourseModel oldCourse;
            if (!course.Name.Equals(course.NewName))
            {
                oldCourse = DB.Courses.SingleOrDefault(c => c.Name.Equals(course.NewName));
                if (oldCourse != null)
                    return Json(new { state = false, msg = "Course is already founded" });
            }
            oldCourse = DB.Courses.SingleOrDefault(c => c.Name.Equals(course.Name));
            oldCourse.Name = course.NewName;
            oldCourse.Desciption = course.Desciption;
            oldCourse.InstructorID = course.InstructorID;
            DB.Courses.Update(oldCourse);
            DB.SaveChanges();
            return DisplayAll();
        }
        [Authorize(Roles = "admin")]

        [HttpPost]
        public IActionResult AddCSV([FromForm] IFormFile readexcel)
        {
            int cnt = 0;
            var reader = new StreamReader(readexcel.OpenReadStream());
            var header = (reader.ReadLine().ToLower()).Split(',').ToList();
            while (reader.Peek() >= 0)
            {
                var course = (reader.ReadLine()).Split(',').ToList();
                var found = DB.Courses.SingleOrDefault(s => s.Name.Equals(course[header.IndexOf("name")]));
                if (found == null)
                {
                    var instructor = DB.Instructors.SingleOrDefault(i => i.Email.Equals(course[header.IndexOf("instructor")]));
                    if (instructor != null)
                    {
                        DB.Courses.Add(new CourseModel() { Name = course[header.IndexOf("name")], Desciption = course[header.IndexOf("description")], InstructorID = instructor.Id, IsGraduate = bool.Parse(course[header.IndexOf("isgraduate")]) });
                        DB.SaveChanges();
                        cnt++;
                    }
                }
            }
            return Json(new { msg = $"{cnt}", state = cnt > 0 ? true : false });
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

        [HttpPost]
        [Authorize(Roles = "instructor")]

        public IActionResult InstructorEdit([FromForm] int id, [FromForm] int min, [FromForm] int max)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null || currentUser.Id == null)
            {
                return Json(new { state = false, msg = "failed" });
            }
            var course = DB.Courses.SingleOrDefault(c => c.Id == id && c.InstructorID == currentUser.Id);
            if (course != null)
            {
                course.MinStd = min;
                course.MaxStd = max;
                DB.Courses.Update(course);
                DB.SaveChanges();
                return Json(new { state = true, msg = "Success" });
            }
            return Json(new { state = false, msg = "Failed to update" });
        }
    }
}
