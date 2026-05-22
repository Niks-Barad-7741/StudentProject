using StudentProj.Models;
using System.Data;

namespace StudentProj.Repository
{
    public interface IRegisterRepository
    {
        Task<Student> GetStudentbyphoneasync(string phone);
        Task<bool> RegisterAsync(Student student);

        //Task<Roles> GetRoleByNameAsync(string roleName);
        Task AssignRoleAsync(int studentId, int roleId);
        Task<List<string>> GetStudentRolesAsync(int studentId);
        //Task UpdateStudentRoleAsync(int studentId, string roleName);
        Task<Student> GetStudentByIdAsync(int studentId);
        Task<Roles> GetRoleByIdAsync(int roleId);
        Task UpdateStudentRoleAsync(int studentId, int roleId);
    }
}