using Microsoft.EntityFrameworkCore;
using SentinelWatch.Models; // We need to access our models

namespace SentinelWatch.Data
{
    public class ApplicationDbContext : DbContext
    {
        // This constructor is essential for setting up the context
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // These DbSet properties represent your tables in the database
        public DbSet<User> Users { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<ReportStatus> ReportStatuses { get; set; }
        public DbSet<NonProfitContact> NonProfitContacts { get; set; }

        // This method allows us to configure the database model further
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Decimal Precision for Location
            modelBuilder.Entity<Location>(entity =>
            {
                entity.Property(e => e.Latitude).HasColumnType("decimal(10, 6)");
                entity.Property(e => e.Longitude).HasColumnType("decimal(10, 6)");
            });

            // Configure Decimal Precision for Report
            modelBuilder.Entity<Report>(entity =>
            {
                entity.Property(e => e.Magnitude).HasColumnType("decimal(5, 2)");
                entity.Property(e => e.Temperature).HasColumnType("decimal(5, 2)");
                entity.Property(e => e.Humidity).HasColumnType("decimal(5, 2)");
                entity.Property(e => e.WindSpeed).HasColumnType("decimal(5, 2)");
                entity.Property(e => e.Precipitation).HasColumnType("decimal(5, 2)");

                // Configure Enum to String conversion (makes DB more readable)
                entity.Property(e => e.ReportType)
                      .HasConversion<string>();

                // Configure relationship: Report to User (Nullable FK)
                // If a User is deleted, set the UserId in their reports to NULL.
                entity.HasOne(d => d.User)
                      .WithMany(p => p.Reports)
                      .HasForeignKey(d => d.UserId)
                      .OnDelete(DeleteBehavior.SetNull);

                // Configure relationship: Report to Location (Required FK)
                // If a Location is deleted, delete all associated Reports.
                entity.HasOne(d => d.Location)
                      .WithMany(p => p.Reports)
                      .HasForeignKey(d => d.LocationId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Enum to String conversion for ReportStatus
            modelBuilder.Entity<ReportStatus>(entity =>
            {
                entity.Property(e => e.Status)
                      .HasConversion<string>();

                // Configure relationship: ReportStatus to User (Nullable FK)
                // If a User (reviewer) is deleted, set ReviewedById to NULL.
                entity.HasOne(d => d.ReviewedBy)
                      .WithMany(p => p.ReviewedStatuses)
                      .HasForeignKey(d => d.ReviewedById)
                      .OnDelete(DeleteBehavior.SetNull);

                // Configure relationship: ReportStatus to Report (Required FK)
                // If a Report is deleted, delete all its status history.
                entity.HasOne(d => d.Report)
                      .WithMany(p => p.StatusHistory)
                      .HasForeignKey(d => d.ReportId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure relationship: NonProfitContact to Location
            modelBuilder.Entity<NonProfitContact>(entity =>
            {
                entity.HasOne(d => d.Location)
                      .WithMany(p => p.NonProfitContacts)
                      .HasForeignKey(d => d.LocationId)
                      .OnDelete(DeleteBehavior.Cascade); // If Location deleted, delete contacts.
            });

            // We are relying on [Index] attributes in User.cs for uniqueness,
            // but you could also configure them here using:
            // modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
            // modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        }
    }
}