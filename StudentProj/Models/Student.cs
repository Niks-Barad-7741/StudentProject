using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentProj.Models
{
    public class Student
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

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
        public string PasswordHash { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedAt { get; set; }
        public string? RefereshToken { get; set; }

        public DateTime? RefereshTokenExpiryTime { get; set; }

        // Audit Columns (Option A - Appended at the end of the table)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public string? UpdatedBy { get; set; }

        public string? DeletedBy { get; set; }

        public string? IpAddress { get; set; }



        // OTP Tracking
        public string? ResetOtp { get; set; }

        public DateTime? ResetOtpExpiry { get; set; }

        public ICollection<StudentRoles> StudentRoles { get; set; }

    }
}
