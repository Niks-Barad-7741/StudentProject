using Microsoft.EntityFrameworkCore;
using StudentProj.Data;
using StudentProj.Models;
using StudentProj.Repository_Interface;

namespace StudentProj.Repository
{
    public class MenuRepository : IMenuRepository
    {
        private readonly StudentDbcontext _dbcontext;

        public MenuRepository(StudentDbcontext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task<Menu> CreateMenuAsync(Menu menu)
        {
            var existing = await _dbcontext.Menus
                .FirstOrDefaultAsync(m => m.MenuName.ToLower() == menu.MenuName.ToLower());

            if (existing != null)
            {
                if (existing.IsDeleted)
                {
                    existing.IsDeleted = false;
                    existing.DeletedAt = null;
                    _dbcontext.Menus.Update(existing);
                    await _dbcontext.SaveChangesAsync();
                }
                return existing;
            }

            await _dbcontext.Menus.AddAsync(menu);
            await _dbcontext.SaveChangesAsync();
            return menu;
        }

        public async Task<bool> DeleteMenuAsync(int id)
        {
            var menu = await GetMenuByIdAsync(id);
            if (menu == null) return false;
            
            menu.IsDeleted = true;
            menu.DeletedAt = DateTime.Now;
            _dbcontext.Menus.Update(menu);
            await _dbcontext.SaveChangesAsync();
            return true;
        }

        public async Task<List<Menu>> GetAllMenusAsync()
        {
            return await _dbcontext.Menus
                .Where(m => !m.IsDeleted)
                .ToListAsync();
        }

        public async Task<Menu?> GetMenuByIdAsync(int id)
        {
            return await _dbcontext.Menus
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);
        }

        public async Task<Menu?> GetMenuByNameAsync(string name)
        {
            return await _dbcontext.Menus
                .FirstOrDefaultAsync(m => m.MenuName.ToLower() == name.ToLower() && !m.IsDeleted);
        }

        public async Task<bool> MenuExistsAsync(string name)
        {
            return await _dbcontext.Menus
                .AnyAsync(m => m.MenuName.ToLower() == name.ToLower() && !m.IsDeleted);
        }

        public async Task<bool> UpdateMenuAsync(int id, Menu menu)
        {
            _dbcontext.Menus.Update(menu);
            await _dbcontext.SaveChangesAsync();
            return menu.Id == id;
        }

        public async Task<List<Menu>> GetMenusFromUserAsync(int userId, List<string> Roles) 
        {
            if (Roles.Contains("Super Admin", StringComparer.OrdinalIgnoreCase)) 
            {
                return await GetAllMenusAsync();
            }
            return await _dbcontext.StudentRoles
                .Where(n => n.StudentId == userId && !n.IsDeleted && !n.Role.IsDeleted)
                .SelectMany(n => _dbcontext.RolePrivileges
                    .Where(nb => nb.RoleId == n.RoleId
                            && !nb.IsDeleted
                            && !nb.Privilege.IsDeleted
                            && nb.Menu != null && !nb.Menu.IsDeleted)
                    .Select(n => n.Menu))
                .Distinct()
                .ToListAsync();
        }
    }
}
