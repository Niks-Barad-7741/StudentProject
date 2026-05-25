using Microsoft.EntityFrameworkCore;
using StudentProj.Data;
using StudentProj.Models;
using System.Data;

namespace StudentProj.Repository
{
    public class RegisterRepository : IRegisterRepository
    {
        private readonly StudentDbcontext _dbcontext;
        public RegisterRepository(StudentDbcontext dbcontext)
        {
            _dbcontext = dbcontext;
        }
        public async Task<Student> GetStudentbyphoneasync(string phone)
        {
            //return await _dbcontext.Student
            //    .Where(s => s.Email.ToLower().Equals(email.ToLower()))
            //    .FirstOrDefaultAsync();
            return await _dbcontext.Student
                .Where(s => s.Phone.Equals(phone) && !s.IsDeleted)
                .FirstOrDefaultAsync();
        }
        public async Task<bool> RegisterAsync(Student student)
        {
            await _dbcontext.Student.AddAsync(student);
            await _dbcontext.SaveChangesAsync();
            return true;
        }


        public async Task<Roles> GetRoleByNameAsync(string roleName)
        {
            return await _dbcontext.Roles
                .Where(r => r.RoleName.ToLower()
                    .Equals(roleName.ToLower()) && !r.IsDeleted)
                .FirstOrDefaultAsync();
        }
        public async Task AssignRoleAsync(
           int studentId, int roleId)
        {
            var studentRole = new StudentRoles
            {
                StudentId = studentId,
                RoleId = roleId
            };
            await _dbcontext.StudentRoles.AddAsync(studentRole);
            await _dbcontext.SaveChangesAsync();
        }
        public async Task<List<string>> GetStudentRolesAsync(
           int studentId)
        {
            return await _dbcontext.StudentRoles
                .Where(sr => sr.StudentId == studentId && !sr.IsDeleted && !sr.Role.IsDeleted)
                .Select(sr => sr.Role.RoleName)
                .ToListAsync();

        }
        public async Task UpdateStudentRoleAsync(
           int studentId, string roleName)
        {
            // get new role
            var role = await GetRoleByNameAsync(roleName);
            if (role == null) return;

            // remove existing roles
            var existing = await _dbcontext.StudentRoles
                .Where(sr => sr.StudentId == studentId && !sr.IsDeleted)
                .ToListAsync();
            foreach (var sr in existing)
            {
                sr.IsDeleted = true;
                sr.DeletedAt = DateTime.UtcNow;
            }
            _dbcontext.StudentRoles.UpdateRange(existing);

            // assign new role
            await _dbcontext.StudentRoles.AddAsync(
                new StudentRoles
                {
                    StudentId = studentId,
                    RoleId = role.Id
                });
            await _dbcontext.SaveChangesAsync();
        }
        public async Task<Student> GetStudentByIdAsync(int studentId)
        {
            return await _dbcontext.Student
                .Where(s => s.Id == studentId && !s.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<Roles> GetRoleByIdAsync(int roleId)
        {
            return await _dbcontext.Roles
                .FirstOrDefaultAsync(r => r.Id == roleId && !r.IsDeleted);
        }

        public async Task UpdateStudentRoleAsync(int studentId, int roleId)
        {
            var role = await GetRoleByIdAsync(roleId);

            if (role == null)
                return;

            var studentRole = await _dbcontext.StudentRoles
                .FirstOrDefaultAsync(sr => sr.StudentId == studentId && !sr.IsDeleted);

            if (studentRole != null)
            {
                studentRole.RoleId = roleId;
            }
            else
            {
                _dbcontext.StudentRoles.Add(new StudentRoles
                {
                    StudentId = studentId,
                    RoleId = roleId
                });
            }

            await _dbcontext.SaveChangesAsync();
        }
    }
}
