using Accounts.API.Common.Dto.CostCenter;
using Accounts.API.Interfaces.Repositories;
using Accounts.API.Interfaces.Services;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Accounts.API.Services
{
    public class CostCenterService : ICostCenterService
    {
        private readonly ICostCenterRepository _costCenterRepository;
        private readonly IMapper _mapper;

        public CostCenterService(ICostCenterRepository costCenterRepository, IMapper mapper)
        {
            _costCenterRepository = costCenterRepository;
            _mapper = mapper;
        }
        public async Task<List<CostCenterDto>> GetAllByConvenio(long convenio)
        {
            var cost_centers = await _costCenterRepository.GetAllByConvenio(convenio);
            return _mapper.Map<List<CostCenterDto>>(cost_centers);

        }
    }
}
