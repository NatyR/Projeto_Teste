using System;

namespace Accounts.API.Integrations.BulllaEmpresa.Responses
{
    public class BulllaEmpresaAuthResponse
    {
        public string dataValidade { get; set; }
        public string token { get; set; }
        public string dataGeracao { get; set; }
    }
}
