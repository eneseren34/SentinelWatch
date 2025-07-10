using System.ComponentModel.DataAnnotations;

namespace SentinelWatch.Models
{
    public class Location
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;

        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        [Required]
        [StringLength(100)]
        public string Country { get; set; } = string.Empty;

        public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
        public virtual ICollection<NonProfitContact> NonProfitContacts { get; set; } = new List<NonProfitContact>();
    }
}