using StudentProj.DTO;
using StudentProj.Models;

namespace StudentProj.Repository
{
    public interface IStudent
    {
        Task<List<StudentDTO>> GetAllStudentsasync();
        Task<Student> GetStudentbyid(int id);
        Task<int> Createstudentasync(Student student);
        Task<bool> UpdateStudentasync(int id,Student student);
        Task<Student> Getstudentbynameasync(string name);
        Task<StudentDTO> GetStudentbyemailasync(string email);
        Task<bool> DeleteStudentasync(Student student);
        Task<int> UpsertStudentAsync(Student student);

    }
}
