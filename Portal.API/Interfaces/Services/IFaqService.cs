using Portal.API.Common.Dto;
using Portal.API.Dto.Faq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.API.Interfaces.Services
{
    public interface IFaqService
    {
        Task<IEnumerable<FaqDto>> GetAll();
        Task<IEnumerable<FaqDto>> GetActiveFaqs();
        Task<ResponseDto<FaqDto>> GetAllPaged(int limit, int skip, string search, string order);
        Task<FaqDto> Get(long id);
        Task<FaqDto> Create(FaqAddDto faq);
        Task<FaqDto> Update(FaqUpdateDto faq);
        Task Delete(long id);
        Task Activate(long[] ids);
        Task Deactivate(long[] ids);        
    }
}
