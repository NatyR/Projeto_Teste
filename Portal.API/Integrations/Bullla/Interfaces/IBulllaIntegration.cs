using Portal.API.Integrations.Bullla.Responses;
using System.Collections.Generic;

namespace Portal.API.Integrations.Bullla.Interfaces
{
    public interface IBulllaIntegration
    {
        BulllaAllowedShopsResponse GetShops(string email);
    }
}
