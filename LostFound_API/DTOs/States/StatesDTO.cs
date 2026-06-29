using System.Text.Json.Serialization;

namespace LostFound_API.DTOs.States
{
    public class StatesDTO
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
