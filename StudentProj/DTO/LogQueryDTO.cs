using StackExchange.Redis;
using System;
namespace StudentProj.DTO
{
    public class LogQueryDTO
    {
        public string? Email { get; set; }
        public string? Action { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
    public class LogResponseDTO 
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Action { get; set; }
        public string Method { get; set; }
        public string Path { get; set; }
        public string IpAddress { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
