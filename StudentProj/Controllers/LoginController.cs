using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using StudentProj.DTO;
using StudentProj.Repository;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace StudentProj.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ILoginRepository _login;
        private readonly IConfiguration _config;
        public LoginController(ILoginRepository login,IConfiguration config) 
        {
            _login = login;
            _config = config;
        }
        [HttpPost("Login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Login(LoginDTO dto)
        {
            // find student by email
            var student = await _login.GetStudentbyemailasync(dto.Email);
            if (student == null)
                return Unauthorized("Invalid email or password.");

            // verify password using BCrypt
            bool isValid = BCrypt.Net.BCrypt.Verify(dto.Password, student.PasswordHash);
            if (!isValid)
                return Unauthorized("Invalid email or password.");

            // generate token
            var token = GenerateToken(student);
            return Ok(new { token = token });
        }
        private string GenerateToken(Models.Student student)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["JWT-Token"]));

            var credentials = new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("Id", student.Id.ToString()),
                new Claim("Email", student.Email),
                new Claim("Name", student.Name)
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
