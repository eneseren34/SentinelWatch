using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration; // Required for IConfiguration
using System;
using System.Net.Http;
using System.Text.Json; // For System.Text.Json
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class WeatherController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly string _openWeatherApiKey;

    public WeatherController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        // Retrieve the API key from appsettings.json
        _openWeatherApiKey = _configuration["OpenWeatherMap:ApiKey"] ?? throw new ArgumentNullException("OpenWeatherMap API Key not configured");
    }

    // GET: /api/Weather?lat=34.05&lon=-118.25
    [HttpGet]
    public async Task<IActionResult> GetWeather([FromQuery] double lat, [FromQuery] double lon)
    {
        if (string.IsNullOrEmpty(_openWeatherApiKey))
        {
            return StatusCode(500, "OpenWeatherMap API Key is not configured on the server.");
        }

        var client = _httpClientFactory.CreateClient();
        // Using HTTPS for OpenWeatherMap API
        var requestUrl = $"https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&appid={_openWeatherApiKey}&units=metric"; // units=metric for Celsius

        try
        {
            var response = await client.GetAsync(requestUrl);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                // We'll return the raw JSON for now.
                // In a more refined app, you'd map this to a C# DTO (Data Transfer Object)
                // and return only the fields you need.
                using var jsonDoc = JsonDocument.Parse(jsonResponse);
                return Ok(jsonDoc.RootElement.Clone()); // Return the parsed JSON object
            }
            else
            {
                // Log the error response content for debugging
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"OpenWeatherMap API Error: {response.StatusCode} - {errorContent}");
                return StatusCode((int)response.StatusCode, $"Error fetching weather data: {response.ReasonPhrase}");
            }
        }
        catch (Exception ex)
        {
            // Log the exception
            Console.WriteLine($"Exception fetching weather: {ex.Message}");
            return StatusCode(500, "Internal server error while fetching weather data.");
        }
    }
}