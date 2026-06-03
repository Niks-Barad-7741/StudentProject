using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudentProj.Attributes;
using StudentProj.DTO;
using StudentProj.Enums;
using StudentProj.Models;
using StudentProj.Repository_Interface;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace StudentProj.Controllers
{
    [Route("api/menus")]
    [ApiController]
    
    public class MenuController : ControllerBase
    {
        private readonly IMenuRepository _menuRepo;

        public MenuController(IMenuRepository menuRepo)
        {
            _menuRepo = menuRepo;
        }

        [HttpGet]
        [HasPermission("Read", "Menus")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllMenus()
        {
            var menus = await _menuRepo.GetAllMenusAsync();
            var response = menus.Select(m => new
            {
                Id = m.Id,
                MenuName = m.MenuName
            });

            var success = ApiResponse<object>.Create(ResponseStatus.UserRetriveSuccessfully, "Menus retrieved successfully.", response);
            return StatusCode(success.StatusCodes, success);
        }

        [HttpPost]
        [HasPermission("Create", "Menus")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> CreateMenu([FromBody] MenuDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.MenuName))
            {
                var error = ApiResponse<object>.Create(ResponseStatus.BadRequest, "Menu name is required.");
                return StatusCode(error.StatusCodes, error);
            }

            var exists = await _menuRepo.MenuExistsAsync(dto.MenuName);
            if (exists)
            {
                var error = ApiResponse<object>.Create(ResponseStatus.BadRequest, $"Menu '{dto.MenuName}' already exists!");
                return StatusCode(error.StatusCodes, error);
            }

            var menu = new Menu { MenuName = dto.MenuName };
            var created = await _menuRepo.CreateMenuAsync(menu);

            var success = ApiResponse<Menu>.Create(ResponseStatus.RoleCreatedSuccessfully, "Menu created successfully.", created);
            return Created("", success);
        }


        [HttpPut("{id}")]
        [HasPermission("Update", "Menus")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateMenu(int id, [FromBody] MenuDTO dto) 
        {
            if (id <= 0) 
            {
                var error = ApiResponse<object>.Create(ResponseStatus.BadRequest, "Invalid Menu Id!");
                return StatusCode(error.StatusCodes, error);
            }
            if (string.IsNullOrWhiteSpace(dto.MenuName))
            {
                var error = ApiResponse<object>.Create(ResponseStatus.BadRequest, "Menu Name is Required");
                return StatusCode(error.StatusCodes, error);
            }
            var existingmenu = await _menuRepo.GetMenuByIdAsync(id);
            if (existingmenu == null )
            {
                var error = ApiResponse<object>.Create(ResponseStatus.PrivilegeNotFound, $"Menu With ID {id} not found.");
                return StatusCode(error.StatusCodes, error);
            }
            var nameExists = await _menuRepo.MenuExistsAsync(dto.MenuName);
            if (nameExists && !existingmenu.MenuName.Equals(dto.MenuName,StringComparison.OrdinalIgnoreCase))
            {
                var error = ApiResponse<object>.Create(ResponseStatus.BadRequest, $"Menu `{dto.MenuName}` already exists");
                return StatusCode(error.StatusCodes, error);
            }
            existingmenu.MenuName = dto.MenuName;
            await _menuRepo.UpdateMenuAsync(id, existingmenu);

            var success = ApiResponse<object>.Create(ResponseStatus.UserUpdatedSuccessfully, "Menu Updated Succesfully");
            return StatusCode(success.StatusCodes, success);
        }

        [HttpDelete("{id}")]
        [HasPermission("Delete", "Menus")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteMenu(int id)
        {
            if (id <= 0)
            {
                var error = ApiResponse<object>.Create(ResponseStatus.BadRequest, "Invalid menu id!");
                return StatusCode(error.StatusCodes, error);
            }

            var result = await _menuRepo.DeleteMenuAsync(id);
            if (!result)
            {
                var error = ApiResponse<object>.Create(ResponseStatus.PrivilegeNotFound, $"Menu with ID {id} not found.");
                return StatusCode(error.StatusCodes, error);
            }

            var success = ApiResponse<object>.Create(ResponseStatus.UserSoftDeleteSuccessfully, "Menu soft-deleted successfully.");
            return StatusCode(success.StatusCodes, success);
        }

        [Authorize]
        [HttpGet("My-Menus")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetMyMenus()
        {
            var userIdClaim = HttpContext.User.FindFirst("Id");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId)) 
            {
                var failResponese = ApiResponse<object>.Create(ResponseStatus.Unauthorized);
                return StatusCode(failResponese.StatusCodes, failResponese);
            }
            var roles = HttpContext.User.FindAll(System.Security.Claims.ClaimTypes.Role)
                .Select(n => n.Value)
                .ToList();

            var menus = await _menuRepo.GetMenusFromUserAsync(userId,roles);

            var response = menus.Select
                (n => new
                {
                    Id = n.Id,
                    MenuName = n.MenuName,
                });

            var success = ApiResponse<object>.Create(ResponseStatus.UserRetriveSuccessfully, "Your Menus Retrive Succesfully.", response);
            return StatusCode(success.StatusCodes, success);
        }



    }
}
