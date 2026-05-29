using StudentProj.Models;

namespace StudentProj.Repository
{
    public interface IMenuRepository
    {
        Task<List<Menu>> GetAllMenusAsync();
        Task<Menu?> GetMenuByIdAsync(int id);
        Task<Menu?> GetMenuByNameAsync(string name);
        Task<bool> MenuExistsAsync(string name);
        Task<Menu> CreateMenuAsync(Menu menu);
        Task<bool> UpdateMenuAsync(int id, Menu menu);
        Task<bool> DeleteMenuAsync(int id);
    }
}
