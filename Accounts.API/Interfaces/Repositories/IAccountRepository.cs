using Accounts.API.Common.Dto;
using Accounts.API.Common.Dto.Account;
using Accounts.API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Accounts.API.Interfaces.Repositories
{
    public interface IAccountRepository
    {
        Task<List<Account>> GetAllByConvenio(long convenio);
        Task<Account> GetByRegistrationNumber(string registration, long convenio);
        Task<Account> GetByCpfAndConvenio(string cpf, long convenio);
        Task<Account> GetByCpfAndGrupo(string cpf, long grupo);
        Task<Account> Get(long id, long convenio);
        Task<ResponseDto<Account>> GetAllPaged(long convenio_id, int limit, int skip, string search, string order, string? accountStatus, string? cardStatus, string? cardsFilter);
        Task<ResponseDto<BlockedAccount>> GetBlockedPaged(int limit, int skip, string order, BlockedAccountFilterDto filter);
        Task<List<BlockedAccount>> GetBlocked(BlockedAccountFilterDto filter);
        Task<Account> Create(Account account);
        Task<StatusDismissal> GetStatusDismissal(string cpf);

    }
}
