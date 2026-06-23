using System.ComponentModel.DataAnnotations;

namespace StudentProj.DTO
{
    public class EnrollmentDTO
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public DateTime EnrolledAt { get; set; }
        public decimal? Grade { get; set; }
    }

    public class EnrollStudentDTO 
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        public int CourseId { get; set; }
    }

    public class UpdateGradeDTO 
    {
        [Required]
        [Range(0, 100)]
        public decimal Grade { get; set; }
    }
}
