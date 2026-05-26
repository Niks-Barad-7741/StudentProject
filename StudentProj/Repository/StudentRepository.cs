using Microsoft.EntityFrameworkCore;
using StudentProj.Data;
using StudentProj.DTO;
using StudentProj.Models;

namespace StudentProj.Repository
{
    public class StudentRepository : IStudent
    {
        private readonly StudentDbcontext _context;
        public StudentRepository(StudentDbcontext context) 
        {
            _context = context;
        }
        public async Task<int> Createstudentasync(Student student)
        {
            await _context.Student.AddAsync(student);
            await _context.SaveChangesAsync();
            return student.Id;
        }

        public async Task<bool> DeleteStudentasync(Student student)
        {
            //_context.Student.Remove(student);
            //await _context.SaveChangesAsync();
            //return true;
            student.IsDeleted = true;
            student.DeletedAt = DateTime.Now;
            _context.Student.Update(student);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<StudentDTO>> GetAllStudentsasync()
        {
            //return await _context.Student.ToListAsync();
            //   return await _context.Student
            //.Include(x => x.StudentRoles)
            //.ThenInclude(x => x.Role)
            //.ToListAsync();
            return await _context.Student
        .Where(x => !x.IsDeleted)
        .Select(x => new StudentDTO
        {
            Name = x.Name,
            Email = x.Email,
            Address = x.Address,
            Phone = x.Phone
        })
        .ToListAsync();
        }

        public async Task<StudentDTO> GetStudentbyemailasync(string email)
        {
            //return await _context.Student
            //    .Where(s => s.Email.ToLower().Equals(email.ToLower()))
            //    .FirstOrDefaultAsync();
            return await _context.Student
       .Where(x => x.Email.ToLower() == email.ToLower() && !x.IsDeleted)
       .Select(x => new StudentDTO
       {
           Name = x.Name,
           Email = x.Email,
           Address = x.Address,
           Phone = x.Phone
       })
       .FirstOrDefaultAsync();
        }

        public async Task<Student> GetStudentbyid(int id)
        {
            return await _context.Student.Where(student => student.Id == id && !student.IsDeleted).FirstOrDefaultAsync();
        //    return await _context.Student
        //.Where(x => x.Id == id)
        //.Select(x => new StudentDTO
        //{
        //    Name = x.Name,
        //    Email = x.Email,
        //    Address = x.Address,
        //    Phone = x.Phone
        //})
        //.FirstOrDefaultAsync();
        }

        public async Task<Student> Getstudentbynameasync(string name)
        {
            return await _context.Student.Where(student => student.Name.ToLower().Contains(name.ToLower()) && !student.IsDeleted).FirstOrDefaultAsync();
        //    return await _context.Student
        //.Where(x => x.Name.ToLower().Contains(name.ToLower()))
        //.Select(x => new StudentDTO
        //{
        //    Name = x.Name,
        //    Email = x.Email,
        //    Address = x.Address,
        //    Phone = x.Phone
        //})
        //.FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateStudentasync(int id, Student student)
        {
            _context.Student.Update(student);
            await _context.SaveChangesAsync();
            return student.Id == id;
        }
    }
}
