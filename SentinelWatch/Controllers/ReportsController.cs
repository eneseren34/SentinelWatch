using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Needed for DbContext and Include/Select/ToListAsync
using SentinelWatch.Data;        // Needed for ApplicationDbContext
using System.Linq;                 // Needed for Select
using System.Threading.Tasks;      // Needed for async operations

[Route("api/[controller]")] // Sets the base URL to /api/Reports
[ApiController]
public class ReportsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    // Constructor: This 'injects' our ApplicationDbContext so we can use it here.
    public ReportsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /api/Reports
    [HttpGet]
    public async Task<IActionResult> GetReports()
    {
        // Query the database for active reports
        var reportData = await _context.Reports
            .Include(r => r.Location) // IMPORTANT: We need Location to get Lat/Lng
            .Where(r => r.IsActive)   // We probably only want active reports on the map
            .Select(r => new // Select only the data our map needs
            {
                id = r.Id,
                category = r.Category,
                severity = r.Severity,
                timestamp = r.Timestamp,
                latitude = r.Location.Latitude,   // Get Latitude from joined Location
                longitude = r.Location.Longitude  // Get Longitude from joined Location
            })
            .ToListAsync(); // Execute the query asynchronously

        // Return the data as JSON with a 200 OK status code
        return Ok(reportData);
    }

    // POST: /api/Reports
    [HttpPost]
    public async Task<IActionResult> CreateReportFromMap([FromBody] CreateReportDto reportDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // 1. Create a new Location for this map click.
            //    (A more advanced version might find nearby locations,
            //     but for now, we'll create a new one each time).
            var newLocation = new SentinelWatch.Models.Location
            {
                Name = $"Map Report @ {DateTime.UtcNow:G}", // Simple name
                Latitude = reportDto.Latitude,
                Longitude = reportDto.Longitude,
                Country = "Turkey" // Defaulting to Turkey based on your context
            };
            _context.Locations.Add(newLocation);
            // We need to save here to get an Id for the newLocation
            await _context.SaveChangesAsync();

            // 2. Create the new Report
            var newReport = new SentinelWatch.Models.Report
            {
                Category = reportDto.Category,
                Severity = reportDto.Severity,
                ReportType = SentinelWatch.Models.ReportType.Emergency, // Defaulting type
                LocationId = newLocation.Id, // Link to the new location
                IsActive = true, // New reports are active
                Timestamp = DateTime.UtcNow,
                UserId = null // No user auth yet
            };
            _context.Reports.Add(newReport);
            await _context.SaveChangesAsync();

            // 3. Return the newly created report (or just a success message)
            //    We should return something similar to what the GET returns,
            //    so our map can potentially add it instantly.
            var result = new
            {
                id = newReport.Id,
                category = newReport.Category,
                severity = newReport.Severity,
                timestamp = newReport.Timestamp,
                latitude = newLocation.Latitude,
                longitude = newLocation.Longitude
            };

            // Return 201 Created status with the new report data
            return CreatedAtAction(nameof(GetReports), new { id = newReport.Id }, result);
        }
        catch (Exception ex)
        {
            // Log the exception (important for real apps!)
            return StatusCode(500, "Internal server error: " + ex.Message);
        }
    }

    // We can add more methods later (like POST to add reports)
}

public class CreateReportDto
{
    public required string Category { get; set; }
    public string? Severity { get; set; }
    public required decimal Latitude { get; set; }
    public required decimal Longitude { get; set; }
}