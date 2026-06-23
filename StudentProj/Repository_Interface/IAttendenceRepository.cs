using StudentProj.DTO;

namespace StudentProj.Repository_Interface
{
    public interface IAttendenceRepository
    {
        Task<AttendanceDTO> RecordAsync(RecordAttendanceDTO dto);
        Task<IEnumerable<AttendanceDTO>> GetBySubjectIdAsync(int subjectId, DateTime date);
        Task<ReportAttendenceDTO> GetRecordAsync(int studentId);
    }
}
