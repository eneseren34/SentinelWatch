using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace SentinelWatch.Models
{
    [Index(nameof(Username), IsUnique = true)]
    [Index(nameof(Email), IsUnique = true)]
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Password { get; set; } = string.Empty; // We'll hash this later

        public DateTime JoinDate { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
        public virtual ICollection<ReportStatus> ReviewedStatuses { get; set; } = new List<ReportStatus>();
    }
}