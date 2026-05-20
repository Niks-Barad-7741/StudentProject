using System.ComponentModel.DataAnnotations;

namespace StudentProj.DTO
{
    public class LoginDTO
    {

        [Required]
        [EmailAddress]
        [StringLength(50)]

        public string Email { get; set; }

        [Required]
        public string Password { get; set; }    
    }
}
