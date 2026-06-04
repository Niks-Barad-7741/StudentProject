using StudentProj.Models;

namespace StudentProj.Repository_Interface
{
    public interface ILoginRepository
    {
        Task<Student> GetStudentbyemailasync(string email);
        Task<List<string>> GetStudentRolesAsync(int studentId);
        Task<List<string>> GetStudentPermissionAsync(int studentId);

    }
}