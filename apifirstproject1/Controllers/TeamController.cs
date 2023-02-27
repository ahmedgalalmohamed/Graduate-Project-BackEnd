﻿using Graduate_Project_BackEnd.ViewModel;
using Graduate_Project_BackEnd.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
                    var teams = DB.Teams.Where(t => t.CourseID == course.Id && !t.IsComplete && t.Id != std_course.TeamID).Select(t => new { TeamID= t.Id, t.Name, leader = DB.Students.SingleOrDefault(s=>s.Id == t.LeaderID).Name });
                        
                        //DB.Courses_Students.Where(cs => cs.CourseID == course.Id && cs.Team.IsComplete == false && cs.TeamID != std_course.TeamID && cs.StudentID == cs.Team.LeaderID).Select(cs => new { cs.TeamID, cs.Team.Name, leader = cs.Student.Name }).ToList();
                    return Json(new { state = true, msg = "Success", data = teams });
                }
                return Json(new { state = false, msg = "failed" });
            }
            return Json(new { state = false, msg = "failed" });
        }
        [Authorize(Roles = "student,proffessor")]
        [HttpPost]
        public IActionResult getMyTeam([FromForm] int id)
        {
            var leader = DB.Courses_Students.Where(cs => cs.TeamID == id && cs.Team.LeaderID == cs.StudentID).Include(t => t.Team).Include(cs => cs.Student).Select(s => new { s.StudentID, s.Student.Name, s.Student.Email });
            var members = DB.Courses_Students.Where(cs => cs.TeamID == id && cs.Team.LeaderID != cs.StudentID).Include(cs => cs.Student).Select(s => new { s.StudentID, s.Student.Name, s.Student.Email });
            if (members != null)
            {
                return Json(new { state = true, msg = "Success", data = new { leader, members } });
            }
            return Json(new { state = false, msg = "Failed to get data" });
        }

    }
}
