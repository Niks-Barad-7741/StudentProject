using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace StudentProj.DTO
{
    public class RegisterDTO
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]

        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(50)]

        public string Email { get; set; }

        [Required]
        [StringLength(200)]
        public string Address { get; set; }

        [Required]
        [StringLength(10, MinimumLength = 10)]
        [Phone]
        public string Phone { get; set; }

        [Required]
        [DefaultValue("")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", ErrorMessage = "Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        public string Password { get; set; }
    }
}
