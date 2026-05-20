using Microsoft.EntityFrameworkCore;
using StudentProj.Data;
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
            _context.Student.Remove(student);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Student>> GetAllStudentsasync()
        {
           return await _context.Student.ToListAsync();
        }

        public async Task<Student> GetStudentbyemailasync(string email)
        {
            return await _context.Student
                .Where(s => s.Email.ToLower().Equals(email.ToLower()))
                .FirstOrDefaultAsync();
        }

        public async Task<Student> GetStudentbyid(int id)
        {
            return await _context.Student.Where(student => student.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Student> Getstudentbynameasync(string name)
        {
            return await _context.Student.Where(student => student.Name.ToLower().Equals(name.ToLower())).FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateStudentasync(int id, Student student)
        {
            _context.Student.Update(student);
            await _context.SaveChangesAsync();
            return student.Id == id;
        }
    }
}
