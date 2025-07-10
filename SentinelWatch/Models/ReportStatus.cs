using System.ComponentModel.DataAnnotations.Schema;

namespace SentinelWatch.Models
{
    public class ReportStatus
    {
        public int Id { get; set; }
        public int ReportId { get; set; }
        public ReportStatusValue Status { get; set; } = ReportStatusValue.Pending;
        public int? ReviewedById { get; set; }
        public DateTime? ReviewTimestamp { get; set; }

        [ForeignKey("ReportId")]
        public virtual Report Report { get; set; } = null!;

        [ForeignKey("ReviewedById")]
        public virtual User? ReviewedBy { get; set; }
    }
}