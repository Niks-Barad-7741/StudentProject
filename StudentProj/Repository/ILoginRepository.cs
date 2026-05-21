using StudentProj.Models;

namespace StudentProj.Repository
{
    public interface ILoginRepository
    {
        Task<Student> GetStudentbyemailasync(string email);
        Task<List<string>> GetStudentRolesAsync(int studentId);

    }
}