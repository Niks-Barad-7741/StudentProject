using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using StudentProj.Attributes;
using StudentProj.DTO;
using StudentProj.Models;
using StudentProj.Repository;
using StudentProj.Services;
using StudentProj.Enums;
using System.Security.Claims;
using StudentProj.Common;

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
            var privileges = await _privilege.GetPrivilegeByRoleNamesAsync(roles);
            var token = _JWT_service.GenerateToken(student, roles, privileges);

            await _logging.LogActivityAsync(student.Name, student.Email, "Registration Succeeded", HttpContext);

            // Standardize return payload to match login format (using ApiResponse<LoginResponseDTO>)
            var authData = new LoginResponseDTO
            {
                Token = token
            };
            var response = ApiResponse<LoginResponseDTO>.Create(ResponseStatus.UserRegisterSuccessfully, authData);
            return StatusCode(response.StatusCodes, response);
        }

        [HttpPost("AssignRole")]
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
                var error = ApiResponse<object>.Create(ResponseStatus.UserNotFound, "Student not found.");
                return StatusCode(error.StatusCodes, error);
            }

            // check valid roles from comma-separated string
            if (string.IsNullOrWhiteSpace(dto.RoleIds))
            {
                var error = ApiResponse<object>.Create(ResponseStatus.BadRequest, "Role IDs must not be empty.");
                return StatusCode(error.StatusCodes, error);
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
                var error = ApiResponse<object>.Create(ResponseStatus.BadRequest, "Role IDs must be a comma-separated list of numbers (e.g. '1,2').");
                return StatusCode(error.StatusCodes, error);
            }

            foreach (var roleId in roleIdsList)
            {
                var role = await _auth.GetRoleByIdAsync(roleId);
                if (role == null)
                {
                    var error = ApiResponse<object>.Create(ResponseStatus.RoleNotFound, $"Invalid role ID: {roleId}. Only Admin, User, or Super Admin allowed.");
                    return StatusCode(error.StatusCodes, error);
                }
            }

            foreach (var roleId in roleIdsList)
            {
                await _auth.UpdateStudentRoleAsync(dto.StudentId, roleId);
            }

            var assignerEmail = HttpContext.User.FindFirst("Email")?.Value ?? HttpContext.User.FindFirst(ClaimTypes.Email)?.Value ?? "Anonymous";
            var assignerName = HttpContext.User.FindFirst("Name")?.Value ?? HttpContext.User.FindFirst(ClaimTypes.Name)?.Value ?? "Anonymous";
            await _logging.LogActivityAsync(
                assignerName,
                assignerEmail,
                $"Assigned roles '{dto.RoleIds}' to student ID {dto.StudentId}.", 
                HttpContext
            );

            var success = ApiResponse<object>.Create(ResponseStatus.RoleAssignedSuccessfully, $"Roles {dto.RoleIds} assigned successfully to student ID {dto.StudentId}.");
            return StatusCode(success.StatusCodes, success);
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
                var error = ApiResponse<object>.Create(ResponseStatus.UserNotFound, "Student Not Found");
                return StatusCode(error.StatusCodes, error);
            }

            if (string.IsNullOrWhiteSpace(dto.RoleIds))
            {
                var error = ApiResponse<object>.Create(ResponseStatus.BadRequest, "Role IDs must not be empty.");
                return StatusCode(error.StatusCodes, error);
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
                var error = ApiResponse<object>.Create(ResponseStatus.BadRequest, "Role IDs must be a comma-separated list of numbers (e.g. '1,2').");
                return StatusCode(error.StatusCodes, error);
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
                var error = ApiResponse<object>.Create(ResponseStatus.RoleNotFound, "None of the specified roles are currently assigned to the student.");
                return StatusCode(error.StatusCodes, error);
            }

            var revokerEmail = HttpContext.User.FindFirst("Email")?.Value ?? HttpContext.User.FindFirst(ClaimTypes.Email)?.Value ?? "Anonymous";
            var revokerName = HttpContext.User.FindFirst("Name")?.Value ?? HttpContext.User.FindFirst(ClaimTypes.Name)?.Value ?? "Anonymous";
            await _logging.LogActivityAsync(
                revokerName,
                revokerEmail,
                $"Revoked roles '{string.Join(",", revokedRoles)}' from student ID {dto.StudentId}.", 
                HttpContext
            );

            string successMessage = $"Roles {string.Join(",", revokedRoles)} revoked successfully from Student ID {dto.StudentId}.";
            if (failedRoles.Count > 0)
            {
                successMessage += $" (Note: Roles {string.Join(",", failedRoles)} were not assigned and could not be revoked).";
            }

            var success = ApiResponse<object>.Create(ResponseStatus.RoleRevokedSuccessfully, successMessage);
            return StatusCode(success.StatusCodes, success);
        }
    }
}
