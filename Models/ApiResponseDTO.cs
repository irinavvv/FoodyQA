

using System.Text.Json.Serialization;

namespace FoodyExamPrep.Models
{
    internal class ApiResponseDTO
    {
        [JsonPropertyName("msg")]
        public string? Msg { get; set; }

        [JsonPropertyName("foodId")]
        public string FoodId { get; set; }
    }
}
