using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentProj.Attributes;
using StudentProj.DTO;
using StudentProj.Models;
using StudentProj.Repository;

namespace StudentProj.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Super Admin")] // Only Super Admin can manage permissions!
    [HasPermission("manage:permissions")]
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionRepository _permissionRepo;
        private readonly IRoleRepository _roleRepo;
        private readonly IValidator<PermissionDTO> _validator;

        public PermissionsController(
            IPermissionRepository permissionRepo,
            IRoleRepository roleRepo,
            IValidator<PermissionDTO> validator)
        {
            _permissionRepo = permissionRepo;
            _roleRepo = roleRepo;
            _validator = validator;
        }

        // GET all active permissions
        [HttpGet("GetAllPermissions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Permissions>>> GetAllPermissions()
        {
            var permissions = await _permissionRepo.GetAllPermissionsAsync();
            return Ok(permissions);
        }

        // 1. Create a Permission
        [HttpPost("CreatePermission")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> CreatePermission([FromBody] PermissionDTO dto)
        {
            // Validate input permission string
            var validation = await _validator.ValidateAsync(dto);
            if (!validation.IsValid)
                return BadRequest(validation.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

            // Check if permission already exists
            var exists = await _permissionRepo.PermissionExistsAsync(dto.PermissionName);
            if (exists)
                return BadRequest($"Permission '{dto.PermissionName}' already exists!");

            var permission = new Permissions
            {
                PermissionName = dto.PermissionName.ToLower()
            };

            var created = await _permissionRepo.CreatePermissionAsync(permission);
            return Created("", created);
        }

        // 2. Assign Permission to Role
        [HttpPost("AssignPermissionToRole")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> AssignPermissionToRole([FromBody] AssignPermissionDTO dto)
        {
            // Check if Role exists
            var role = await _roleRepo.GetRoleByIdAsync(dto.RoleId);
            if (role == null)
                return NotFound($"Role with ID {dto.RoleId} not found!");

            // Check if Permission exists
            var permission = await _permissionRepo.GetPermissionByIdAsync(dto.PermissionId);
            if (permission == null)
                return NotFound($"Permission with ID {dto.PermissionId} not found!");

            // Map them together
            var success = await _permissionRepo.AssignPermissionToRoleAsync(dto.RoleId, dto.PermissionId);
            if (!success)
                return BadRequest("This permission is already assigned to this role!");

            return Ok($"Permission '{permission.PermissionName}' assigned to role '{role.RoleName}' successfully.");
        }

        [HttpPut("UpdatePermission/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdatePermission(int id, [FromBody] PermissionDTO dto)
        {
            if (id <= 0) return BadRequest("Invalid permission id!");

            // Validate the new name
            var validation = await _validator.ValidateAsync(dto);
            if (!validation.IsValid)
                return BadRequest(validation.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

            var existing = await _permissionRepo.GetPermissionByIdAsync(id);
            if (existing == null) return NotFound($"Permission with ID {id} not found.");

            // Check if new name already exists elsewhere
            var nameExists = await _permissionRepo.PermissionExistsAsync(dto.PermissionName);
            if (nameExists && !existing.PermissionName.Equals(dto.PermissionName, StringComparison.OrdinalIgnoreCase))
                return BadRequest($"Permission '{dto.PermissionName}' already exists!");

            existing.PermissionName = dto.PermissionName.ToLower();
            await _permissionRepo.UpdatePermissionRoleAsync(id, existing);

            return NoContent();
        }

        [HttpDelete("DeletePermission/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeletePermission(int id)
        {
            if (id <= 0) return BadRequest("Invalid permission id!");

            var result = await _permissionRepo.DeletePermissionAsync(id);
            if (!result) return NotFound($"Permission with ID {id} not found.");

            return Ok("Permission soft-deleted successfully.");
        }

        [HttpDelete("RemovePermissionFromRole")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> RemovePermissionFromRole([FromBody] AssignPermissionDTO dto)
        {
            var result = await _permissionRepo.RemovePermissionFromRoleAsync(dto.RoleId, dto.PermissionId);
            if (!result) return NotFound("Mapping not found or already deleted.");

            return Ok("Permission revoked from role successfully.");
        }
    }

}