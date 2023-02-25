using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Graduate_Project_BackEnd
{
    public class Jwt
    {
        IConfiguration configuration;
        public Jwt(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public string JwtCreation(List<Claim> claims)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(configuration["Jwt:Issuer"],
             configuration["Jwt:Audience"],
             claims,
             expires: DateTime.Now.AddDays(1),
             signingCredentials: credentials);
            Console.WriteLine("ahmed galal");
            Console.WriteLine("mohamed Ahmed");
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
