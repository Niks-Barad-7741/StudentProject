using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudentProj.Attributes;
using StudentProj.DTO;
using StudentProj.Models;
using StudentProj.Repository;
using StudentProj.Enums;
using StudentProj.Common;
using StudentProj.Services;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System;

namespace StudentProj.Controllers
{
    [Route("api/roles")]
    [ApiController]
    [HasPrivilege("manage:roles")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleRepository _role;
        private readonly IRegisterRepository _auth;
        private readonly ILoggingService _logging;
        private readonly IValidator<RoleDTO> _validator;

        public RoleController(
            IRoleRepository role,
            IRegisterRepository auth,
            ILoggingService logging,
            IValidator<RoleDTO> validator)
        {
            _role = role;
            _auth = auth;
            _logging = logging;
            _validator = validator;
        }

        // GET all roles
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllRoles()
        {
            var roles = await _role.GetAllRolesAsync();

            // map to response DTO
            var response = roles.Select(r =>
                new RoleResponseDTO
                {
                    Id = r.Id,
                    RoleName = r.RoleName
                }).ToList();

            var success = ApiResponse<IEnumerable<RoleResponseDTO>>.Create(ResponseStatus.RoleRetriveSuccessfully, response);
            return StatusCode(success.StatusCodes, success);
        }

        // GET role by id
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetRoleById(int id)
        {
            if (id <= 0)
            {
                var error = ApiResponse<object>.Create(ResponseStatus.BadRequest, "Invalid role id!");
                return StatusCode(error.StatusCodes, error);
            }

            var role = await _role.GetRoleByIdAsync(id);
            if (role == null)
            {
                var error = ApiResponse<object>.Create(ResponseStatus.RoleNotFound, $"Role with id {id} not found!");
                return StatusCode(error.StatusCodes, error);
            }

            var responseDTO = new RoleResponseDTO
            {
                Id = role.Id,
                RoleName = role.RoleName
            };

            var success = ApiResponse<RoleResponseDTO>.Create(ResponseStatus.RoleRetriveSuccessfully, responseDTO);
            return StatusCode(success.StatusCodes, success);
        }

        // POST create role
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> CreateRole([FromBody] RoleDTO dto)
        {
            // fluent validation
            var validation = await _validator.ValidateAsync(dto);
            if (!validation.IsValid)
            {
                var errorDetails = validation.Errors.Select(e => new { Field = e.PropertyName, Message = e.ErrorMessage }).ToList();
                var error = ApiResponse<object>.FailureResponse("Validation failed.", 400, errorDetails);
                return StatusCode(error.StatusCodes, error);
            }

            // check duplicate - case insensitive
            var exists = await _role.RoleExistsAsync(dto.RoleName);
            if (exists)
            {
                var error = ApiResponse<object>.Create(ResponseStatus.BadRequest, $"Role '{dto.RoleName}' already exists!");
                return StatusCode(error.StatusCodes, error);
            }

            // create role
            var role = new Roles
            {
                RoleName = char.ToUpper(dto.RoleName[0]) + dto.RoleName.Substring(1).ToLower()
            };

            var created = await _role.CreateRoleAsync(role);
            var responseDTO = new RoleResponseDTO
            {
                Id = created.Id,
                RoleName = created.RoleName
            };

            var success = ApiResponse<RoleResponseDTO>.Create(ResponseStatus.RoleCreatedSuccessfully, responseDTO);
            return CreatedAtAction(nameof(GetRoleById), new { id = created.Id }, success);
        }

        // DELETE role
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteRole(int id)
        {
            if (id <= 0)
            {
                var error = ApiResponse<object>.Create(ResponseStatus.BadRequest, "Invalid role id!");
                return StatusCode(error.StatusCodes, error);
            }

            var result = await _role.DeleteRoleAsync(id);
            if (!result)
            {
                var error = ApiResponse<object>.Create(ResponseStatus.RoleNotFound, $"Role with id {id} not found!");
                return StatusCode(error.StatusCodes, error);
            }

            var success = ApiResponse<object>.Create(ResponseStatus.RoleDeletedSuccessfully);
            return StatusCode(success.StatusCodes, success);
        }

        // PUT update role
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateRole(int id, [FromBody] RoleDTO dto) 
        {
            if (id <= 0)
            {
                var error = ApiResponse<object>.Create(ResponseStatus.BadRequest, "Invalid role id!");
                return StatusCode(error.StatusCodes, error);
            }

            var validation = await _validator.ValidateAsync(dto);
            if (!validation.IsValid) 
            {
                var errorDetails = validation.Errors.Select(e => new { Field = e.PropertyName, Message = e.ErrorMessage }).ToList();
                var error = ApiResponse<object>.FailureResponse("Validation failed.", 400, errorDetails);
                return StatusCode(error.StatusCodes, error);
            }
            var existingRole = await _role.GetRoleByIdAsync(id);
            if (existingRole == null) 
            {
                var error = ApiResponse<object>.Create(ResponseStatus.RoleNotFound, $"Role with id {id} not found!");
                return StatusCode(error.StatusCodes, error);
            }
            string formattedName = char.ToUpper(dto.RoleName[0]) + dto.RoleName.Substring(1).ToLower();

            var exists = await _role.RoleExistsAsync(formattedName);
            if (exists && !existingRole.RoleName.Equals(formattedName, StringComparison.OrdinalIgnoreCase))
            {
                var error = ApiResponse<object>.Create(ResponseStatus.BadRequest, $"Role '{formattedName}' already exists!");
                return StatusCode(error.StatusCodes, error);
            }

            existingRole.RoleName = formattedName;
            await _role.UpdateRoleAsync(id, existingRole);

            var success = ApiResponse<object>.Create(ResponseStatus.RoleUpdatedSuccessfully);
            return StatusCode(success.StatusCodes, success);
        }

        // POST assign roles to student
        [HttpPost("assign")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> AssignRole(AssignRoleDTO dto)
        {
            var student = await _auth.GetStudentByIdAsync(dto.StudentId);
            if (student == null)
            {
                var error = ApiResponse<object>.Create(ResponseStatus.UserNotFound, "Student not found.");
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

        // POST revoke roles from student
        [HttpPost("revoke")]
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