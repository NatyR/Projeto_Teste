using Newtonsoft.Json;

namespace Accounts.API.Integrations.BulllaEmpresa.Responses
{
    public class BulllaEmpresaResponse
    {
        [JsonProperty("codigo")]
        public string Codigo { get; set; }
        [JsonProperty("mensagem")]
        public string Mensagem { get; set; }
    }
}
