using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentProj.Models
{
    public class RoutePermissions
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string HttpMethod { get; set; } = string.Empty; // GET, POST, PUT, DELETE

        [Required]
        [StringLength(250)]
        public string PathPattern { get; set; } = string.Empty; // e.g. "/api/students", "/api/students/{id}"

        [Required]
        [StringLength(50)]
        public string RequiredMenuName { get; set; } = string.Empty; // e.g. "create-student"

        [Required]
        [StringLength(50)]
        public string RequiredPermissionName { get; set; } = string.Empty; // e.g. "Write", "Read"
    }
}
