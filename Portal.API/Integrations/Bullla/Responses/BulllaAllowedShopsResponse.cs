using Newtonsoft.Json;
using System.Collections.Generic;

namespace Portal.API.Integrations.Bullla.Responses
{
    public class BulllaAllowedShopsResponse : BulllaResponseBase
    {
        [JsonProperty("codigo")]
        public string Code { get; set; }

        [JsonProperty("randomToken")]
        public string Token { get; set; }

        [JsonProperty("mensagem")]
        public string Message { get; set; }

        [JsonProperty("uuid")]
        public string Uuid { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("listaPortalRh")]
        public List<BulllaAllowedShopResponse> Shops { get; set; }
    }

}
