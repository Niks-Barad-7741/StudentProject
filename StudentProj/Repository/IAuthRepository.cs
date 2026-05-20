using StudentProj.Models;

namespace StudentProj.Repository
{
    public interface IAuthRepository
    {
        Task<Student> GetStudentbyemailasync(string email);
        Task<bool> RegisterAsync(Student student);
    }
}