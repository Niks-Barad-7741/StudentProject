using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudentProj.Attributes;
using StudentProj.DTO;
using StudentProj.Enums;
using StudentProj.Models;
using StudentProj.Repository;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentProj.Controllers
{
    [Route("api/menus")]
    [ApiController]
    [HasPermission("Read", "Menus")]
    public class MenuController : ControllerBase
    {
        private readonly IMenuRepository _menuRepo;

        public MenuController(IMenuRepository menuRepo)
        {
            _menuRepo = menuRepo;
        }

        [HttpGet]
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
    }
}
