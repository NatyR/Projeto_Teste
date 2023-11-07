using Accounts.API.Common.Dto;
using Accounts.API.Common.Dto.Solicitation;
using Accounts.API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Accounts.API.Interfaces.Repositories
{
    public interface ISolicitationRepository
    {
        Task<Solicitation> Create(Solicitation solicitation);
        Task<Solicitation> GetById(long id);
        Task<List<SolicitationType>> GetTypes();
        Task<ResponseDto<Solicitation>> GetAllPagedByConvenio(long convenio_id, SolicitationFilterDto filter, int limit, int skip, string order);
        Task<ResponseDto<Solicitation>> GetAllPaged(SolicitationFilterDto filter, int limit, int skip, string order);
        Task<List<Solicitation>> GetAll(SolicitationFilterDto filter);
        Task RegisterError(long id, Solicitation solicitation);
        Task RegisterSuccess(long id, Solicitation solicitation);
    }
}
