using System.ComponentModel.DataAnnotations;

namespace StudentProj.DTO
{
    public class SubjectDTO
    {
        public string Id { get; set; }
        public string SubjectName { get; set; }
        public int SubjectCode { get; set; }
        public int CourseId { get; set; }
    }

    public class  CreateSubjectDTO
    {
        [Required]
        [StringLength(50)]
        public string SubjectName { get; set; }

        [Required]
        public int SubjectCode { get; set; }

        [Required]
        public int CourseId { get; set; }
    }
    public class UpdateSubjectDTO
    {
        [Required]
        [StringLength(50)]
        public string SubjectName { get; set; }

        [Required]
        public int SubjectCode { get; set; }

        [Required]
        public int CourseId { get; set; }
    }
}
