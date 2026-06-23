using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StudentProj.Data;
using StudentProj.DTO;
using StudentProj.Mapping;
using StudentProj.Models;
using StudentProj.Repository_Interface;
using System.Runtime.CompilerServices;

namespace StudentProj.Repository
{
    public class CourseRepository : ICourseRepository
    {
        private readonly StudentDbcontext _dbcontext;
        private readonly IMapper _mapper;

        public CourseRepository(StudentDbcontext dbcontext,IMapper mapper) 
        {
            _dbcontext = dbcontext;
            _mapper = mapper;
        }

        public async Task<SubjectDTO> AddSubjectAsync(int courseId, CreateSubjectDTO dto)
        {
            var course = await _dbcontext.Course.FirstOrDefaultAsync(n => n.Id == courseId && !n.isDeleted);
            if (course == null) return null;

            var subject = _mapper.Map<Subject>(dto);
            subject.CourseId = courseId;
            _dbcontext.Subject.Add(subject);
            await _dbcontext.SaveChangesAsync();
            return _mapper.Map<SubjectDTO>(subject);
        }

        public async Task<CourseDTO> CreateAsync(CreateCourseDTO dto)
        {
            var course = _mapper.Map<Course>(dto);
            _dbcontext.Course.Add(course);
            await _dbcontext.SaveChangesAsync();
            return _mapper.Map<CourseDTO>(course);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var course = await _dbcontext.Course.FirstOrDefaultAsync(n => n.Id == id && !n.isDeleted);
            if (course == null) return false;

            course.isDeleted = true;
            await _dbcontext.SaveChangesAsync();
            return true;

        }

        public async Task<IEnumerable<CourseDTO>> GetAllAsync()
        {
            var course = await _dbcontext.Course
                .Where(n => !n.isDeleted)
                .ToListAsync();
            return _mapper.Map<IEnumerable<CourseDTO>>(course);
        }

        public async Task<CourseDTO> GetByIdAsync(int id)
        {
            var course = await _dbcontext.Course
                .FirstOrDefaultAsync(n => n.Id == id && !n.isDeleted);
            if (course == null) return null;
            return _mapper.Map<CourseDTO>(course);
        }

        public async Task<IEnumerable<SubjectDTO>> GetSubjectsAsync(int courseId)
        {
            var subject = await _dbcontext.Subject
                .Where(n => n.Id == courseId && !n.IsDeleted)
                .ToListAsync();
            return _mapper.Map<IEnumerable<SubjectDTO>>(subject);
        }

        public async Task<CourseDTO?> UpdateAsync(int id, UpdateCourseDTO dto)
        {
            var course = await _dbcontext.Course.FirstOrDefaultAsync(n => n.Id == id && !n.isDeleted);
            if (course == null) return null;
            _mapper.Map(dto, course);
            await _dbcontext.SaveChangesAsync();
            return _mapper.Map<CourseDTO>(course);
        }
    }
}
