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
        private readonly ILoggingService _logging;

        public RegisterController(
            IRegisterRepository auth,
            JwtService JWT_service,
            IPrivilegeRepository privilege,
            ILoggingService logging) 
        {
            _auth = auth;
            _JWT_service = JWT_service;
            _privilege = privilege;
            _logging = logging;
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
            {
                await _logging.LogActivityAsync(dto.Email, "Registration Failed: Phone number already registered", HttpContext);
                // return BadRequest("Phone number already registered!");
                return BadRequest(new FailResponseDTO 
                { 
                    statusCodes = (int)Enums.ResponseStatus.BadRequest, 
                    message = "Phone number already registered!" 
                });
            }

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

            await _logging.LogActivityAsync(student.Email, "Registration Succeeded", HttpContext);
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
            {
                // return NotFound("Student not found.");
                return NotFound(new FailResponseDTO 
                { 
                    statusCodes = (int)Enums.ResponseStatus.NotFound, 
                    message = "Student not found." 
                });
            }

            // check valid roles from comma-separated string
            if (string.IsNullOrWhiteSpace(dto.RoleIds))
            {
                return BadRequest(new FailResponseDTO
                {
                    statusCodes = (int)Enums.ResponseStatus.BadRequest,
                    message = "Role IDs must not be empty."
                });
            }

            List<int> roleIdsList;
            try
            {
                roleIdsList = dto.RoleIds.Split(',')
                    .Select(r => int.Parse(r.Trim()))
                    .ToList();
            }
            catch (FormatException)
            {
                return BadRequest(new FailResponseDTO
                {
                    statusCodes = (int)Enums.ResponseStatus.BadRequest,
                    message = "Role IDs must be a comma-separated list of numbers (e.g. '1,2')."
                });
            }

            foreach (var roleId in roleIdsList)
            {
                var role = await _auth.GetRoleByIdAsync(roleId);
                if (role == null)
                {
                    // return BadRequest("Invalid role. Only Admin or User allowed.");
                    return BadRequest(new FailResponseDTO 
                    { 
                        statusCodes = (int)Enums.ResponseStatus.BadRequest, 
                        message = $"Invalid role ID: {roleId}. Only Admin, User, or Super Admin allowed." 
                    });
                }
            }

            // update roles
            /* Original single role assignment commented out:
            await _auth.UpdateStudentRoleAsync(dto.StudentId, dto.RoleId);
            return Ok($"Role {dto.RoleId} assigned successfully.");
            */
            foreach (var roleId in roleIdsList)
            {
                await _auth.UpdateStudentRoleAsync(dto.StudentId, roleId);
            }

            await _logging.LogActivityAsync(
                HttpContext.User.Identity?.Name ?? "Anonymous", 
                $"Assigned roles '{dto.RoleIds}' to student ID {dto.StudentId}.", 
                HttpContext
            );

            return Ok(new BaseResponseDTO
            {
                statusCodes = (int)Enums.ResponseStatus.Success,
                message = $"Roles {dto.RoleIds} assigned successfully to student ID {dto.StudentId}."
            });
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
                // return NotFound("Student Not Found");
                return NotFound(new FailResponseDTO 
                { 
                    statusCodes = (int)Enums.ResponseStatus.NotFound, 
                    message = "Student Not Found" 
                });
            }

            if (string.IsNullOrWhiteSpace(dto.RoleIds))
            {
                return BadRequest(new FailResponseDTO
                {
                    statusCodes = (int)Enums.ResponseStatus.BadRequest,
                    message = "Role IDs must not be empty."
                });
            }

            List<int> roleIdsList;
            try
            {
                roleIdsList = dto.RoleIds.Split(',')
                    .Select(r => int.Parse(r.Trim()))
                    .ToList();
            }
            catch (FormatException)
            {
                return BadRequest(new FailResponseDTO
                {
                    statusCodes = (int)Enums.ResponseStatus.BadRequest,
                    message = "Role IDs must be a comma-separated list of numbers (e.g. '1,2')."
                });
            }

            var revokedRoles = new List<int>();
            var failedRoles = new List<int>();

            foreach (var roleId in roleIdsList)
            {
                var result = await _auth.RevokeRoleAsync(dto.StudentId, roleId);
                if (result)
                {
                    revokedRoles.Add(roleId);
                }
                else
                {
                    failedRoles.Add(roleId);
                }
            }

            if (revokedRoles.Count == 0)
            {
                return NotFound(new FailResponseDTO 
                { 
                    statusCodes = (int)Enums.ResponseStatus.NotFound, 
                    message = "None of the specified roles are currently assigned to the student." 
                });
            }

            await _logging.LogActivityAsync(
                HttpContext.User.Identity?.Name ?? "Anonymous", 
                $"Revoked roles '{string.Join(",", revokedRoles)}' from student ID {dto.StudentId}.", 
                HttpContext
            );

            string successMessage = $"Roles {string.Join(",", revokedRoles)} revoked successfully from Student ID {dto.StudentId}.";
            if (failedRoles.Count > 0)
            {
                successMessage += $" (Note: Roles {string.Join(",", failedRoles)} were not assigned and could not be revoked).";
            }

            return Ok(new BaseResponseDTO
            {
                statusCodes = (int)Enums.ResponseStatus.Success,
                message = successMessage
            });
        }
    }
}
