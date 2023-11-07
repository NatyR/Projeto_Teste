using Accounts.API.Common.Dto.Branch;
using Accounts.API.Interfaces.Repositories;
using Accounts.API.Interfaces.Services;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Accounts.API.Services
{
    public class BranchService : IBranchService
    {
        private readonly IBranchRepository _branchRepository;
        private readonly IMapper _mapper;

        public BranchService(IBranchRepository branchRepository, IMapper mapper)
        {
            _branchRepository = branchRepository;
            _mapper = mapper;
        }
        public async Task<List<BranchDto>> GetAllByConvenio(long convenio)
        {
            var branches = await _branchRepository.GetAllByConvenio(convenio);
            return _mapper.Map<List<BranchDto>>(branches);
        }
    }
}
