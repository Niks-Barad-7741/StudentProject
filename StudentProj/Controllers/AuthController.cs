using BCrypt.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudentProj.Attributes;
using StudentProj.DTO;
using StudentProj.Enums;
using StudentProj.Models;
using StudentProj.Repository;
using StudentProj.Services;
using StudentProj.Common;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace StudentProj.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IRegisterRepository _auth;
        private readonly ILoginRepository _login;
        private readonly JwtService _JWT_service;
        private readonly ILoggingService _logging;

        public AuthController(
            IRegisterRepository auth,
            ILoginRepository login,
            JwtService JWT_service,
            ILoggingService logging)
        {
            _auth = auth;
            _login = login;
            _JWT_service = JWT_service;
            _logging = logging;
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Register(RegisterDTO dto)
        {
            // check Phone Number exists
            var existing = await _auth.GetStudentbyphoneasync(dto.Phone);
            if (existing != null)
            {
                await _logging.LogActivityAsync(dto.Name, dto.Email, "Registration Failed: Phone number already registered", HttpContext);
                var errorResponse = ApiResponse<object>.Create(ResponseStatus.UserAlreadyExist, "Phone number already registered!");
                return StatusCode(errorResponse.StatusCodes, errorResponse);
            }

            // create student
            var student = new Student
            {
                Name = dto.Name,
                Email = dto.Email,
                Address = dto.Address,
                Phone = dto.Phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "User", // Default for self-registration
                IpAddress = IpHelper.GetClientIpAddress(HttpContext)
            };

            await _auth.RegisterAsync(student);

            var studentRole = await _auth.GetRoleByIdAsync(3);
            if (studentRole != null)
                await _auth.AssignRoleAsync(student.Id, studentRole.Id);

            var roles = await _auth.GetStudentRolesAsync(student.Id);
            var token = _JWT_service.GenerateToken(student, roles);

            await _logging.LogActivityAsync(student.Name, student.Email, "Registration Succeeded", HttpContext);

            // Standardize return payload to match login format (using ApiResponse<LoginResponseDTO>)
            var authData = new LoginResponseDTO
            {
                Token = token
            };
            var response = ApiResponse<LoginResponseDTO>.Create(ResponseStatus.UserRegisterSuccessfully, authData);
            return StatusCode(response.StatusCodes, response);
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Login(LoginDTO dto)
        {
            // find student
            var student = await _login.GetStudentbyemailasync(dto.Email);
            if (student == null)
            {
                await _logging.LogActivityAsync("Anonymous", dto.Email, "Login Failed: Invalid Email", HttpContext);
                var errorResponse = ApiResponse<object>.Create(ResponseStatus.InvalidCredentials, "Invalid email.");
                return StatusCode(errorResponse.StatusCodes, errorResponse);
            }

            // verify password
            bool isValid = BCrypt.Net.BCrypt.Verify(dto.Password, student.PasswordHash);
            if (!isValid)
            {
                await _logging.LogActivityAsync("Anonymous", dto.Email, "Login Failed: Invalid Password", HttpContext);
                var errorResponse = ApiResponse<object>.Create(ResponseStatus.InvalidCredentials, "Invalid Password.");
                return StatusCode(errorResponse.StatusCodes, errorResponse);
            }

            var roles = await _login.GetStudentRolesAsync(student.Id);
            var token = _JWT_service.GenerateToken(student, roles);

            // Return standardized ApiResponse wrapped around LoginResponseDTO (token only)
            await _logging.LogActivityAsync(student.Name, student.Email, "Login Succeeded", HttpContext);
            var authData = new LoginResponseDTO
            {
                Token = token
            };
            var response = ApiResponse<LoginResponseDTO>.Create(ResponseStatus.UserLoginSuccessfully, authData);
            return StatusCode(response.StatusCodes, response);
        }
    }
}
