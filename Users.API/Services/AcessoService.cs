using Users.API.Interfaces.Services;
using Users.API.Dto.Acesso;
using Users.API.Interfaces.Repositories;
using AutoMapper;
using System.Threading.Tasks;
using System.Collections.Generic;
using Users.API.Entities;
using Users.API.Common.Dto;

namespace Users.API.Services
{
    public class AcessoService : IAcessoService
    {
        private readonly IAcessoRepository _acessoRepository;
        //Automapper
        private readonly IMapper _mapper;

        public AcessoService(IAcessoRepository acessoRepository,
                              IMapper mapper)
        {
            _acessoRepository = acessoRepository;
            _mapper = mapper;
        }


        public async Task<AcessoDto> Get(long id)
        {
            return _mapper.Map<AcessoDto>(await _acessoRepository.Get(id));
        }

        public async Task<ResponseDto<AcessoDto>> GetAllPaged(AcessoFilterDto filterOptions, int limit, int skip, string order)
        {
            return _mapper.Map<ResponseDto<AcessoDto>>(await _acessoRepository.GetAllPaged(filterOptions, limit, skip, order));
        }
        public async Task<List<AcessoDto>> GetAll(AcessoFilterDto filterOptions)
        {
            return _mapper.Map<List<AcessoDto>>(await _acessoRepository.GetAll(filterOptions));
        }
    }
}