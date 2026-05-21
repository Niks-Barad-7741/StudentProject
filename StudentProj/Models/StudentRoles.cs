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

        [ForeignKey("StudentId")]
        [JsonIgnore]
        public Student Student { get; set; }

        [ForeignKey("RoleId")]
        public Roles Role { get; set; }

    }
}
