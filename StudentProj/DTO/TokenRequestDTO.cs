using System.ComponentModel.DataAnnotations;

namespace StudentProj.DTO
{
    public class TokenRequestDTO
    {
        [Required]
        public string AccessToken { get; set; }

        [Required]
        public string RefereshToken { get; set; }
    }
}
