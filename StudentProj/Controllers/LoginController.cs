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
        private readonly ILoggingService _logging;

        public LoginController(
            ILoginRepository login,
            JwtService jwtService,
            IPrivilegeRepository privilege,
            ILoggingService logging) 
        {
            _login = login;
            _JWT_service = jwtService;
            _privilege = privilege;
            _logging = logging;
        }

        [HttpPost("Login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Login(LoginDTO dto)
        {
            // find student
            var student = await _login.GetStudentbyemailasync(dto.Email);
            if (student == null)
            {
                await _logging.LogActivityAsync("Anonymous", dto.Email, "Login Failed: Invalid Email", HttpContext);
                var errorResponse = ApiResponse<object>.Create(Enums.ResponseStatus.InvalidCredentials, "Invalid email.");
                return StatusCode(errorResponse.StatusCodes, errorResponse);
            }

            // verify password
            bool isValid = BCrypt.Net.BCrypt.Verify(dto.Password, student.PasswordHash);
            if (!isValid)
            {
                await _logging.LogActivityAsync("Anonymous", dto.Email, "Login Failed: Invalid Password", HttpContext);
                var errorResponse = ApiResponse<object>.Create(Enums.ResponseStatus.InvalidCredentials, "Invalid Password.");
                return StatusCode(errorResponse.StatusCodes, errorResponse);
            }
            //throw new Exception("Invalid Password");

            var roles = await _login.GetStudentRolesAsync(student.Id);
            var privileges = await _privilege.GetPrivilegeByRoleNamesAsync(roles);
            var token = _JWT_service.GenerateToken(student, roles, privileges);

            // Return standardized ApiResponse wrapped around LoginResponseDTO (token only)
            await _logging.LogActivityAsync(student.Name, student.Email, "Login Succeeded", HttpContext);
            var authData = new LoginResponseDTO
            {
                Token = token
            };
            var response = ApiResponse<LoginResponseDTO>.Create(Enums.ResponseStatus.UserLoginSuccessfully, authData);
            return StatusCode(response.StatusCodes, response);

            /* Original LoginResponseDTO format commented out:
            return Ok(new LoginResponseDTO
            {
                StatusCodes = (int)Enums.ResponseStatus.Success,
                Message = "Login successful!",
                Token = token
            });
            */
        }
    }
}
