using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SentinelWatch.Models
{
    public class NonProfitContact
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [EmailAddress]
        public string ContactEmail { get; set; } = string.Empty;

        [StringLength(20)]
        [Phone]
        public string? Phone { get; set; }
        public int LocationId { get; set; }

        [StringLength(255)]
        [Url]
        public string? Website { get; set; }

        [ForeignKey("LocationId")]
        public virtual Location Location { get; set; } = null!;
    }
}