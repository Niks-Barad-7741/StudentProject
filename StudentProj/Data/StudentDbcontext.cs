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
        public DbSet<Roles> Roles { get; set; }
        public DbSet<StudentRoles> StudentRoles { get; set; }
        public DbSet<RolePrivileges> RolePrivileges { get; set; }
        public DbSet<Privileges> Privileges { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<Logs> Logs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) 
        {
            modelBuilder.Entity<StudentRoles>(entity =>
            {

                entity.HasOne(sr => sr.Student)
                    .WithMany(s => s.StudentRoles)
                    .HasForeignKey(sr => sr.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(sr => sr.Role)
                    .WithMany(r => r.StudentRoles)
                    .HasForeignKey(sr => sr.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<RolePrivileges>(entity =>
            {
                entity.HasOne(rp => rp.Menu)
                    .WithMany(m => m.RolePrivileges)
                    .HasForeignKey(rp => rp.MenuId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Roles>().HasData(
                new Roles { Id = 1, RoleName = "Super Admin",IsDeleted = false },
                new Roles { Id = 2, RoleName = "Admin" ,IsDeleted = false },
                new Roles { Id = 3, RoleName = "User", IsDeleted = false }
            );
        }
    }
}
