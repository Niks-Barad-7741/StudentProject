using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
namespace StudentProj.Models
{
    public class Course
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string CourseName { get; set; }

        [StringLength(100)]
        public string? Description { get; set; }

        public bool isDeleted { get; set; } = false;

        public ICollection<Subject> Subjects { get; set; } = new List<Subject>();
    }
}
