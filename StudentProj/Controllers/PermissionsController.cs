using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudentProj.Attributes;
using StudentProj.DTO;
using StudentProj.Models;
using StudentProj.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StudentProj.Repository_Interface;
using AutoMapper;

namespace StudentProj.Controllers
{
    [Route("api/permissions")]
    [ApiController]
    [Authorize]
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionRepository _permissionRepo;
        private readonly IRoleRepository _roleRepo;
        private readonly IValidator<PermissionDTO> _validator;
        private readonly IMapper _mapper;

        public PermissionsController(
            IPermissionRepository permissionRepo,
            IRoleRepository roleRepo,
            IValidator<PermissionDTO> validator,
            IMapper mapper)
        {
            _permissionRepo = permissionRepo;
            _roleRepo = roleRepo;
            _validator = validator;
            _mapper = mapper;
        }

        // GET all active Permission
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllPermission()
        {
            var permissions = await _permissionRepo.GetAllPermissionAsync();

            var response = _mapper.Map<IEnumerable<PermissionDTO>>(permissions);

            var success = ApiResponse<IEnumerable<PermissionDTO>>.Create(ResponseStatus.PermissionRetriveSuccessfully, response);
            return StatusCode(success.StatusCodes, success);
        }

        // 1. Create a Permission
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> CreatePermission([FromBody] PermissionDTO dto)
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
            var exists = await _permissionRepo.PermissionExistsAsync(dto.PermissionName);
            if (exists)
            {
                var error = ApiResponse<object>.Create(ResponseStatus.BadRequest, $"Permission '{dto.PermissionName}' already exists!");
                return StatusCode(error.StatusCodes, error);
            }

            var permission = _mapper.Map<Permissions>(dto);
            permission.PermissionName = dto.PermissionName.ToLower();

            var created = await _permissionRepo.CreatePermissionAsync(permission);
            var success = ApiResponse<Permissions>.Create(ResponseStatus.RoleCreatedSuccessfully, created);
            return Created("", success);
        }

        // 2. Assign Permission to Role
        [HttpPost("assign")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> AssignPermissionToRole([FromBody] AssignPermissionDTO dto)
        {
            // Check if Role exists
            var role = await _roleRepo.GetRoleByIdAsync(dto.RoleId);
            if (role == null)
            {
                var error = ApiResponse<object>.Create(ResponseStatus.RoleNotFound, $"Role with ID {dto.RoleId} not found!");
                return StatusCode(error.StatusCodes, error);
            }

            if (string.IsNullOrWhiteSpace(dto.PermissionIds))
            {
                var error = ApiResponse<object>.Create(ResponseStatus.BadRequest, "Permission IDs must not be empty.");
                return StatusCode(error.StatusCodes, error);
            }

            List<int> permissionIdsList;
            try
            {
                permissionIdsList = dto.PermissionIds.Split(',')
                    .Select(r => int.Parse(r.Trim()))
                    .ToList();
            }
            catch (FormatException)
            {
                var error = ApiResponse<object>.Create(ResponseStatus.BadRequest, "Permission IDs must be a comma-separated list of numbers (e.g. '1,2').");
                return StatusCode(error.StatusCodes, error);
            }

            // Optional: validate all permissions exist
            foreach(var pid in permissionIdsList)
            {
                var permission = await _permissionRepo.GetPermissionByIdAsync(pid);
                if (permission == null)
                {
                    var error = ApiResponse<object>.Create(ResponseStatus.PermissionNotFound, $"Permission with ID {pid} not found!");
                    return StatusCode(error.StatusCodes, error);
                }
            }

            // Map them together
            int successCount = 0;
            foreach(var pid in permissionIdsList)
            {
                var result = await _permissionRepo.AssignPermissionToRoleAsync(dto.RoleId, pid, dto.MenuId);
                if(result) successCount++;
            }

            if (successCount == 0)
            {
                var error = ApiResponse<object>.Create(ResponseStatus.BadRequest, "These permissions are already assigned to this role for this menu!");
                return StatusCode(error.StatusCodes, error);
            }

            var success = ApiResponse<object>.Create(ResponseStatus.PermissionAssignedSuccessfully, $"{successCount} Permissions assigned to role '{role.RoleName}' successfully.");
            return StatusCode(success.StatusCodes, success);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdatePermission(int id, [FromBody] PermissionDTO dto)
        {
            if (id <= 0) 
            {
                var error = ApiResponse<object>.Create(ResponseStatus.BadRequest, "Invalid permission id!");
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

            var existing = await _permissionRepo.GetPermissionByIdAsync(id);
            if (existing == null) 
            {
                var error = ApiResponse<object>.Create(ResponseStatus.PermissionNotFound, $"Permission with ID {id} not found.");
                return StatusCode(error.StatusCodes, error);
            }

            // Check if new name already exists elsewhere
            var nameExists = await _permissionRepo.PermissionExistsAsync(dto.PermissionName);
            if (nameExists && !existing.PermissionName.Equals(dto.PermissionName, StringComparison.OrdinalIgnoreCase))
            {
                var error = ApiResponse<object>.Create(ResponseStatus.BadRequest, $"Permission '{dto.PermissionName}' already exists!");
                return StatusCode(error.StatusCodes, error);
            }

            _mapper.Map(dto, existing);
            existing.PermissionName = dto.PermissionName.ToLower();
            await _permissionRepo.UpdatePermissionRoleAsync(id, existing);

            var success = ApiResponse<object>.Create(ResponseStatus.UserUpdatedSuccessfully, "Permission updated successfully.");
            return StatusCode(success.StatusCodes, success);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeletePermission(int id)
        {
            if (id <= 0) 
            {
                var error = ApiResponse<object>.Create(ResponseStatus.BadRequest, "Invalid permission id!");
                return StatusCode(error.StatusCodes, error);
            }

            var result = await _permissionRepo.DeletePermissionAsync(id);
            if (!result) 
            {
                var error = ApiResponse<object>.Create(ResponseStatus.PermissionNotFound, $"Permission with ID {id} not found.");
                return StatusCode(error.StatusCodes, error);
            }

            var success = ApiResponse<object>.Create(ResponseStatus.UserSoftDeleteSuccessfully, "Permission soft-deleted successfully.");
            return StatusCode(success.StatusCodes, success);
        }

        [HttpDelete("revoke")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> RemovePermissionFromRole([FromBody] AssignPermissionDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.PermissionIds))
            {
                var error = ApiResponse<object>.Create(ResponseStatus.BadRequest, "Permission IDs must not be empty.");
                return StatusCode(error.StatusCodes, error);
            }

            List<int> permissionIdsList;
            try
            {
                permissionIdsList = dto.PermissionIds.Split(',')
                    .Select(r => int.Parse(r.Trim()))
                    .ToList();
            }
            catch (FormatException)
            {
                var error = ApiResponse<object>.Create(ResponseStatus.BadRequest, "Permission IDs must be a comma-separated list of numbers (e.g. '1,2').");
                return StatusCode(error.StatusCodes, error);
            }

            int successCount = 0;
            foreach(var pid in permissionIdsList)
            {
                var result = await _permissionRepo.RemovePermissionFromRoleAsync(dto.RoleId, pid, dto.MenuId);
                if (result) successCount++;
            }

            if (successCount == 0) 
            {
                var error = ApiResponse<object>.Create(ResponseStatus.PermissionNotFound, "Mappings not found or already deleted.");
                return StatusCode(error.StatusCodes, error);
            }

            var success = ApiResponse<object>.Create(ResponseStatus.PermissionRevokedSuccessfully, $"{successCount} Permissions revoked from role successfully.");
            return StatusCode(success.StatusCodes, success);
        }
    }
}