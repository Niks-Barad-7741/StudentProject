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
        public async Task<ActionResult<IEnumerable<Privileges>>> GetAllPrivilege()
        {
            var privileges = await _privilegeRepo.GetAllPrivilegeAsync();

            var response = privileges.Select(p => new PrivilegeDTO
            {
               PrivilegeName=  p.PrivilegeName

            });

            // return Ok(response);
            return Ok(new ApiResponse<IEnumerable<PrivilegeDTO>> 
            { 
                statusCodes = (int)Enums.ResponseStatus.Success, 
                message = "Privileges retrieved successfully.", 
                data = response 
            });
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
                return BadRequest(validation.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

            // Check if permission already exists
            var exists = await _privilegeRepo.PrivilegeExistsAsync(dto.PrivilegeName);
            if (exists)
            {
                // return BadRequest($"Permission '{dto.PrivilegeName}' already exists!");
                return BadRequest(new FailResponseDTO 
                { 
                    statusCodes = (int)Enums.ResponseStatus.BadRequest, 
                    message = $"Permission '{dto.PrivilegeName}' already exists!" 
                });
            }

            var permission = new Privileges
            {
                PrivilegeName = dto.PrivilegeName.ToLower()
            };

            var created = await _privilegeRepo.CreatePrivilegeAsync(permission);
            // return Created("", created);
            return Created("", new ApiResponse<Privileges> 
            { 
                statusCodes = (int)Enums.ResponseStatus.Created, 
                message = "Privilege created successfully.", 
                data = created 
            });
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
                // return NotFound($"Role with ID {dto.RoleId} not found!");
                return NotFound(new FailResponseDTO 
                { 
                    statusCodes = (int)Enums.ResponseStatus.NotFound, 
                    message = $"Role with ID {dto.RoleId} not found!" 
                });
            }

            // Check if Privilege exists
            var privilege = await _privilegeRepo.GetPrivilegeByIdAsync(dto.PrivilegeId);
            if (privilege == null)
            {
                // return NotFound($"Privilege with ID {dto.PrivilegeId} not found!");
                return NotFound(new FailResponseDTO 
                { 
                    statusCodes = (int)Enums.ResponseStatus.NotFound, 
                    message = $"Privilege with ID {dto.PrivilegeId} not found!" 
                });
            }

            // Map them together
            var success = await _privilegeRepo.AssignPrivilegeToRoleAsync(dto.RoleId, dto.PrivilegeId);
            if (!success)
            {
                // return BadRequest("This privilege is already assigned to this role!");
                return BadRequest(new FailResponseDTO 
                { 
                    statusCodes = (int)Enums.ResponseStatus.BadRequest, 
                    message = "This privilege is already assigned to this role!" 
                });
            }

            // return Ok($"Privilege '{privilege.PrivilegeName}' assigned to role '{role.RoleName}' successfully.");
            return Ok(new BaseResponseDTO 
            { 
                statusCodes = (int)Enums.ResponseStatus.Success, 
                message = $"Privilege '{privilege.PrivilegeName}' assigned to role '{role.RoleName}' successfully." 
            });
        }

        [HttpPut("UpdatePrivilege/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdatePrivilege(int id, [FromBody] PrivilegeDTO dto)
        {
            if (id <= 0) 
            {
                // return BadRequest("Invalid privilege id!");
                return BadRequest(new FailResponseDTO 
                { 
                    statusCodes = (int)Enums.ResponseStatus.BadRequest, 
                    message = "Invalid privilege id!" 
                });
            }

            // Validate the new name
            var validation = await _validator.ValidateAsync(dto);
            if (!validation.IsValid)
                return BadRequest(validation.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

            var existing = await _privilegeRepo.GetPrivilegeByIdAsync(id);
            if (existing == null) 
            {
                // return NotFound($"Privilege with ID {id} not found.");
                return NotFound(new FailResponseDTO 
                { 
                    statusCodes = (int)Enums.ResponseStatus.NotFound, 
                    message = $"Privilege with ID {id} not found." 
                });
            }

            // Check if new name already exists elsewhere
            var nameExists = await _privilegeRepo.PrivilegeExistsAsync(dto.PrivilegeName);
            if (nameExists && !existing.PrivilegeName.Equals(dto.PrivilegeName, StringComparison.OrdinalIgnoreCase))
            {
                // return BadRequest($"Privilege '{dto.PrivilegeName}' already exists!");
                return BadRequest(new FailResponseDTO 
                { 
                    statusCodes = (int)Enums.ResponseStatus.BadRequest, 
                    message = $"Privilege '{dto.PrivilegeName}' already exists!" 
                });
            }

            existing.PrivilegeName = dto.PrivilegeName.ToLower();
            await _privilegeRepo.UpdatePrivilegeRoleAsync(id, existing);
            // return NoContent();
            return Ok(new BaseResponseDTO 
            { 
                statusCodes = (int)Enums.ResponseStatus.Success, 
                message = "Privilege updated successfully." 
            });
        }

        [HttpDelete("DeletePermission/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeletePrivilege(int id)
        {
            if (id <= 0) 
            {
                // return BadRequest("Invalid privilege id!");
                return BadRequest(new FailResponseDTO 
                { 
                    statusCodes = (int)Enums.ResponseStatus.BadRequest, 
                    message = "Invalid privilege id!" 
                });
            }

            var result = await _privilegeRepo.DeletePrivilegeAsync(id);
            if (!result) 
            {
                // return NotFound($"Privilege with ID {id} not found.");
                return NotFound(new FailResponseDTO 
                { 
                    statusCodes = (int)Enums.ResponseStatus.NotFound, 
                    message = $"Privilege with ID {id} not found." 
                });
            }

            // return Ok("Privilege soft-deleted successfully.");
            return Ok(new BaseResponseDTO 
            { 
                statusCodes = (int)Enums.ResponseStatus.Success, 
                message = "Privilege soft-deleted successfully." 
            });
        }

        [HttpDelete("RemovePrivilegeFromRole")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> RemovePrivilegeFromRole([FromBody] AssignPrivilegeDTO dto)
        {
            var result = await _privilegeRepo.RemovePrivilegeFromRoleAsync(dto.RoleId, dto.PrivilegeId);
            if (!result) 
            {
                // return NotFound("Mapping not found or already deleted.");
                return NotFound(new FailResponseDTO 
                { 
                    statusCodes = (int)Enums.ResponseStatus.NotFound, 
                    message = "Mapping not found or already deleted." 
                });
            }

            // return Ok("Privilege revoked from role successfully.");
            return Ok(new BaseResponseDTO 
            { 
                statusCodes = (int)Enums.ResponseStatus.Success, 
                message = "Privilege revoked from role successfully." 
            });
        }
    }

}