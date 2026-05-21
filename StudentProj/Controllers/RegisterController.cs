using Microsoft.AspNetCore.Mvc;
using StudentProj.DTO;
using StudentProj.Models;
using StudentProj.Repository;
using BCrypt.Net;

namespace StudentProj.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class RegisterController : ControllerBase
    {
        private readonly IRegisterRepository _auth;

        public RegisterController(IRegisterRepository auth) 
        {
            _auth = auth;
        }

        [HttpPost("Register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        //public async Task<ActionResult> Register(RegisterDTO dto)
        //{
        //    var existing = await _auth.GetStudentbyemailasync(dto.Email);
        //    if (existing != null)
        //        return BadRequest("Email already registered.");

        //    var student = new Student
        //    {
        //        Name = dto.Name,
        //        Email = dto.Email,
        //        Address = dto.Address,
        //        Phone = dto.Phone,
        //        PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        //    };

        //    await _auth.RegisterAsync(student);
        //    return Ok(student);
        //}
        public async Task<ActionResult<RegisterResponseDTO>> Register(
            RegisterDTO dto)
        {
            // check email exists
            var existing = await _auth
                .GetStudentbyemailasync(dto.Email);
            if (existing != null)
                return BadRequest("Email already registered.");

            // create student
            var student = new Student
            {
                Name = dto.Name,
                Email = dto.Email,
                Address = dto.Address,
                Phone = dto.Phone,
                PasswordHash = BCrypt.Net.BCrypt
                    .HashPassword(dto.Password)
            };

            await _auth.RegisterAsync(student);

            // ✅ assign Student role by default
            var studentRole = await _auth
                .GetRoleByNameAsync("User");
            if (studentRole != null)
                await _auth.AssignRoleAsync(
                    student.Id, studentRole.Id);

            return Ok(new RegisterResponseDTO
            {
                Name = student.Name,
                Email = student.Email,
                Role = studentRole.RoleName
            });
        }

        // ✅ Admin only - assign role to existing student
        //[HttpPost("AssignRole")]
        //[Microsoft.AspNetCore.Authorization.Authorize(
        //    Roles = "Admin")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //public async Task<ActionResult> AssignRole(
        //    AssignRoleDTO dto)
        //{
        //    await _auth.UpdateStudentRoleAsync(
        //        dto.StudentId, dto.RoleName);
        //    return Ok($"Role {dto.RoleName} assigned!");
        //}
        [HttpPost("AssignRole")]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> AssignRole(AssignRoleDTO dto)
        {
            // check student exists
            var student = await _auth.GetStudentByIdAsync(dto.StudentId);
            if (student == null)
                return NotFound("Student not found.");

            // check valid role
            var role = await _auth.GetRoleByNameAsync(dto.RoleName);
            if (role == null)
                return BadRequest("Invalid role. Only Admin or User allowed.");

            // update role
            await _auth.UpdateStudentRoleAsync(dto.StudentId, dto.RoleName);

            return Ok($"Role {dto.RoleName} assigned successfully.");
        }
    }
}
