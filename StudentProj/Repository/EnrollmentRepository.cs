using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StudentProj.Data;
using StudentProj.DTO;
using StudentProj.Models;
using StudentProj.Repository_Interface;
using StudentProj.Common;

namespace StudentProj.Repository
{
    public class EnrollmentRepository : IEnrollmentRepository
    {
        private readonly StudentDbcontext _dbcontext;
        private readonly IMapper _mapper;

        public EnrollmentRepository(StudentDbcontext dbcontext, IMapper mapper) 
        {
            _dbcontext = dbcontext;
            _mapper = mapper;
        }
        public async Task<EnrollmentDTO> EnrollStudentAsync(EnrollStudentDTO dto)
        {
            var check = await _dbcontext.Enrollment
                .AnyAsync(n => n.StudentId == dto.StudentId
                && n.CourseId == dto.CourseId
                && !n.IsDeleted);
            if (check)
            {
                return null;
            }
            var enrollment = _mapper.Map<Enrollment>(dto);

            enrollment.EnrolledAt = DateTimeHelper.GetIndianStandardTime();
            enrollment.EnrolledAt = DateTime.UtcNow;
            _dbcontext.Enrollment.Add(enrollment);
            await _dbcontext.SaveChangesAsync();

            var created = await _dbcontext.Enrollment
                .Include(n => n.Student)
                .Include(n => n.Course)
                .FirstOrDefaultAsync(n => n.Id == enrollment.Id);

            return _mapper.Map<EnrollmentDTO>(created);
        }

        public async Task<IEnumerable<EnrollmentDTO>> GetStudentByIdAsync(int studentId)
        {
            var enrollment = await _dbcontext.Enrollment
                .Include(n => n.Student)
                .Include(n => n.Course)
                .Where(n => n.StudentId == studentId && !n.IsDeleted)
                .ToListAsync();
            return _mapper.Map<IEnumerable<EnrollmentDTO>>(enrollment);
        }

        public async Task<EnrollmentDTO> UpdateGradeAsync(int id, UpdateGradeDTO dto)
        {
            var enrollment = await _dbcontext.Enrollment
                .Include(n => n.Student)
                .Include(n => n.Course)
                .FirstOrDefaultAsync(n => n.Id == id && !n.IsDeleted);
            if (enrollment == null)
            {
                return null;
            }
            enrollment.Grade = dto.Grade;
            await _dbcontext.SaveChangesAsync();
            return _mapper.Map<EnrollmentDTO>(enrollment);
        }
    }
}
