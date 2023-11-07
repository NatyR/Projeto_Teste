using Accounts.API.Integrations.BulllaEmpresa.Request;
using Accounts.API.Integrations.BulllaEmpresa.Responses;
using System.Collections.Generic;

namespace Accounts.API.Integrations.BulllaEmpresa.Interfaces
{
    public interface IBulllaEmpresaIntegration
    {
        BulllaEmpresaResponse CancelamentoConta(long convenioId,string user, List<BulllaEmpresaCancelamentoContaRequest> contas);
    }
}
