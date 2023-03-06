using Graduate_Project_BackEnd;
using Graduate_Project_BackEnd.Models;
using Graduate_Project_BackEnd.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace Graduate_Project_BackEnd.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class UserController : Controller
    {
        DBCONTEXT DB;
        Jwt jwt;
        public UserController(DBCONTEXT dB, IConfiguration configuration)
        {
            DB = dB;
            jwt = new Jwt(configuration);
        }
        [HttpPost]
        public IActionResult Login([FromBody] UserLoginVM userLogin)
        {
            string role = "";
            string name = "";
            int id = 0;
            switch (userLogin.Role.ToLower())
            {
                case "instructor":
                    InstructorModel inst = DB.Instructors.SingleOrDefault(a => a.Email == userLogin.Email && a.Password == userLogin.Password);
                    role = "instructor";
                    name = (inst == null ? "" : inst.Name);
                    id = (inst == null ? 0 : inst.Id);
                    break;
                case "student":
                    StudentsModel std = DB.Students.SingleOrDefault(a => a.Email == userLogin.Email && a.Password == userLogin.Password);
                    role = "student";
                    name = (std == null ? "" : std.Name);
                    id = (std == null ? 0 : std.Id);

                    break;
                case "proffessor":
                    ProffessorModel prof = DB.Proffessors.SingleOrDefault(a => a.Email == userLogin.Email && a.Password == userLogin.Password);
                    role = "proffessor";
                    name = (prof == null ? "" : prof.Name);
                    id = (prof == null ? 0 : prof.Id);

                    break;
            }

            if (role != "" && name != "")
            {
                List<Claim> claims = new() {
                     new Claim(ClaimTypes.Email,userLogin.Email),
                     new Claim(ClaimTypes.Name, name),
                     new Claim(ClaimTypes.Role, role),
                     new Claim(ClaimTypes.Sid, id.ToString()),
                    };
                string token = jwt.JwtCreation(claims);
                UserLoginVM data = new UserLoginVM()
                {
                    Email = userLogin.Email,
                    Id = id,
                    Name = name,
                    Role = role,
                    Exp = 1,
                };
                return Json(new { state = true, msg = "success", token, data });
            }
            return Json(new { state = false, msg = "Failed" });
        }

        [HttpGet]
        public IActionResult myProfile()
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                return Json(new { state = false, msg = "failed" });
            }
            return Json(new { state = true, msg = "success", data = currentUser });
        }
        [Authorize(Roles = "instructor,student,proffessor")]
        [HttpPost]
        public IActionResult Profile([FromForm] int id, [FromForm] string role)
        {
            string msg = "show";
            try
            {
                var currentUser = GetCurrentUser();
                if (currentUser == null)
                {
                    return Json(new { state = false, msg = "failed" });
                }
                switch (role.ToLower())
                {
                    case "student":
                        var student = DB.Students.Where(s => s.Id == id).Select(s => new { s.Id, s.Name, s.Email, s.Address, s.Phone, s.Desciption, s.img, s.Semester }).ToList();
                        if (student.Count == 0)
                            break;
                        var skills = DB.Skils.Where(s => s.StudentID == student[0].Id);
                        if (currentUser.Id == student[0].Id && currentUser.Role.ToLower() == role.ToLower())
                            msg = "own";
                        return Json(new { state = true, msg = msg, data = new { user = student, skills } });

                    case "instructor":
                        var instructor = DB.Instructors.Where(i => i.Id == id).Select(i => new { i.Id, i.Name, i.Email, i.Address, i.Phone, i.Desciption, i.img }).ToList();
                        if (instructor.Count == 0)
                            break;
                        if (currentUser.Id == instructor[0].Id && currentUser.Role.ToLower() == role.ToLower())
                            msg = "own";
                        return Json(new { state = true, msg = msg, data = new { user = instructor } });

                    case "proffessor":
                        var professor = DB.Proffessors.Where(p => p.Id == id).Select(p => new { p.Id, p.Name, p.Email, p.Address, p.Phone, p.Desciption, p.img, p.TeamCount }).ToList();
                        if (professor.Count == 0)
                            break;
                        if (currentUser.Id == professor[0].Id && currentUser.Role.ToLower() == role.ToLower())
                            msg = "own";
                        return Json(new { state = true, msg = msg, data = new { user = professor } });

                    default:
                        return Json(new { state = false, msg = "Role Invalid" });
                }
                return Json(new { state = false, msg = "Role Invalid or user not found" });
            }
            catch { return Json(new { state = false, msg = "Token Invalid" }); }
            return Json(new { state = false, msg = "Not Found" });
        }

        [Authorize(Roles = "instructor,student,proffessor")]
        [HttpGet]
        public IActionResult GetEditData()
        {
            try
            {
                var currentUser = GetCurrentUser();
                if (currentUser == null)
                {
                    return Json(new { state = false, msg = "failed" });
                }
                switch (currentUser.Role.ToLower())
                {
                    case "student":
                        var student = DB.Students.Where(s => s.Id == currentUser.Id).Select(s => new { address = s.Address, phone = s.Phone, desc = s.Desciption, team_count = 0 }).ToList();
                        if (student.Count == 0)
                            break;
                        return Json(new { state = true, msg = "Success", data = student });

                    case "instructor":
                        var instructor = DB.Instructors.Where(i => i.Id == currentUser.Id).Select(i => new { address = i.Address, phone = i.Phone, desc = i.Desciption, team_count = 0 }).ToList();
                        if (instructor.Count == 0)
                            break;
                        return Json(new { state = true, msg = "Success", data = instructor });

                    case "proffessor":
                        var professor = DB.Proffessors.Where(p => p.Id == currentUser.Id).Select(p => new { address = p.Address, phone = p.Phone, desc = p.Desciption, team_count = p.TeamCount }).ToList();
                        if (professor.Count == 0)
                            break;
                        return Json(new { state = true, msg = "Success", data = professor });

                    default:
                        return Json(new { state = false, msg = "Role Invalid" });
                }
            }
            catch { return Json(new { state = false, msg = "Token Invalid" }); }
            return Json(new { state = false, msg = "User Not Found" });
        }


        [Authorize(Roles = "instructor,student,proffessor")]
        [HttpPost]
        public IActionResult EditProfile([FromBody] UserVM user)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                return Json(new { state = false, msg = "failed" });
            }
            switch (currentUser.Role.ToLower())
            {
                case "student":
                    var student = DB.Students.SingleOrDefault(s => s.Id == currentUser.Id);
                    if (student == null)
                        break;
                    student.Desciption = user.Description;
                    student.Address = user.Address;
                    student.Phone = user.Phone;
                    DB.Students.Update(student);
                    DB.SaveChanges();
                    return Json(new { state = true, msg = "success" });

                case "instructor":
                    var instructor = DB.Instructors.SingleOrDefault(s => s.Id == currentUser.Id);
                    if (instructor == null)
                        break;
                    instructor.Desciption = user.Description;
                    instructor.Address = user.Address;
                    instructor.Phone = user.Phone;
                    DB.Instructors.Update(instructor);
                    DB.SaveChanges();
                    return Json(new { state = true, msg = "success" });

                case "proffessor":
                    var prof = DB.Proffessors.SingleOrDefault(s => s.Id == currentUser.Id);
                    if (prof == null)
                        break;
                    prof.Desciption = user.Description;
                    prof.Address = user.Address;
                    prof.Phone = user.Phone;
                    prof.TeamCount = (int)user.TeamCount;
                    DB.Proffessors.Update(prof);
                    DB.SaveChanges();
                    return Json(new { state = true, msg = "success" });

                default:
                    return Json(new { state = false, msg = "Role Invalid" });
            }
            return Json(new { state = false, msg = "Failed To Update" });
        }
        [Authorize(Roles = "instructor,student,proffessor")]
        [HttpPost]
        public IActionResult EditPassword([FromForm] string oldPass, [FromForm] string newPass)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                return Json(new { state = false, msg = "failed" });
            }
            switch (currentUser.Role.ToLower())
            {
                case "student":
                    var student = DB.Students.SingleOrDefault(s => s.Id == currentUser.Id && s.Password.Equals(oldPass));
                    if (student == null)
                        return Json(new { state = false, msg = "Incorrect Password" });
                    student.Password = newPass;
                    DB.Students.Update(student);
                    DB.SaveChanges();
                    return Json(new { state = true, msg = "success" });

                case "instructor":
                    var instructor = DB.Instructors.SingleOrDefault(s => s.Id == currentUser.Id);
                    if (instructor == null)
                        return Json(new { state = false, msg = "Incorrect Password" });
                    instructor.Password = newPass;
                    DB.Instructors.Update(instructor);
                    DB.SaveChanges();
                    return Json(new { state = true, msg = "success" });

                case "proffessor":
                    var prof = DB.Proffessors.SingleOrDefault(s => s.Id == currentUser.Id);
                    if (prof == null)
                        return Json(new { state = false, msg = "Incorrect Password" });
                    prof.Password = newPass;
                    DB.Proffessors.Update(prof);
                    DB.SaveChanges();
                    return Json(new { state = true, msg = "success" });

                default:
                    return Json(new { state = false, msg = "Role Invalid" });
            }
            return Json(new { state = false, msg = "Failed To Update" });
        }
        [HttpPost]
        public IActionResult ChangeImg([FromForm] IFormFile file)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                return Json(new { state = false, msg = "failed" });
            }
            var ms = new MemoryStream();
            file.CopyTo(ms);
            string img = "data:image/png;base64, " + Convert.ToBase64String(ms.ToArray());
            ms.Close();
            switch (currentUser.Role.ToLower())
            {
                case "student":
                    var student = DB.Students.SingleOrDefault(s => s.Id == currentUser.Id);
                    if (student == null)
                        break;
                    student.img = img;
                    DB.Students.Update(student);
                    DB.SaveChanges();
                    return Json(new { state = true, msg = "success", data = img });

                case "instructor":
                    var instructor = DB.Instructors.SingleOrDefault(s => s.Id == currentUser.Id);
                    if (instructor == null)
                        break;
                    instructor.img = img;
                    DB.Instructors.Update(instructor);
                    DB.SaveChanges();
                    return Json(new { state = true, msg = "success", data = img });

                case "proffessor":
                    var prof = DB.Proffessors.SingleOrDefault(s => s.Id == currentUser.Id);
                    if (prof == null)
                        break;
                    prof.img = img;
                    DB.Proffessors.Update(prof);
                    DB.SaveChanges();
                    return Json(new { state = true, msg = "success", data = img });

                default:
                    return Json(new { state = false, msg = "Role Invalid" });
            }
            return Json(new { state = false, msg = "Failed To Update" });
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
