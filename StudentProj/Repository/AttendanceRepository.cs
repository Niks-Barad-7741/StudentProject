using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StudentProj.Data;
using StudentProj.DTO;
using StudentProj.Enums;
using StudentProj.Models;
using StudentProj.Repository_Interface;
using StudentProj.Common;

namespace StudentProj.Repository
{
    public class AttendanceRepository : IAttendenceRepository
    {
        private readonly StudentDbcontext _dbcontext;
        private readonly IMapper _mapper;

        public AttendanceRepository(StudentDbcontext dbcontext, IMapper mapper) 
        {
            _dbcontext = dbcontext;
            _mapper = mapper;
        }

        public async Task<IEnumerable<AttendanceDTO>> GetBySubjectIdAsync(int subjectId, DateTime date)
        {
                var attendance = await _dbcontext.Attendance
                .Include(n => n.Student)
                .Include(n => n.Subject)
                .Where(n =>
                n.SubjectId == subjectId &&
                n.Date.Date == date.Date &&
                !n.IsDeleted &&
                !n.Subject.IsDeleted)
                .ToListAsync();
               

            return _mapper.Map<IEnumerable<AttendanceDTO>>(attendance);
        }

        public async Task<ReportAttendenceDTO> GetRecordAsync(int studentId)
        {
            var student = await _dbcontext.Student.FirstOrDefaultAsync(n => n.Id == studentId && !n.IsDeleted);
            if (student == null)
            {
                return null;
            }

            var records = await _dbcontext.Attendance
                .Where(n => n.StudentId == studentId && !n.IsDeleted)
                .ToListAsync();

            int total = records.Count;
            int present = records.Count(n => n.Status == "Present");

            return new ReportAttendenceDTO
            {
                StudentId = student.Id,
                StudentName = student.Name,
                TotalClasses = total,
                PresentClass = present,
                AttendancePercentage = total == 0 ? 0 : Math.Round((decimal)present /total * 100, 2) 
            };
            //throw new NotImplementedException();
        }

        public async Task<AttendanceDTO> RecordAsync(RecordAttendanceDTO dto)
        {
            var subject = await _dbcontext.Subject
                .FirstOrDefaultAsync(n => n.Id == dto.SubjectId && !n.IsDeleted);

            if (subject == null) 
            {
                return null;
                //var error = ApiResponse<object>.Create(ResponseStatus.NotFound, "Subject not found");
                //return StatusCode(error.StatusCodes);
            }

            var exists = await _dbcontext.Attendance
                .FirstOrDefaultAsync(n => n.StudentId == dto.StudentId
                && n.SubjectId == dto.SubjectId 
                && n.Date.Date == dto.Date.Date 
                && !n.IsDeleted);

            if (exists != null)
            {
                exists.Status = dto.Status;

                await _dbcontext.SaveChangesAsync();

                var updated = await _dbcontext.Attendance
                    .Include(n => n.Student)
                    .Include(n => n.Subject)
                    .FirstOrDefaultAsync(n => n.Id == exists.Id);
                return _mapper.Map<AttendanceDTO>(updated);
            }

            var attendance = _mapper.Map<Attendance>(dto);
            attendance.Date = DateTimeHelper.GetIndianStandardTime();

            _dbcontext.Attendance.Add(attendance);
            await _dbcontext.SaveChangesAsync();

            var created = await _dbcontext.Attendance
                .Include(n => n.Student)
                .Include(n => n.Subject)
                .FirstOrDefaultAsync(n => n.Id == attendance.Id);
            
            return _mapper.Map<AttendanceDTO>(created);
        }
    }
}
