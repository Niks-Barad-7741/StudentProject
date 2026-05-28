using System.ComponentModel.DataAnnotations.Schema;

namespace StudentProj.Models
{
    public class Logs
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string? Email {get; set;}

        public string Action { get; set; }

        public string Method { get; set; }

        public string Path { get; set; }

        public string IpAddress { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
