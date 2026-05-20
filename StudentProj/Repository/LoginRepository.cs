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
    }
}
