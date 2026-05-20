using System.ComponentModel.DataAnnotations;

namespace StudentProj.DTO
{
    public class StudentDTO
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public int Phone { get; set; }
    }
}
