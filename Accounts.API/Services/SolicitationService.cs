using Accounts.API.Common.Dto;
using Accounts.API.Common.Dto.Account;
using Accounts.API.Common.Dto.Solicitation;
using Accounts.API.Common.Dto.User;
using Accounts.API.Entities;
using Accounts.API.Integrations.AwsS3;
using Accounts.API.Interfaces.Repositories;
using Accounts.API.Interfaces.Services;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Accounts.API.Services
{
    public class SolicitationService : ISolicitationService
    {
        private readonly ISolicitationRepository _solicitationRepository;
        private readonly IMapper _mapper;

        public SolicitationService(
            ISolicitationRepository solicitationRepository,
            IMapper mapper)
        {
            _solicitationRepository = solicitationRepository;
            _mapper = mapper;
        }
        public async Task<ResponseDto<SolicitationDto>> GetAllPagedByConvenio(long convenio_id, SolicitationFilterDto filter, int limit, int skip, string order)
        {
            var entities = await _solicitationRepository.GetAllPagedByConvenio(convenio_id, filter, limit, skip, order);
            var dtos = _mapper.Map<ResponseDto<SolicitationDto>>(entities);
            return dtos;
        }
        public async Task<ResponseDto<SolicitationDto>> GetAllPaged(SolicitationFilterDto filter, int limit, int skip, string order)
        {
            var entities = await _solicitationRepository.GetAllPaged(filter, limit, skip, order);
            var dtos = _mapper.Map<ResponseDto<SolicitationDto>>(entities);
            return dtos;
        }
        public async Task<List<SolicitationDto>> GetAll(SolicitationFilterDto filter)
        {
            var entities = await _solicitationRepository.GetAll(filter);
            var dtos = _mapper.Map<List<SolicitationDto>>(entities);
            return dtos;
        }

        public async Task<SolicitationDto> GetById(long id)
        {
            var entity = await _solicitationRepository.GetById(id);
            var dto = _mapper.Map<SolicitationDto>(entity);
            return dto;
        }

        public async Task<List<SolicitationTypeDto>> GetTypes()
        {
            var entity = await _solicitationRepository.GetTypes();
            return _mapper.Map<List<SolicitationTypeDto>>(entity);
        }

        public async Task RegisterCallback(BulllaEmpresaCallbackDto callback)
        {
            foreach(var account in callback.listaConta)
            {
                var solicitation = await _solicitationRepository.GetById(account.idRegistro);
                if (solicitation == null)
                    continue;
                solicitation.BlockTypeId = account.idTipoBloqueio;
                solicitation.AccountStatus = account.statusConta;
                solicitation.Observation = account.mensagemProcessamento;
                if (account.sucessoTransacao)
                {
                    await _solicitationRepository.RegisterSuccess(solicitation.Id, solicitation);
                } else
                {
                    await _solicitationRepository.RegisterError(solicitation.Id, solicitation);
                }
            }
            
            
        }
    }
}
