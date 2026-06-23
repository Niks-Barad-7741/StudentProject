using StudentProj.DTO;
using System.Diagnostics.Eventing.Reader;

namespace StudentProj.Repository_Interface
{
    public interface ILogsRepository
    {
        Task<IEnumerable<LogResponseDTO>> GetLogsAsync(LogQueryDTO query);
    }
}
