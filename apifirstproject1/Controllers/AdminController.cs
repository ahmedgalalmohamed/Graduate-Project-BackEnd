using Graduate_Project_BackEnd.Models;
using Graduate_Project_BackEnd.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Graduate_Project_BackEnd.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class AdminController : Controller
    {
        DBCONTEXT DB;
        Jwt jwt;
        public AdminController(DBCONTEXT dB, IConfiguration configuration)
        {
            DB = dB;
            jwt = new Jwt(configuration);
        }
        [HttpPost]
        public IActionResult Login([FromBody] LoginVM adminVM)
        {
            var admin = DB.Admin.SingleOrDefault(a => a.Email == adminVM.Email && a.Password == adminVM.Password);

            if (admin != null)
            {
                List<Claim> claims = new() {
                     new Claim(ClaimTypes.Email,admin.Email),
                     new Claim(ClaimTypes.Name, admin.Name),
                     new Claim(ClaimTypes.Role, "admin"),
                    };
                string token = jwt.JwtCreation(claims);
                UserLoginVM data = new UserLoginVM()
                {
                    Email = admin.Email,
                    Name = admin.Name,
                };
                return Json(new { state = true, msg = "success", token, data });
            }
            return Json(new { state = false, msg = "Failed" });
        }
        [HttpGet]
        [Authorize(Roles = "admin")]
        public IActionResult Profile()
        {
            try
            {
                var currentAdmin = GetCurrentUser();
                if (currentAdmin != null)
                {
                    return Json(new { state = true, msg = "success", data = currentAdmin });

                }
            }
            catch { return Json(new { state = false, msg = "Token Invalid" }); }
            return Json(new { state = false, msg = "Not Found" });
        }
        private AdminModel GetCurrentUser()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (identity != null)
            {
                var userClaims = identity.Claims;

                return new AdminModel
                {
                    Email = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Email).Value,
                    Name = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Name).Value,
                };
            }
            return null;
        }
    }
}

