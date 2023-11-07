
using Portal.API.Dto.Configuration;
using Portal.API.Integrations.Ploomes.Dto;
using System.Threading.Tasks;

namespace Portal.API.Interfaces.Services
{
    public interface IConfigurationService
    {
        
        Task<PloomesOwnerDto> GetOwner(int grupo);
        Task<ConfigurationDto> Update(ConfigurationUpdateDto config);

    }
}
