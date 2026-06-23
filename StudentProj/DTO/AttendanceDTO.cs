using StackExchange.Redis;
using System.ComponentModel.DataAnnotations;

namespace StudentProj.DTO
{
    public class AttendanceDTO
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public int SubjectId { get; set; }
        public string SubjectName { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
    }

    public class  RecordAttendanceDTO
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        public int SubjectId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [RegularExpression("Present|Absent|Late")]
        public string Status { get; set; }
    }

    public class ReportAttendenceDTO
    {
        public int StudentId { get; set; }
        public string StudentName{ get; set; }
        public int TotalClasses { get; set; }
        public int PresentClass { get; set; }
        public decimal AttendancePercentage { get; set; }

    }
}
