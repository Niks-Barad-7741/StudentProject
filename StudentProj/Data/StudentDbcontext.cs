using Microsoft.EntityFrameworkCore;
using StudentProj.Models;

namespace StudentProj.Data
{
    public class StudentDbcontext : DbContext
    {
        public StudentDbcontext(DbContextOptions<StudentDbcontext> options) : base(options) 
        {
        }
        public DbSet<Student> Student { get; set; }
    }
}
