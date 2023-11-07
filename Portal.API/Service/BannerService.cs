using AutoMapper;
using Portal.API.Common.Dto;
using Portal.API.Dto.Banner;
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
    public class BannerService : IBannerService
    {
        private readonly IBannerRepository _bannerRepository;
        private readonly IMapper _mapper;

        public BannerService(IBannerRepository bannerRepository,
            IMapper mapper)
        {
            _bannerRepository = bannerRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<BannerDto>> GetAll()
        {
            var banners = _mapper.Map<IEnumerable<BannerDto>>(await _bannerRepository.GetAll());
            foreach (var banner in banners)
            {
                banner.ImageSrc = AwsS3Integration.GetSignedUrl("banners/" + banner.Id.ToString() + Path.GetExtension(banner.Image),60);
            }
            return banners;

        }

        public async Task<IEnumerable<BannerDto>> GetActiveBanners()
        {
            var banners = _mapper.Map<IEnumerable<BannerDto>>(await _bannerRepository.GetActiveBanners());
            foreach (var banner in banners)
            {
                banner.ImageSrc = AwsS3Integration.GetSignedUrl("banners/" + banner.Id.ToString() + Path.GetExtension(banner.Image), 60);
            }
            return banners;
        }
        public async Task<BannerDto> Get(long id)
        {
            var banner  = _mapper.Map<BannerDto>(await _bannerRepository.Get(id));
            banner.ImageSrc = AwsS3Integration.GetSignedUrl("banners/" + banner.Id.ToString() + Path.GetExtension(banner.Image), 60);
            return banner;

        }

        public async Task<BannerDto> Create(BannerAddDto banner)
        {
            var entity = _mapper.Map<Banner>(banner);
            await _bannerRepository.Create(entity);
            if (banner.FormFile != null)
            {
                try
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        entity.Image = banner.FormFile.FileName;
                        var key = "banners/" + entity.Id.ToString() + Path.GetExtension(banner.FormFile.FileName);
                        await banner.FormFile.CopyToAsync(memoryStream);
                        await AwsS3Integration.UploadFileAsync(memoryStream, key);
                        await _bannerRepository.Update(entity);
                    }
                }
                catch (Exception ex)
                {
                    if (entity.Id > 0)
                    {
                        await _bannerRepository.Delete(entity.Id);
                    }
                    throw new Exception("Não foi possível fazer o upload da imagem");
                }
            }
            return _mapper.Map<BannerDto>(entity);
        }

        public async Task<BannerDto> Update(BannerUpdateDto banner)
        {
            var existing = await _bannerRepository.Get(banner.Id);
            if (banner.FormFile != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    banner.Image = banner.FormFile.FileName;
                    var key = "banners/" + banner.Id.ToString() + Path.GetExtension(banner.FormFile.FileName);
                    await banner.FormFile.CopyToAsync(memoryStream);
                    await AwsS3Integration.UploadFileAsync(memoryStream, key);
                }
            }
            else
            {
                banner.Image = existing.Image;
            }
            var entity = _mapper.Map<Banner>(banner);
            await _bannerRepository.Update(entity);
            return _mapper.Map<BannerDto>(entity);
        }

        public async Task Delete(long id)
        {
            await _bannerRepository.Delete(id);
        }

        public async Task<ResponseDto<BannerDto>> GetAllPaged(int limit, int skip, string search, string order)
        {
            return _mapper.Map<ResponseDto<BannerDto>>(await _bannerRepository.GetAllPaged(limit, skip, search, order));
        }

        public async Task Activate(long[] ids)
        {
            foreach(var id in ids)
            {
                await _bannerRepository.Activate(id);
            }
        }

        public async Task Deactivate(long[] ids)
        {
            foreach (var id in ids)
            {
                await _bannerRepository.Deactivate(id);
            }
        }
        public string GetSignedUrl(BannerDto banner)
        {
            return AwsS3Integration.GetSignedUrl("banners/" + banner.Id.ToString() + Path.GetExtension(banner.Image), 60);
        }
    }
}
