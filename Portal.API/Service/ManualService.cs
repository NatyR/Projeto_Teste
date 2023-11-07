using AutoMapper;
using Portal.API.Common.Dto;
using Portal.API.Dto.Manual;
using Portal.API.Entities;
using Portal.API.Integrations.AwsS3;
using Portal.API.Interfaces.Repositories;
using Portal.API.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.API.Service
{
    public class ManualService : IManualService
    {
        private readonly IManualRepository _manualRepository;
        private readonly IMapper _mapper;

        public ManualService(IManualRepository manualRepository,
            IMapper mapper)
        {
            _manualRepository = manualRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ManualDto>> GetAll()
        {
            return _mapper.Map<IEnumerable<ManualDto>>(await _manualRepository.GetAll());
        }

        public async Task<IEnumerable<ManualDto>> GetActiveManuals()
        {
            return _mapper.Map<IEnumerable<ManualDto>>(await _manualRepository.GetActiveManuals());
        }
        public async Task<ManualDto> Get(long id)
        {
            return _mapper.Map<ManualDto>(await _manualRepository.Get(id));
        }

        public async Task<ManualDto> Create(ManualAddDto manual)
        {

            var entity = _mapper.Map<Manual>(manual);
            entity.Status = "ativo";
            await _manualRepository.Create(entity);
            if (manual.FormFile != null)
            {
                try
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        entity.FileName = manual.FormFile.FileName;
                        var key = "guides/" + entity.Id.ToString() + Path.GetExtension(manual.FormFile.FileName);
                        await manual.FormFile.CopyToAsync(memoryStream);
                        await AwsS3Integration.UploadFileAsync(memoryStream, key);
                        await _manualRepository.Update(entity);
                    }
                }
                catch (Exception ex)
                {
                    if (entity.Id > 0)
                    {
                        await _manualRepository.Delete(entity.Id);
                    }
                    throw new Exception("Não foi possível fazer o upload do arquivo");
                }
            }
            return _mapper.Map<ManualDto>(entity);
        }

        public async Task<ManualDto> Update(ManualUpdateDto manual)
        {
            var existing = await _manualRepository.Get(manual.Id);
            if (manual.FormFile != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    manual.FileName = manual.FormFile.FileName;
                    var key = "guides/" + manual.Id.ToString() + Path.GetExtension(manual.FormFile.FileName);
                    await manual.FormFile.CopyToAsync(memoryStream);
                    await AwsS3Integration.UploadFileAsync(memoryStream,key);
                }
            } else {
                manual.FileName = existing.FileName;
            }
            var entity = _mapper.Map<Manual>(manual);
            await _manualRepository.Update(entity);
            return _mapper.Map<ManualDto>(entity);
        }

        public async Task Delete(long id)
        {
            await _manualRepository.Delete(id);
        }

        public async Task Activate(long[] ids)
        {
            foreach (var id in ids)
            {
                await _manualRepository.Activate(id);
            }
        }

        public async Task Deactivate(long[] ids)
        {
            foreach (var id in ids)
            {
                await _manualRepository.Deactivate(id);
            }
        }
        public async Task<ResponseDto<ManualDto>> GetAllPaged(string manualType, int limit, int skip, string search, string order)
        {
            return _mapper.Map<ResponseDto<ManualDto>>(await _manualRepository.GetAllPaged(manualType, limit, skip, search, order));
        }

        public string GetSignedUrl(ManualDto manual)
        {
            return AwsS3Integration.GetSignedUrl("guides/" + manual.Id.ToString() + Path.GetExtension(manual.FileName));
        }
    }
}
