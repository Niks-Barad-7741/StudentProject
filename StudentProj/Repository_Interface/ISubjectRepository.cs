using StudentProj.DTO;

namespace StudentProj.Repository_Interface
{
    public interface ISubjectRepository
    {
        Task<IEnumerable<SubjectDTO>> GetAllAsync();
        Task<SubjectDTO> GetByIdAsync(int id);
        Task<SubjectDTO> CreateAsync(CreateSubjectDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}
