using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StudentProj.Data;
using StudentProj.DTO;
using StudentProj.Models;
using StudentProj.Repository_Interface;

namespace StudentProj.Repository
{
    public class SubjectRepository : ISubjectRepository
    {
        private readonly StudentDbcontext _dbcontext;
        private readonly IMapper _mapper;

        public SubjectRepository(StudentDbcontext dbcontext, IMapper mapper) 
        {
            _dbcontext = dbcontext;
            _mapper = mapper;
        }
        public async Task<SubjectDTO> CreateAsync(CreateSubjectDTO dto)
        {
            var course = await _dbcontext.Course
                .FirstOrDefaultAsync(n => n.Id == dto.CourseId && !n.isDeleted);

            if (course == null)
            {
                return null;
            }

            var existe = await _dbcontext.Subject
                .FirstOrDefaultAsync(n => n.SubjectCode == dto.SubjectCode && n.CourseId == dto.CourseId && !n.IsDeleted);

            if (existe != null) 
            {
                return null;
            }

            var addsubj = _mapper.Map<Subject>(dto);
            await _dbcontext.Subject.AddAsync(addsubj);
            await _dbcontext.SaveChangesAsync();

            return _mapper.Map<SubjectDTO>(addsubj);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var subject = await _dbcontext.Subject
                .Where(n => n.Id == id && !n.IsDeleted)
                .FirstOrDefaultAsync();
            if (subject == null)
            {
                return false;
            }
            subject.IsDeleted = true;
            await _dbcontext.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<SubjectDTO>> GetAllAsync()
        {
            var subjects = await _dbcontext.Subject
                .Where(n => !n.IsDeleted)
                .ToListAsync();
            return _mapper.Map<IEnumerable<SubjectDTO>>(subjects);
        }

        public async Task<SubjectDTO> GetByIdAsync(int id)
        {
            var subject = await _dbcontext.Subject
                .Where(n => n.Id == id && !n.IsDeleted)
                .FirstOrDefaultAsync();
            return _mapper.Map<SubjectDTO>(subject);
        }

        public async Task<SubjectDTO> UpdateAsync(int id, UpdateSubjectDTO dto)
        {
            var course = await _dbcontext.Course
                .AnyAsync(n => n.Id == dto.CourseId && !n.isDeleted);
            if (!course) 
            {
                return null;
            }

                var subject = await _dbcontext.Subject
                .Where(n => n.Id == id && !n.IsDeleted)
                .FirstOrDefaultAsync();
            if (subject == null)
            {
                return null;
            }
            var update = _mapper.Map(dto, subject);
            await _dbcontext.SaveChangesAsync();
            return _mapper.Map<SubjectDTO>(update);
        }
    }
}
