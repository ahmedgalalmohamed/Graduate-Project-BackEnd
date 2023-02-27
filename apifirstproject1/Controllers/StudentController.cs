using Graduate_Project_BackEnd.Models;
using Graduate_Project_BackEnd.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
namespace Graduate_Project_BackEnd.Controllers
{

    [Route("[controller]/[action]")]
    [ApiController]
    public class StudentController : Controller
    {
        DBCONTEXT DB;
        public StudentController(DBCONTEXT dB)
        {
            DB = dB;
        }
        [Authorize(Roles = "admin")]
        [HttpPost]
        public IActionResult Add([FromBody] StudentVM studentModel)
        {

            var found = DB.Students.SingleOrDefault(s => s.Email.Equals(studentModel.Email));
            if (found != null)
            {
                return Json(new { state = false, msg = "Found" });
            }
            StudentsModel student = new()
            {
                Name = studentModel.Name,
                Email = studentModel.Email,
                Password = studentModel.Password,
                Semester = studentModel.Semester,
            };
            DB.Students.Add(student);
            DB.SaveChanges();

            if (studentModel.CoursesID.Count >= 1)
            {
                found = DB.Students.SingleOrDefault(s => s.Email.Equals(studentModel.Email));
                List<Courses_StudentsModel> courses_Students = new List<Courses_StudentsModel>();
                foreach (int stud_cr in studentModel.CoursesID)
                {
                    courses_Students.Add(new Courses_StudentsModel() { StudentID = found.Id, CourseID = stud_cr });
                }
                DB.Courses_Students.AddRange(courses_Students);
                DB.SaveChanges();
            }
            return Json(new { state = true, msg = "Success" });
        }
        [Authorize(Roles = "admin")]
        [HttpGet]
        public IActionResult Display()
        {
            var Students = DB.Students.Include(s => s.Courses).ToList();
            List<StudentVM> StudentVM = new List<StudentVM>();
            foreach (var std in Students)
            {
                var cour_std = DB.Courses_Students.Where(cs => cs.StudentID == std.Id).Select(c => new { c.CourseID }).ToList();
                List<int> courses = new List<int>();
                foreach (var cour in cour_std)
                    courses.Add((int)cour.CourseID);
                StudentVM.Add(new StudentVM() { Name = std.Name, Email = std.Email, Semester = std.Semester, CoursesID = courses, Password = std.Password });
            }
            return Json(new { state = true, msg = "Success", data = StudentVM });
        }
        [Authorize(Roles = "admin")]
        [HttpPost]
        public IActionResult Delete([FromForm] string email)
        {
            StudentsModel std = DB.Students.SingleOrDefault(s => s.Email.Equals(email));
            if (std == null)
            {
                return Json(new { state = false, msg = "Not Found" });
            }
            DB.Students.Remove(std);
            DB.SaveChanges();
            return Display();
        }
        [Authorize(Roles = "admin")]
        [HttpPost]
        public IActionResult AdminEdit([FromBody] StudentVM student)
        {
            StudentsModel oldStudent = DB.Students.SingleOrDefault(s => s.Email.Equals(student.Email));
            if (oldStudent == null)
                return Json(new { state = false, msg = "failed" });
            oldStudent.Name = student.Name;
            if (student.Password != "")
                oldStudent.Password = student.Password;
            oldStudent.Semester = student.Semester;
            DB.Students.Update(oldStudent);
            DB.SaveChanges();
            List<Courses_StudentsModel> oldCS = DB.Courses_Students.Where(cs => cs.StudentID == oldStudent.Id).ToList();
            DB.Courses_Students.RemoveRange(oldCS);
            DB.SaveChanges();
            List<Courses_StudentsModel> newCS = new List<Courses_StudentsModel>();
            foreach (int stud_cr in student.CoursesID)
            {
                newCS.Add(new Courses_StudentsModel() { StudentID = oldStudent.Id, CourseID = stud_cr });
            }
            DB.Courses_Students.AddRange(newCS);
            DB.SaveChanges();
            return Display();
        }
        [Authorize(Roles = "admin")]
        [HttpPost]
        public IActionResult AddCSV([FromForm] IFormFile readexcel)
        {
            List<string> students = new List<string>();
            List<string> courses = new List<string>();
            var reader = new StreamReader(readexcel.OpenReadStream());
            var header = (reader.ReadLine().ToLower()).Split(',').ToList();
            while (reader.Peek() >= 0)
            {
                var std = (reader.ReadLine()).Split(',').ToList();
                var found = DB.Students.SingleOrDefault(s => s.Email.Equals(std[header.IndexOf("Email")]));
                if (found == null)
                {
                    students.Add(std[header.IndexOf("email")]);
                    courses.Add(std[header.IndexOf("course")]);
                    DB.Students.Add(new StudentsModel() { Name = std[header.IndexOf("name")], Email = std[header.IndexOf("email")], Password = std[header.IndexOf("passeord")], Semester = int.Parse(std[header.IndexOf("semester")]) });
                    DB.SaveChanges();
                }
            }
            for (int index = 0; index < students.Count; index++)
            {
                var s = DB.Students.SingleOrDefault(s => s.Email.Equals(students[index]));

                foreach (var cour in courses[index].Split(','))
                {
                    var c = DB.Courses.SingleOrDefault(c => c.Name.Equals(cour));
                    if (c != null)
                    {
                        DB.Courses_Students.Add(new Courses_StudentsModel() { StudentID = s.Id, CourseID = c.Id });
                        DB.SaveChanges();
                    }
                }
            }
            return Json(new { msg = $"{students.Count}", state = students.Count > 0 ? true : false });
        }
        [Authorize(Roles = "student")]
        [HttpPost]
        public IActionResult GetAvailableStudents([FromForm] int id)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null || currentUser.Id == null)
            {
                return Json(new { state = false, msg = "failed" });
            }
            var course = DB.Courses.SingleOrDefault(c => c.Id == id);
            if (course != null)
            {
                var std_course = DB.Courses_Students.Where(cs => cs.StudentID != currentUser.Id && cs.CourseID == course.Id && cs.TeamID == null).Select(cs => new { cs.StudentID, cs.Student.Name, cs.Student.Email });
                if (std_course != null)
                {
                    return Json(new { state = true, msg = "Success", data = std_course });
                }
                return Json(new { state = false, msg = "failed" });
            }
            return Json(new { state = false, msg = "failed" });
        }
        [Authorize(Roles = "student")]
        [HttpPost]
        public IActionResult AddSkill([FromForm] string skill)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null || currentUser.Id == null)
            {
                return Json(new { state = false, msg = "failed" });
            }
            var foundskill = DB.Skils.SingleOrDefault(s => s.skil.ToLower().Equals(skill.ToLower()) && s.StudentID == currentUser.Id);
            if (foundskill != null)
                return Json(new { state = false, msg = "This Skill is already founded" });
            DB.Skils.Add(new SkilsModel() { skil = skill, StudentID = (int)currentUser.Id });
            DB.SaveChanges();
            var skills = DB.Skils.Where(s => s.StudentID == currentUser.Id);
            return Json(new { state = true, msg = "Success", data = skills });
        }
        [Authorize(Roles = "student")]
        [HttpPost]
        public IActionResult DeleteSkill([FromForm] int skill)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null || currentUser.Id == null)
            {
                return Json(new { state = false, msg = "failed" });
            }
            var foundskill = DB.Skils.SingleOrDefault(s => s.Id == skill && s.StudentID == currentUser.Id);
            if (foundskill != null)
            {
                DB.Skils.Remove(foundskill);
                DB.SaveChanges();
                var skills = DB.Skils.Where(s => s.StudentID == currentUser.Id);
                return Json(new { state = true, msg = "Success", data = skills });
            }
            return Json(new { state = false, msg = "failed" });
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

    }
}
