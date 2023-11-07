using System.Collections.Generic;
using System.Threading.Tasks;
using Users.API.Common.Dto;
using Users.API.Dto.Acesso;

namespace Users.API.Interfaces.Services
{
    public interface IAcessoService
    {
        Task<ResponseDto<AcessoDto>> GetAllPaged(AcessoFilterDto filterOptions, int limit, int skip, string order);
        Task<List<AcessoDto>> GetAll(AcessoFilterDto filterOptions);
        Task<AcessoDto> Get(long id);
    }
}
