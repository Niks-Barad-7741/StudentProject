using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using StudentProj.Attributes;
using StudentProj.DTO;
using StudentProj.Models;
using StudentProj.Repository;
using StudentProj.Services;

namespace StudentProj.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class RegisterController : ControllerBase
    {
        private readonly IRegisterRepository _auth;
        private readonly JwtService _JWT_service;
        private readonly IPrivilegeRepository _privilege;

        public RegisterController(
            IRegisterRepository auth,
            JwtService JWT_service,
            IPrivilegeRepository privilege) 
        {
            _auth = auth;
            _JWT_service = JWT_service;
            _privilege = privilege;
        }

        [HttpPost("Register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RegisterResponseDTO>> Register(
            RegisterDTO dto)
        {
            // check Phone Number exists
            var existing = await _auth
                .GetStudentbyphoneasync(dto.Phone);
            if (existing != null)
                return BadRequest("Phone number already registered!");

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

            var studentRole = await _auth
                .GetRoleByIdAsync(3);
            if (studentRole != null)
                await _auth.AssignRoleAsync(
                    student.Id, studentRole.Id);

            var roles = await _auth.GetStudentRolesAsync(student.Id);
            var privileges = await _privilege.GetPrivilegeByRoleNamesAsync(roles);
            var token = _JWT_service.GenerateToken(student, roles, privileges);

            return Ok(new RegisterResponseDTO
            {
                Status = (int)Enums.ResponseStatus.Success,
                Message = "Registration successful!",
                Name = student.Name,
                Token = token,
                Email = student.Email,
                Role = studentRole.RoleName
            });

            /* Original RegisterResponseDTO format commented out:
            return Ok(new RegisterResponseDTO
            {
                Name = student.Name,
                Token = token,
                Email = student.Email,
                Role = studentRole.RoleName
            });
            */
        }


        [HttpPost("AssignRole")]
        //[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Super Admin,Admin")]
        [HasPrivilege("manage:roles")]
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
            var role = await _auth.GetRoleByIdAsync(dto.RoleId);
            if (role == null)
                return BadRequest("Invalid role. Only Admin or User allowed.");

            // update role
            await _auth.UpdateStudentRoleAsync(dto.StudentId, dto.RoleId);

            return Ok($"Role {dto.RoleId} assigned successfully.");
        }

        [HttpPost("RevokeRole")]
        [HasPrivilege("manage:roles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> RevokeRole([FromBody] AssignRoleDTO dto) 
        {
            var student = await _auth.GetStudentByIdAsync(dto.StudentId);
            if (student == null)
            {
                return NotFound("Student Not Found");
            }

            var result = await _auth.RevokeRoleAsync(dto.StudentId, dto.RoleId);
            if (!result)
            {
                return NotFound("This role is not currently assigned to the student.");
            }
            return Ok($"Role {dto.RoleId} revoked successfully from Student ID {dto.StudentId}.");
        }
    }
}
