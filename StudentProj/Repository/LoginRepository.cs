using Microsoft.EntityFrameworkCore;
using StudentProj.Data;
using StudentProj.Models;
using StudentProj.Repository_Interface;
using StudentProj.DTO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentProj.Repository
{
    public class LoginRepository : ILoginRepository
    {
        private readonly StudentDbcontext _dbcontext;
        public LoginRepository(StudentDbcontext dbcontext)
        {
            _dbcontext = dbcontext;
        }
        public async Task<Student> GetStudentbyemailasync(string email)
        {
            return await _dbcontext.Student
                .Where(s => s.Email.ToLower().Equals(email.ToLower()) && !s.IsDeleted)
                .FirstOrDefaultAsync();
        }
        public async Task<List<string>> GetStudentRolesAsync(
            int studentId)
        {
            return await _dbcontext.StudentRoles
                .Where(sr => sr.StudentId == studentId && !sr.IsDeleted && !sr.Role.IsDeleted && !sr.Student.IsDeleted)
                .Select(sr => sr.Role.RoleName)
                .ToListAsync();
        }

        public async Task<List<UserMenuPermissionDTO>> GetStudentPermissionAsync(int studentId) 
        {
            return await _dbcontext.StudentRoles
                .Where(n => n.StudentId == studentId && !n.IsDeleted && !n.Role.IsDeleted)
                .SelectMany(n => _dbcontext.RolePermissions
                    .Where(nb => nb.RoleId == n.RoleId
                        && !nb.IsDeleted
                        && !nb.Permission.IsDeleted
                        && nb.Menu != null && !nb.Menu.IsDeleted)
                    .Select(nb => new UserMenuPermissionDTO
                    {
                        MenuId = nb.Menu!.Id,
                        MenuName = nb.Menu.MenuName,
                        MenuRoute = nb.Menu.MenuRoute,
                        Permission = nb.Permission!.PermissionName
                    }))
                .Distinct()
                .ToListAsync();
        }
    }
}
