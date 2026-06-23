using System.ComponentModel.DataAnnotations;

namespace StudentProj.DTO
{
    public class CourseDTO
    {
        public int Id { get; set; }
        public string CourseName { get; set; }
        public string? Description { get; set; }

    }
    public class CreateCourseDTO 
    {
        [Required]
        [StringLength(50)]
        public string CourseName { get; set; }

        [Required]
        [StringLength(100)]
        public string? Description { get; set; }
    }

    public class UpdateCourseDTO
    {
        [Required]
        [StringLength(50)]
        public String CourseName { get; set; }

        [Required]
        [StringLength(100)]
        public string? Description { get; set; }

    }
}
