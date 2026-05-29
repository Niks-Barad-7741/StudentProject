using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentProj.Attributes;
using StudentProj.DTO;
using StudentProj.Models;
using StudentProj.Repository;
using StudentProj.Enums;

namespace StudentProj.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles ="Super Admin")]
    [HasPrivilege("manage:permissions")]
    public class PrivilegesController : ControllerBase
    {
        private readonly IPrivilegeRepository _privilegeRepo;
        private readonly IRoleRepository _roleRepo;
        private readonly IValidator<PrivilegeDTO> _validator;

        public PrivilegesController(
            IPrivilegeRepository privilegeRepo,
            IRoleRepository roleRepo,
            IValidator<PrivilegeDTO> validator)
        {
            _privilegeRepo = privilegeRepo;
            _roleRepo = roleRepo;
            _validator = validator;
        }

        // GET all active Privilege
        [HttpGet("GetAllPrivilege")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllPrivilege()
        {
            var privileges = await _privilegeRepo.GetAllPrivilegeAsync();

            var response = privileges.Select(p => new PrivilegeDTO
            {
               PrivilegeName = p.PrivilegeName
            });

            var success = ApiResponse<IEnumerable<PrivilegeDTO>>.Create(ResponseStatus.UserRetriveSuccessfully, response);
            return StatusCode(success.StatusCodes, success);
        }

        // 1. Create a Privilege
        [HttpPost("CreatePrivilege")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> CreatePermission([FromBody] PrivilegeDTO dto)
        {
            // Validate input permission string
            var validation = await _validator.ValidateAsync(dto);
            if (!validation.IsValid)
            {
                var errorDetails = validation.Errors.Select(e => new { Field = e.PropertyName, Message = e.ErrorMessage }).ToList();
                var error = ApiResponse<object>.FailureResponse("Validation failed.", 400, errorDetails);
                return StatusCode(error.StatusCodes, error);
            }

            // Check if permission already exists
            var exists = await _privilegeRepo.PrivilegeExistsAsync(dto.PrivilegeName);
            if (exists)
            {
                var error = ApiResponse<object>.Create(ResponseStatus.BadRequest, $"Permission '{dto.PrivilegeName}' already exists!");
                return StatusCode(error.StatusCodes, error);
            }

            var permission = new Privileges
            {
                PrivilegeName = dto.PrivilegeName.ToLower()
            };

            var created = await _privilegeRepo.CreatePrivilegeAsync(permission);
            var success = ApiResponse<Privileges>.Create(ResponseStatus.RoleCreatedSuccessfully, created);
            return Created("", success);
        }

        // 2. Assign Privilege to Role
        [HttpPost("AssignPrivilegeToRole")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> AssignPrivilegeToRole([FromBody] AssignPrivilegeDTO dto)
        {
            // Check if Role exists
            var role = await _roleRepo.GetRoleByIdAsync(dto.RoleId);
            if (role == null)
            {
                var error = ApiResponse<object>.Create(ResponseStatus.RoleNotFound, $"Role with ID {dto.RoleId} not found!");
                return StatusCode(error.StatusCodes, error);
            }

            // Check if Privilege exists
            var privilege = await _privilegeRepo.GetPrivilegeByIdAsync(dto.PrivilegeId);
            if (privilege == null)
            {
                var error = ApiResponse<object>.Create(ResponseStatus.PrivilegeNotFound, $"Privilege with ID {dto.PrivilegeId} not found!");
                return StatusCode(error.StatusCodes, error);
            }

            // Map them together
            var result = await _privilegeRepo.AssignPrivilegeToRoleAsync(dto.RoleId, dto.PrivilegeId);
            if (!result)
            {
                var error = ApiResponse<object>.Create(ResponseStatus.BadRequest, "This privilege is already assigned to this role!");
                return StatusCode(error.StatusCodes, error);
            }

            var success = ApiResponse<object>.Create(ResponseStatus.PrivilegeAssignedSuccessfully, $"Privilege '{privilege.PrivilegeName}' assigned to role '{role.RoleName}' successfully.");
            return StatusCode(success.StatusCodes, success);
        }

        [HttpPut("UpdatePrivilege/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdatePrivilege(int id, [FromBody] PrivilegeDTO dto)
        {
            if (id <= 0) 
            {
                var error = ApiResponse<object>.Create(ResponseStatus.BadRequest, "Invalid privilege id!");
                return StatusCode(error.StatusCodes, error);
            }

            // Validate the new name
            var validation = await _validator.ValidateAsync(dto);
            if (!validation.IsValid)
            {
                var errorDetails = validation.Errors.Select(e => new { Field = e.PropertyName, Message = e.ErrorMessage }).ToList();
                var error = ApiResponse<object>.FailureResponse("Validation failed.", 400, errorDetails);
                return StatusCode(error.StatusCodes, error);
            }

            var existing = await _privilegeRepo.GetPrivilegeByIdAsync(id);
            if (existing == null) 
            {
                var error = ApiResponse<object>.Create(ResponseStatus.PrivilegeNotFound, $"Privilege with ID {id} not found.");
                return StatusCode(error.StatusCodes, error);
            }

            // Check if new name already exists elsewhere
            var nameExists = await _privilegeRepo.PrivilegeExistsAsync(dto.PrivilegeName);
            if (nameExists && !existing.PrivilegeName.Equals(dto.PrivilegeName, StringComparison.OrdinalIgnoreCase))
            {
                var error = ApiResponse<object>.Create(ResponseStatus.BadRequest, $"Privilege '{dto.PrivilegeName}' already exists!");
                return StatusCode(error.StatusCodes, error);
            }

            existing.PrivilegeName = dto.PrivilegeName.ToLower();
            await _privilegeRepo.UpdatePrivilegeRoleAsync(id, existing);

            var success = ApiResponse<object>.Create(ResponseStatus.UserUpdatedSuccessfully, "Privilege updated successfully.");
            return StatusCode(success.StatusCodes, success);
        }

        [HttpDelete("DeletePermission/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeletePrivilege(int id)
        {
            if (id <= 0) 
            {
                var error = ApiResponse<object>.Create(ResponseStatus.BadRequest, "Invalid privilege id!");
                return StatusCode(error.StatusCodes, error);
            }

            var result = await _privilegeRepo.DeletePrivilegeAsync(id);
            if (!result) 
            {
                var error = ApiResponse<object>.Create(ResponseStatus.PrivilegeNotFound, $"Privilege with ID {id} not found.");
                return StatusCode(error.StatusCodes, error);
            }

            var success = ApiResponse<object>.Create(ResponseStatus.UserSoftDeleteSuccessfully, "Privilege soft-deleted successfully.");
            return StatusCode(success.StatusCodes, success);
        }

        [HttpDelete("RemovePrivilegeFromRole")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> RemovePrivilegeFromRole([FromBody] AssignPrivilegeDTO dto)
        {
            var result = await _privilegeRepo.RemovePrivilegeFromRoleAsync(dto.RoleId, dto.PrivilegeId);
            if (!result) 
            {
                var error = ApiResponse<object>.Create(ResponseStatus.PrivilegeNotFound, "Mapping not found or already deleted.");
                return StatusCode(error.StatusCodes, error);
            }

            var success = ApiResponse<object>.Create(ResponseStatus.PrivilegeRevokedSuccessfully, "Privilege revoked from role successfully.");
            return StatusCode(success.StatusCodes, success);
        }
    }
}