using Microsoft.EntityFrameworkCore;
using StudentProj.Data;
using StudentProj.Models;

namespace StudentProj.Repository
{
    public class AuthRepository : IAuthRepository
    {
        private readonly StudentDbcontext _dbcontext;
        public AuthRepository(StudentDbcontext dbcontext)
        {
            _dbcontext = dbcontext;
        }
        public async Task<Student> GetStudentbyemailasync(string email)
        {
            return await _dbcontext.Student
                .Where(s => s.Email.ToLower().Equals(email.ToLower()))
                .FirstOrDefaultAsync();
        }
        public async Task<bool> RegisterAsync(Student student)
        {
            await _dbcontext.Student.AddAsync(student);
            await _dbcontext.SaveChangesAsync();
            return true;
        }
    }
}
