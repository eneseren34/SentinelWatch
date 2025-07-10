using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SentinelWatch.Models
{
    public class Report
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public int LocationId { get; set; }
        public ReportType ReportType { get; set; }

        [Required]
        [StringLength(100)]
        public string Category { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Severity { get; set; }
        public decimal? Magnitude { get; set; }
        public decimal? Temperature { get; set; }
        public decimal? Humidity { get; set; }
        public decimal? WindSpeed { get; set; }
        public decimal? Precipitation { get; set; }

        [StringLength(255)]
        [Url]
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [ForeignKey("LocationId")]
        public virtual Location Location { get; set; } = null!;

        public virtual ICollection<ReportStatus> StatusHistory { get; set; } = new List<ReportStatus>();
    }
}