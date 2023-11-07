using System.Collections.Generic;

namespace Accounts.API.Common.Dto.Account
{
    public class BulllaEmpresaCallbackDto
    {
        public long convenio { get; set; }
        public string usuario { get; set; }
        public string canal { get; set; }
        public string aplicacao { get; set; }
        public string urlRetorno { get; set; }
        public List<BulllaEmpresaCallbackAccountsDto> listaConta { get; set; }
        

    }
}
