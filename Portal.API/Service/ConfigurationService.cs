using AutoMapper;
using Portal.API.Dto.Configuration;
using Portal.API.Entities;
using Portal.API.Integrations.Interfaces;
using Portal.API.Integrations.Ploomes.Dto;
using Portal.API.Interfaces.Repositories;
using Portal.API.Interfaces.Services;
using System.Threading.Tasks;

namespace Portal.API.Service
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IPloomesIntegration _integration;
        private readonly IMapper _mapper;
        private readonly IConfigurationRepository _configurationRepository;

        public ConfigurationService(IPloomesIntegration integration,
            IConfigurationRepository configurationRepository,
            IMapper mapper)
        {
            _integration = integration;
            _mapper = mapper;
            _configurationRepository = configurationRepository;
        }

        public async Task<PloomesOwnerDto> GetOwner(int grupo)
        {
            return await Task.FromResult(_integration.getOwnerByConvenio(grupo));
        }

        public async Task<ConfigurationDto> Update(ConfigurationUpdateDto config)
        {
            var entity = _mapper.Map<Configuration>(config);
            await _configurationRepository.Update(entity);
            return _mapper.Map<ConfigurationDto>(entity);
        }
    }
}
