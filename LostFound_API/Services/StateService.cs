using LostFound_API.DTOs.States;
using System.Text.Json;

namespace LostFound_API.Services
{
    public class StateService
    {
        public IReadOnlyDictionary<string, StatesDTO> ByCode { get; set; }
        public IReadOnlyDictionary<string, StatesDTO> ByName { get; set; }

        public StateService(IWebHostEnvironment webHostEnvironment)
        {
            // Build the full file path to the JSON file
            // ContentRootPath = root directory of the application
            var path = Path.Combine(
                webHostEnvironment.ContentRootPath,
                "Resources",
                "States.json"
            );

            // Read the entire JSON file into a string
            var json = File.ReadAllText(path);

            // Deserialize JSON into a List of State DTO objects
            // Example JSON:
            // [{ "code": "GA", "name": "Georgia" }, ...]
            var states = JsonSerializer.Deserialize<List<StatesDTO>>(json) ?? new List<StatesDTO>();

            // Create a dictionary for fast lookup by state code
            // Key: state code (e.g. "GA", "CA")
            // Value: StatesDTO object
            // StringComparer.OrdinalIgnoreCase makes it case-insensitive
            // so "ga" and "GA" are treated the same
            ByCode = states.ToDictionary(
                x => x.Code,
                StringComparer.OrdinalIgnoreCase
            );

            // Create a dictionary for fast lookup by state name
            // Key: state name (e.g. "Georgia", "California")
            // Value: StatesDTO object
            // Also case-insensitive for flexible search
            ByName = states.ToDictionary(
                x => x.Name,
                StringComparer.OrdinalIgnoreCase
            );
        }
    }
}
