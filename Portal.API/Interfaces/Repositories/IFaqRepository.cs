using Portal.API.Common.Dto;
using Portal.API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.API.Interfaces.Repositories
{
    public interface IFaqRepository
    {
        Task<List<Faq>> GetActiveFaqs();

        Task<IEnumerable<Faq>> GetAll();
        Task<ResponseDto<Faq>> GetAllPaged(int limit, int skip, string search, string order);
        Task<List<Faq>> GetFaqsForReorder(int order);
        Task<Faq> Get(long id);
        Task<Faq> Create(Faq faq);
        Task<Faq> Update(Faq faq);
        Task Delete(long id);
        Task Activate(long id);
        Task Deactivate(long id);
    }
}
