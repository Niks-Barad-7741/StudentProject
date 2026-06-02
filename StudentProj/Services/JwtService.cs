using Microsoft.IdentityModel.Tokens;
using StudentProj.Data;
using StudentProj.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Linq;

namespace StudentProj.Services
{
    public class JwtService
    {
        private readonly IConfiguration _config;
        private readonly StudentDbcontext _dbcontext;

        public JwtService(IConfiguration config, StudentDbcontext dbcontext) 
        {
            _config = config;
            _dbcontext = dbcontext;
        }
        public string GenerateToken(Student student, List<string> Roles) 
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["JWT-Token"])
                );

            var credintials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim("Id",student.Id.ToString()),
                new Claim("Name", student.Name),
                new Claim("Email", student.Email)

            };
            foreach (var role in Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Fetch active menu-privilege permissions for the student's active roles
            var permissions = _dbcontext.StudentRoles
                .Where(sr => sr.StudentId == student.Id && !sr.IsDeleted && !sr.Role.IsDeleted)
                .SelectMany(sr => _dbcontext.RolePrivileges
                    .Where(rp => rp.RoleId == sr.RoleId 
                              && !rp.IsDeleted 
                              && !rp.Privilege.IsDeleted 
                              && rp.Menu != null && !rp.Menu.IsDeleted)
                    .Select(rp => $"{rp.Privilege.PrivilegeName}:{rp.Menu.MenuName}"))
                .Distinct()
                .ToList();

            foreach (var permission in permissions)
            {
                claims.Add(new Claim("Permission", permission));
            }

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credintials
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
