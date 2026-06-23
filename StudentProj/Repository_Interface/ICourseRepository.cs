using StudentProj.DTO;

namespace StudentProj.Repository_Interface
{
    public interface ICourseRepository
    {
        Task<IEnumerable<CourseDTO>> GetAllAsync();
        Task<CourseDTO> GetByIdAsync(int id);
        Task<CourseDTO> CreateAsync(CreateCourseDTO dto);
        Task<CourseDTO?> UpdateAsync(int id, UpdateCourseDTO dto);
        Task<bool> DeleteAsync(int id);

        Task<SubjectDTO> AddSubjectAsync(int courseId, CreateSubjectDTO dto);
        Task<IEnumerable<SubjectDTO>> GetSubjectsAsync(int courseId);
    }

}
