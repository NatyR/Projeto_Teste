using Newtonsoft.Json;

namespace Portal.API.Integrations.Bullla.Responses
{
    public class BulllaResponseBase
    {
        [JsonProperty("codigoResposta")]
        public string ResponseCode { get; set; }

        [JsonProperty("mensagemResposta")]
        public string Message { get; set; }

        public bool IsValid => !string.IsNullOrEmpty(ResponseCode) && int.TryParse(ResponseCode, out var code) && code == 0;
    }
}
