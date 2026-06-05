using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudentProj.Common;
using StudentProj.DTO;
using StudentProj.Enums;
using StudentProj.Models;
using StudentProj.Repository_Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentProj.Controllers
{
    [Route("api/route-permissions")]
    [ApiController]
    [Authorize]
    public class RoutePermissionsController : ControllerBase
    {
        private readonly IRoutePermissionRepository _repo;
        private readonly IValidator<RoutePermissionDTO> _validator;

        public RoutePermissionsController(IRoutePermissionRepository repo, IValidator<RoutePermissionDTO> validator)
        {
            _repo = repo;
            _validator = validator;
        }

        // GET all route permissions
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAll()
        {
            var list = await _repo.GetAllRoutePermissionsAsync();
            var response = ApiResponse<IEnumerable<RoutePermissions>>.Create(ResponseStatus.UserRetriveSuccessfully, "Route permissions retrieved successfully.", list);
            return StatusCode(response.StatusCodes, response);
        }

        // GET route permission by id
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetById(int id)
        {
            if (id <= 0)
            {
                var error = ApiResponse<object>.Create(ResponseStatus.BadRequest, "Invalid ID!");
                return StatusCode(error.StatusCodes, error);
            }

            var item = await _repo.GetRoutePermissionByIdAsync(id);
            if (item == null)
            {
                var error = ApiResponse<object>.Create(ResponseStatus.PrivilegeNotFound, $"Route permission with ID {id} not found.");
                return StatusCode(error.StatusCodes, error);
            }

            var response = ApiResponse<RoutePermissions>.Create(ResponseStatus.UserRetriveSuccessfully, "Route permission retrieved successfully.", item);
            return StatusCode(response.StatusCodes, response);
        }

        // POST create route permission
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Create([FromBody] RoutePermissionDTO dto)
        {
            var validation = await _validator.ValidateAsync(dto);
            if (!validation.IsValid)
            {
                var errorDetails = validation.Errors.Select(e => new { Field = e.PropertyName, Message = e.ErrorMessage }).ToList();
                var error = ApiResponse<object>.FailureResponse("Validation failed.", 400, errorDetails);
                return StatusCode(error.StatusCodes, error);
            }

            // Check duplicate
            var exists = await _repo.RoutePermissionExistsAsync(dto.HttpMethod, dto.PathPattern);
            if (exists)
            {
                var error = ApiResponse<object>.Create(ResponseStatus.BadRequest, $"Route permission pattern '{dto.HttpMethod} {dto.PathPattern}' already exists!");
                return StatusCode(error.StatusCodes, error);
            }

            var entity = new RoutePermissions
            {
                HttpMethod = dto.HttpMethod.ToUpperInvariant(),
                PathPattern = dto.PathPattern,
                RequiredMenuName = dto.RequiredMenuName,
                RequiredPrivilegeName = dto.RequiredPrivilegeName
            };

            var created = await _repo.CreateRoutePermissionAsync(entity);
            var response = ApiResponse<RoutePermissions>.Create(ResponseStatus.RoleCreatedSuccessfully, "Route permission created successfully.", created);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, response);
        }

        // PUT update route permission
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Update(int id, [FromBody] RoutePermissionDTO dto)
        {
            if (id <= 0)
            {
                var error = ApiResponse<object>.Create(ResponseStatus.BadRequest, "Invalid ID!");
                return StatusCode(error.StatusCodes, error);
            }

            var validation = await _validator.ValidateAsync(dto);
            if (!validation.IsValid)
            {
                var errorDetails = validation.Errors.Select(e => new { Field = e.PropertyName, Message = e.ErrorMessage }).ToList();
                var error = ApiResponse<object>.FailureResponse("Validation failed.", 400, errorDetails);
                return StatusCode(error.StatusCodes, error);
            }

            var existing = await _repo.GetRoutePermissionByIdAsync(id);
            if (existing == null)
            {
                var error = ApiResponse<object>.Create(ResponseStatus.PrivilegeNotFound, $"Route permission with ID {id} not found.");
                return StatusCode(error.StatusCodes, error);
            }

            // If path/method changed, check duplicate
            if (!existing.HttpMethod.Equals(dto.HttpMethod, StringComparison.OrdinalIgnoreCase) ||
                existing.PathPattern != dto.PathPattern)
            {
                var exists = await _repo.RoutePermissionExistsAsync(dto.HttpMethod, dto.PathPattern);
                if (exists)
                {
                    var error = ApiResponse<object>.Create(ResponseStatus.BadRequest, $"Another route permission pattern '{dto.HttpMethod} {dto.PathPattern}' already exists!");
                    return StatusCode(error.StatusCodes, error);
                }
            }

            existing.HttpMethod = dto.HttpMethod.ToUpperInvariant();
            existing.PathPattern = dto.PathPattern;
            existing.RequiredMenuName = dto.RequiredMenuName;
            existing.RequiredPrivilegeName = dto.RequiredPrivilegeName;

            await _repo.UpdateRoutePermissionAsync(id, existing);
            var response = ApiResponse<object>.Create(ResponseStatus.UserUpdatedSuccessfully, "Route permission updated successfully.");
            return StatusCode(response.StatusCodes, response);
        }

        // DELETE route permission
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                var error = ApiResponse<object>.Create(ResponseStatus.BadRequest, "Invalid ID!");
                return StatusCode(error.StatusCodes, error);
            }

            var deleted = await _repo.DeleteRoutePermissionAsync(id);
            if (!deleted)
            {
                var error = ApiResponse<object>.Create(ResponseStatus.PrivilegeNotFound, $"Route permission with ID {id} not found.");
                return StatusCode(error.StatusCodes, error);
            }

            var response = ApiResponse<object>.Create(ResponseStatus.UserSoftDeleteSuccessfully, "Route permission deleted successfully.");
            return StatusCode(response.StatusCodes, response);
        }
    }
}
