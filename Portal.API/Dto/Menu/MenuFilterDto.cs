using System.Text.Json.Serialization;

namespace Portal.API.Dto.Menu
{
    public class MenuFilterDto
    {
        [JsonPropertyName("groupId")]
        public long? GroupId { get; set; }
        [JsonPropertyName("sistemaId")]
        public long? SistemaId { get; set; }
        [JsonPropertyName("searchTerm")]
        public string SearchTerm { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
    }
}
