using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.API.Integrations.Ploomes.Dto
{
    public class PloomesContactResponseDto
    {
        [JsonProperty("@odata.context")]
        public string dataContext { get; set; }
        [JsonProperty("value")]
        public IEnumerable<PloomesContactDto> value { get; set; }
    }
}
