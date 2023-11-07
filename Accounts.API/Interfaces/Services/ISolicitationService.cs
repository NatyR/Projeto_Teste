using Accounts.API.Common.Dto;
using Accounts.API.Common.Dto.Account;
using Accounts.API.Common.Dto.Solicitation;
using Accounts.API.Common.Dto.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Accounts.API.Interfaces.Services
{
    public interface ISolicitationService
    {
        Task<ResponseDto<SolicitationDto>> GetAllPagedByConvenio(long convenio_id, SolicitationFilterDto filter, int limit, int skip, string order);
        Task<ResponseDto<SolicitationDto>> GetAllPaged(SolicitationFilterDto filter, int limit, int skip, string order);
        Task<List<SolicitationDto>> GetAll(SolicitationFilterDto filter);
        Task<SolicitationDto> GetById(long id);
        Task<List<SolicitationTypeDto>> GetTypes();

        Task RegisterCallback(BulllaEmpresaCallbackDto callback);
    }
}
