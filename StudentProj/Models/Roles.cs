using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using Newtonsoft.Json;

namespace StudentProj.Models
{
    public class Roles
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(12)]
        public string RoleName { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedAt { get; set; }

        [JsonIgnore]
        public ICollection<StudentRoles> StudentRoles { get; set; }

        public ICollection<RolePrivileges> RolePrivileges { get; set; }
    }
}
