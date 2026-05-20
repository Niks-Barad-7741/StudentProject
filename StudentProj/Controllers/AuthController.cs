using Microsoft.AspNetCore.Mvc;
using StudentProj.DTO;
using StudentProj.Models;
using StudentProj.Repository;
using BCrypt.Net;

namespace StudentProj.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _auth;

        public AuthController(IAuthRepository auth) 
        {
            _auth = auth;
        }

        [HttpPost("Register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Register(RegisterDTO dto)
        {
            var existing = await _auth.GetStudentbyemailasync(dto.Email);
            if (existing != null)
                return BadRequest("Email already registered.");

            var student = new Student
            {
                Name = dto.Name,
                Email = dto.Email,
                Address = dto.Address,
                Phone = dto.Phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            await _auth.RegisterAsync(student);
            return Ok(student);
        }
    }
}
