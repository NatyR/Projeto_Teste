using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.API.Integrations.Ploomes.Dto
{
    public class PloomesOwnerDto
    {
        [JsonProperty("Id")]
        public int Id { get; set; }
        [JsonProperty("Name")]
        public string Name { get; set; }
        [JsonProperty("Phone")]
        public string Phone { get; set; }
        [JsonProperty("Email")]
        public string Email { get; set; }
        [JsonProperty("AvatarUrl")]
        public string AvatarUrl { get; set; }

    }
}
