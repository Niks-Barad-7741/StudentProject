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
                .Where(s => s.Phone.Equals(phone))
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
                    .Equals(roleName.ToLower()))
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
                .Where(sr => sr.StudentId == studentId)
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
                .Where(sr => sr.StudentId == studentId)
                .ToListAsync();
            _dbcontext.StudentRoles.RemoveRange(existing);

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
                .FirstOrDefaultAsync(x => x.Id == studentId);
        }

        public async Task<Roles> GetRoleByIdAsync(int roleId)
        {
            return await _dbcontext.Roles
                .FirstOrDefaultAsync(r => r.Id == roleId);
        }

        public async Task UpdateStudentRoleAsync(int studentId, int roleId)
        {
            var role = await GetRoleByIdAsync(roleId);

            if (role == null)
                return;

            var studentRole = await _dbcontext.StudentRoles
                .FirstOrDefaultAsync(sr => sr.StudentId == studentId);

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
