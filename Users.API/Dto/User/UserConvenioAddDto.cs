using Newtonsoft.Json;

namespace Users.API.Dto.User
{
    public class UserConvenioAddDto
    {
        [JsonProperty("convenioId")]
        public long ConvenioId { get; set; }
        [JsonProperty("profileId")]
        public long ProfileId { get; set; }
    }
}
