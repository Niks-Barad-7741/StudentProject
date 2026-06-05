using StudentProj.Models;
using StudentProj.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentProj.Repository_Interface
{
    public interface ILoginRepository
    {
        Task<Student> GetStudentbyemailasync(string email);
        Task<List<string>> GetStudentRolesAsync(int studentId);
        Task<List<UserMenuPermissionDTO>> GetStudentPermissionAsync(int studentId);
    }
}