using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentProj.Models
{
    public class Menu
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string MenuName { get; set; }

        [Required]
        [StringLength(100)]
        public string MenuRoute { get; set; } = string.Empty;

        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedAt { get; set; }

        public ICollection<RolePrivileges> RolePrivileges { get; set; }
    }
}
