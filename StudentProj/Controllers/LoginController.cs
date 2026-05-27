using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudentProj.DTO;
using StudentProj.Repository;
using StudentProj.Services;

namespace StudentProj.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ILoginRepository _login;
        private readonly JwtService _JWT_service;
        private readonly IPrivilegeRepository _privilege;

        public LoginController(
            ILoginRepository login,
            JwtService jwtService,
            IPrivilegeRepository privilege) 
        {
            _login = login;
            _JWT_service = jwtService;
            _privilege = privilege;
        }

        [HttpPost("Login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Login(LoginDTO dto)
        {
            // find student
            var student = await _login.GetStudentbyemailasync(dto.Email);
            if (student == null)
                return Unauthorized("Invalid email or password.");

            // verify password
            bool isValid = BCrypt.Net.BCrypt.Verify(dto.Password, student.PasswordHash);
            if (!isValid)
                return Unauthorized("Invalid email or password.");

            var roles = await _login.GetStudentRolesAsync(student.Id);
            var privileges = await _privilege.GetPrivilegeByRoleNamesAsync(roles);
            var token = _JWT_service.GenerateToken(student, roles, privileges);

            // New format: Return status, message, and token
            return Ok(new LoginResponseDTO
            {
                Status = (int)Enums.ResponseStatus.Success,
                Message = "Login successful!",
                Token = token
            });

            /* Original AuthResponseDTO format commented out:
            return Ok(new AuthResponseDTO
            {
                Name = student.Name,
                Email = student.Email,
                Token = token,
                Role = string.Join(",", roles),
                Message = "Login successful!"
            });
            */
        }
    }
}
