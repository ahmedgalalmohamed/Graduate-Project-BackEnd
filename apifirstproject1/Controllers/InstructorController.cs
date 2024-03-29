﻿using Graduate_Project_BackEnd.Models;
using Graduate_Project_BackEnd.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Graduate_Project_BackEnd.Controllers
{
    // [Authorize(Roles = "admin")]
    [Route("[controller]/[action]")]
    [ApiController]
    public class InstructorController : Controller
    {
        DBCONTEXT DB;
        public InstructorController(DBCONTEXT dB)
        {
            DB = dB;
        }
        [HttpGet]
        public IActionResult Display()
        {
            var Instructors = DB.Instructors.OrderBy(i => i.Name).Select(i => new { i.Id, i.Name, i.Email, i.Password }).ToList();
            if (Instructors != null)
                return Json(new { state = true, msg = "Success", data = Instructors });
            return Json(new { state = false, msg = "failed", data = Instructors });
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
        public IActionResult Add([FromBody] InstructorVM instructor)
        {

            InstructorModel found = DB.Instructors.SingleOrDefault(i => i.Email.Equals(instructor.Email));
            if (found != null)
                return Json(new { state = false, msg = "Instructor found" });
            InstructorModel newInstructor = new InstructorModel();
            newInstructor.Name = instructor.Name;
            newInstructor.Email = instructor.Email;
            newInstructor.Password = instructor.Password;
            newInstructor.img = ImageConverter.Converter("wwwroot/default-avatar.png");
            DB.Instructors.Add(newInstructor);
            DB.SaveChanges();
            return Json(new { state = true, msg = "Success" });
        }

        [HttpPut]
        public IActionResult AdminEdit([FromBody] InstructorVM instructor)
        {

            InstructorModel oldInstructor = DB.Instructors.SingleOrDefault(i => i.Email.Equals(instructor.Email));
            if (oldInstructor == null)
                return Json(new { state = false, msg = "Not Found" });
            oldInstructor.Name = instructor.Name;
            oldInstructor.Password = instructor.Password;
            DB.Instructors.Update(oldInstructor);
            DB.SaveChanges();
            return Display();
        }
        [HttpDelete]
        public IActionResult Delete([FromForm] string email)
        {

            InstructorModel found = DB.Instructors.SingleOrDefault(i => i.Email.Equals(email));
            if (found == null)
                return Json(new { state = false, msg = "Not Found" });
            DB.Instructors.Remove(found);
            DB.SaveChanges();
            return Display();
        }

        [HttpPost]
        public IActionResult AddCSV([FromForm] IFormFile readexcel)
        {
            int cnt = 0;

            var reader = new StreamReader(readexcel.OpenReadStream());
            var header = (reader.ReadLine().ToLower()).Split(',').ToList();
            string image = ImageConverter.Converter("wwwroot/default-avatar.png");
            while (reader.Peek() >= 0)
            {
                var prof = (reader.ReadLine()).Split(',').ToList();
                var found = DB.Instructors.SingleOrDefault(s => s.Email.Equals(prof[header.IndexOf("email")]));
                if (found == null)
                {
                    cnt++;
                    DB.Instructors.Add(new InstructorModel() { Name = prof[header.IndexOf("name")], Email = prof[header.IndexOf("email")], Password = prof[header.IndexOf("password")], img = image
                });
                    DB.SaveChanges();
                }
            }
            return Json(new { msg = $"{cnt}", state = cnt > 0 ? true : false });
        }

        [Authorize(Roles = "instructor")]
        [HttpPost]
        public IActionResult MyTeams([FromForm] int id)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null || currentUser.Id == null || !currentUser.Role.Equals("instructor"))
            {
                return Json(new { state = false, msg = "failed" });
            }
            var teams = DB.Teams.Where(t => t.CourseID == id).Select(t => new { t.Id, t.Name, leader = t.Leader.Name }).ToList();
            var courseName = DB.Courses.SingleOrDefault(c => c.Id == id).Name;
            return Json(new { state = true, msg = "Success", data = new { teams, courseName } });
        }
    }
}
