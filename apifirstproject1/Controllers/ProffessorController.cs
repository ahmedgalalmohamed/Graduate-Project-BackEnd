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
            var Proffessors = DB.Proffessors.ToList();
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
            var header = (reader.ReadLine()).Split(',').ToList();
            while (reader.Peek() >= 0)
            {
                var prof = (reader.ReadLine()).Split(',').ToList();
                var found = DB.Proffessors.SingleOrDefault(s => s.Email.Equals(prof[header.IndexOf("Email")]));
                if (found == null)
                {
                    cnt++;
                    DB.Proffessors.Add(new ProffessorModel() { Name = prof[header.IndexOf("Name")], Email = prof[header.IndexOf("Email")], Password = prof[header.IndexOf("Password")] });
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
            var Profs = DB.Proffessors.Where(p => p.TeamCount > DB.Teams.Where(t => t.ProfID == p.Id).Select(t => t.Name).ToList().Count).Select(p=>new {p.Name,p.Email,p.Id});

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
    }
}
