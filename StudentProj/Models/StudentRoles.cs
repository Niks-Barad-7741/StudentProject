using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
namespace StudentProj.Models
{
    public class StudentRoles
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int RoleId { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedAt { get; set; }

        [ForeignKey("StudentId")]
        [JsonIgnore]
        public Student Student { get; set; }

        [ForeignKey("RoleId")]
        public Roles Role { get; set; }

    }
}
