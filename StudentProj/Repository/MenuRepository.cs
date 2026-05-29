using Microsoft.EntityFrameworkCore;
using StudentProj.Data;
using StudentProj.Models;

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
    }
}
