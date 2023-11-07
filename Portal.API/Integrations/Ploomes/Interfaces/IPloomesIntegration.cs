using Portal.API.Integrations.Ploomes.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.API.Integrations.Interfaces
{
   public interface IPloomesIntegration
    {
        PloomesOwnerDto getOwnerByConvenio(int grupo);
    }
}
