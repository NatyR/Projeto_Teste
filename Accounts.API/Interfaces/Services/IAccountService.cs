using Accounts.API.Common.Dto;
using Accounts.API.Common.Dto.Account;
using Accounts.API.Common.Dto.Rabbit;
using Accounts.API.Common.Dto.User;
using Accounts.API.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace Accounts.API.Interfaces.Services
{
    public interface IAccountService
    {
        Task<List<AccountDto>> GetAllByConvenio(long convenio);
        Task<AccountDto> GetByRegistrationNumber(string registration, long convenio);
        Task<AccountDto> GetByCpfAndConvenio(string cpf, long convenio);
        Task<AccountDto> GetByCpfAndGrupo(string cpf, long grupo);
        Task<AccountDto> Get(long id, long convenio);
        Task<ResponseDto<AccountDto>> GetAllPaged(long convenio_id, int limit, int skip, string search, string order, string? accountStatus, string? cardStatus, string? cardsFilter);
        Task<ResponseDto<BlockedAccountDto>> GetBlockedPaged(int limit, int skip, string order, BlockedAccountFilterDto filter);
        Task<List<BlockedAccountDto>> GetBlocked(BlockedAccountFilterDto filter);
        Task Create(AccountAddDto account);
        Task Update(AccountAddDto account);
        Task Block(AccountBlockDto accounts);
        
        Task Dismissal(AccountDismissalDto accounts);
        Task Unblock(AccountUnblockDto accounts);
        Task Reissue(AccountReissueDto accounts);
        
        Task ChangeLimit(AccountLimitDto accounts);
        Task RegisterImport(FileUploadDto fileUpload, long convenio, UserDtoAccounts currentUser);
        Task<AccountValidationDto> ValidateFile(FileUploadDto fileUpload, long convenio);
        Task UploadFileNewCard(FileUploadDto fileUpload, long convenio, UserDtoAccounts currentUser);
        Task<AccountValidationDto> ValidateFileDismissal(FileUploadDto fileUpload, long convenio);
        Task ImportFileDismissal(FileUploadDto fileUpload, long convenio, UserDtoAccounts user);
        Task<List<MessageAttachmentDto>> GlobalLimit(GlobalLimitFileUploadDto model);
        Task<string> SecurityUnblock(long convenio, AccountModel data, int userId);
        Task<dynamic> ContasDesligadas(long convenio, string cpfColaborador, DateTime? dataInicio, DateTime? dataFim, int? pagina, int? tamanhoPagina, int userId, int? statusRecisao);
        Task<dynamic> Faturas(long convenio, int idConta, int userId);
        Task<dynamic> Contas(long convenio, string cpf, string email, string nome, int? pagina, int? tamanhoPagina, string ascendente, string ordem, string status, int userId);
        Task<dynamic> AbatimentoDivida(long convenio, AbatimentoDivida data, int userId);
        Task<dynamic> FaturasResidual(long convenio, int idConta, int userId);
        Task<dynamic> Cargas(long convenio, int idConta, int userId);
        Task<dynamic> Cartoes(long convenio, string cpf, string email, string nome, int? pagina, int? tamanhoPagina, string ascendente, string ordem, string status, int userId);
        Task<string> JobNotificationEmail();
        Task<ReportDismissalDiscount> ReportDismissalDiscount(long? shopId, long? groupId, DateTime? dtStart, DateTime? dtEnd, int? page, int? pageSize, int userId, string userMail, string userType, string filter);
        Task<MemoryStream> ReportDismissalCSV(long? shopId, long? groupId, DateTime? dtStart, DateTime? dtEnd, int userId, string userMail, string userType, string search);

    }
}
