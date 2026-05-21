using Microsoft.EntityFrameworkCore;
using StudentProj.Data;
using StudentProj.Models;

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
                .Where(s => s.Email.ToLower().Equals(email.ToLower()))
                .FirstOrDefaultAsync();
        }
        public async Task<List<string>> GetStudentRolesAsync(
            int studentId)
        {
            return await _dbcontext.StudentRoles
                .Where(sr => sr.StudentId == studentId)
                .Select(sr => sr.Role.RoleName)
                .ToListAsync();
        }
    }
}
