using StudentProj.DTO;

namespace StudentProj.Repository_Interface
{
    public interface IEnrollmentRepository
    {
        Task<EnrollmentDTO> EnrollStudentAsync(EnrollStudentDTO dto);
        Task<IEnumerable<EnrollmentDTO>> GetStudentByIdAsync(int studentId);
        Task<EnrollmentDTO> UpdateGradeAsync(int id, UpdateGradeDTO dto);
    }
}
