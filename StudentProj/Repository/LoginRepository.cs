using Microsoft.EntityFrameworkCore;
using StudentProj.Data;
using StudentProj.Models;
using StudentProj.Repository_Interface;

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

        public async Task<List<String>> GetStudentPermissionAsync(int studentId) 
        {
            return await _dbcontext.StudentRoles
                .Where(n => n.StudentId ==studentId && !n.IsDeleted && !n.Role.IsDeleted)
                .SelectMany(n => _dbcontext.RolePrivileges
                .Where(nb => nb.RoleId == n.RoleId
                && !nb.IsDeleted
                && !nb.Privilege.IsDeleted
                && nb.Menu != null && !nb.Menu.IsDeleted)
                .Select(n => $"{n.Privilege!.PrivilegeName}:{n.Menu!.MenuName}"))
                .Distinct()
                .ToListAsync();

        }
    }
}
