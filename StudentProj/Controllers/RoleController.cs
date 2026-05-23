// Controllers/RoleController.cs
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentProj.DTO;
using StudentProj.Models;
using StudentProj.Repository;
using System.Data;

namespace StudentProj.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] // ✅ Admin only!
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
        public async Task<ActionResult<IEnumerable<RoleResponseDTO>>> GetAllRoles()
        {
            var roles = await _role.GetAllRolesAsync();

            // map to response DTO
            var response = roles.Select(r =>
                new RoleResponseDTO
                {
                    Id = r.Id,
                    RoleName = r.RoleName
                }).ToList();

            return Ok(response);
        }

        // GET role by id
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RoleResponseDTO>>
            GetRoleById(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid role id!");

            var role = await _role.GetRoleByIdAsync(id);
            if (role == null)
                return NotFound(
                    $"Role with id {id} not found!");

            return Ok(new RoleResponseDTO
            {
                Id = role.Id,
                RoleName = role.RoleName
            });
        }

        // POST create role
        [HttpPost("CreateRole")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RoleResponseDTO>>
            CreateRole([FromBody] RoleDTO dto)
        {
            // fluent validation
            var validation = await _validator
                .ValidateAsync(dto);
            if (!validation.IsValid)
                return BadRequest(validation.Errors
                    .Select(e => new
                    {
                        Field = e.PropertyName,
                        Message = e.ErrorMessage
                    }));

            // ✅ check duplicate - case insensitive
            var exists = await _role
                .RoleExistsAsync(dto.RoleName);
            if (exists)
                return BadRequest(
                    $"Role '{dto.RoleName}' already exists!");

            // create role
            var role = new Roles
            {
                // ✅ capitalize first letter
                RoleName = char.ToUpper(dto.RoleName[0])
                    + dto.RoleName.Substring(1).ToLower()
            };

            var created = await _role.CreateRoleAsync(role);

            return CreatedAtAction(
                nameof(GetRoleById),
                new { id = created.Id },
                new RoleResponseDTO
                {
                    Id = created.Id,
                    RoleName = created.RoleName
                });
        }

        // DELETE role
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteRole(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid role id!");

            var result = await _role.DeleteRoleAsync(id);
            if (!result)
                return NotFound(
                    $"Role with id {id} not found!");

            return Ok("Role deleted successfully!");
        }
    }
}