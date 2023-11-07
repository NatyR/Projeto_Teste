using Newtonsoft.Json;

namespace Portal.API.Integrations.Bullla.Responses
{
    public class BulllaAllowedShopResponse
    {

        [JsonProperty("nomeEmpresa")]
        public string CompanyName { get; set; }

        [JsonProperty("codEmpresa")]
        public string CompanyCode { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

    }
}
