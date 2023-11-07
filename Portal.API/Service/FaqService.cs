using AutoMapper;
using Portal.API.Common.Dto;
using Portal.API.Dto.Faq;
using Portal.API.Entities;
using Portal.API.Interfaces.Repositories;
using Portal.API.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.API.Service
{
    public class FaqService : IFaqService
    {
        private readonly IFaqRepository _faqRepository;
        private readonly IMapper _mapper;

        public FaqService(IFaqRepository faqRepository,
            IMapper mapper)
        {
            _faqRepository = faqRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<FaqDto>> GetAll()
        {
            return _mapper.Map<IEnumerable<FaqDto>>(await _faqRepository.GetAll());
        }

        public async Task<IEnumerable<FaqDto>> GetActiveFaqs()
        {
            return _mapper.Map<IEnumerable<FaqDto>>(await _faqRepository.GetActiveFaqs());
        }

        public async Task<FaqDto> Get(long id)
        {
            return _mapper.Map<FaqDto>(await _faqRepository.Get(id));
        }

        private async Task reorder(long id, int order)
        {
            var faqs = await _faqRepository.GetFaqsForReorder(order);
            foreach (var f in faqs)
            {
                if (f.Id != id)
                {
                    f.Order = ++order;
                    await _faqRepository.Update(f);
                }
            }
        }
        public async Task<FaqDto> Create(FaqAddDto faq)
        {
            var entity = _mapper.Map<Faq>(faq);
            entity.Status = "ativo";
            await _faqRepository.Create(entity);
            await this.reorder(entity.Id, entity.Order);
            return _mapper.Map<FaqDto>(entity);
        }

        public async Task<FaqDto> Update(FaqUpdateDto faq)
        {
            var entity = _mapper.Map<Faq>(faq);
            await _faqRepository.Update(entity);
            await this.reorder(entity.Id, entity.Order);
            return _mapper.Map<FaqDto>(entity);
        }

        public async Task Delete(long id)
        {
            await _faqRepository.Delete(id);
        }

        public async Task Activate(long[] ids)
        {
            foreach (var id in ids)
            {
                await _faqRepository.Activate(id);
            }
        }

        public async Task Deactivate(long[] ids)
        {
            foreach (var id in ids)
            {
                await _faqRepository.Deactivate(id);
            }
        }

        public async Task<ResponseDto<FaqDto>> GetAllPaged(int limit, int skip, string search, string order)
        {
            return _mapper.Map<ResponseDto<FaqDto>>(await _faqRepository.GetAllPaged(limit, skip, search, order));
        }
    }
}
