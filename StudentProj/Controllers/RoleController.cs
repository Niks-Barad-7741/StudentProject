using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentProj.Attributes;
using StudentProj.DTO;
using StudentProj.Models;
using StudentProj.Repository;
using StudentProj.Enums;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace StudentProj.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [HasPrivilege("manage:roles")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleRepository _role;
        private readonly IValidator<RoleDTO> _validator;

        public RoleController(
            IRoleRepository role,
            IValidator<RoleDTO> validator)
        {
            _role = role;
            _validator = validator;
        }

        // GET all roles
        [HttpGet("GetAllRoles")]
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

            var success = ApiResponse<IEnumerable<RoleResponseDTO>>.Create(ResponseStatus.UserRetriveSuccessfully, response);
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

            var success = ApiResponse<RoleResponseDTO>.Create(ResponseStatus.UserRetriveSuccessfully, responseDTO);
            return StatusCode(success.StatusCodes, success);
        }

        // POST create role
        [HttpPost("CreateRole")]
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

        [HttpPut("UpdateRole/{id}")]
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
    }
}