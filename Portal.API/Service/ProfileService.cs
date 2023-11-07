using Portal.API.Interfaces.Services;
using Portal.API.Dto.Profile;
using Portal.API.Interfaces.Repositories;
using AutoMapper;
using System.Threading.Tasks;
using System.Collections.Generic;
using Portal.API.Entities;
using Portal.API.Common.Dto;
using Portal.API.Dto.ProfileMenu;

namespace Portal.API.Service
{
    public class ProfileServicePortal : IProfileServicePortal
    {
        private readonly IProfileRepositoryPortal _profileRepository;
        private readonly IProfileMenuPortalRepository _profileMenuRepository;
        //Automapper
        private readonly IMapper _mapper;

        public ProfileServicePortal(IProfileRepositoryPortal profileRepository,
            IProfileMenuPortalRepository profileMenuRepository,
                              IMapper mapper)
        {
            _profileRepository = profileRepository;
            _profileMenuRepository = profileMenuRepository;
            _mapper = mapper;
        }

        public async Task<ResponseDto<PortalProfileDto>> GetAllPaged(int limit, int skip, string search, string order)
        {
            return _mapper.Map<ResponseDto<PortalProfileDto>>(await _profileRepository.GetAllPaged(limit, skip, search, order));
        }
        public async Task<IEnumerable<PortalProfileDto>> GetAll()
        {
            return _mapper.Map<IEnumerable<PortalProfileDto>>(await _profileRepository.GetAll());
        }

        public async Task<IEnumerable<PortalProfileDto>> GetBySistema(long sistemaId)
        {
            return _mapper.Map<IEnumerable<PortalProfileDto>>(await _profileRepository.GetBySistema(sistemaId));
        }

        public async Task<PortalProfileDto> Get(long id)
        {
            var profile = _mapper.Map<PortalProfileDto>(await _profileRepository.Get(id));
            profile.ProfileMenu = _mapper.Map<List<ProfileMenuDto>>(await _profileMenuRepository.GetByProfile(id));
            return profile;
        }

        public async Task<PortalProfileDto> Create(PortalProfileAddDto model)
        {
            var profile = _mapper.Map<Entities.ProfilePortal>(model);
            await _profileRepository.Create(profile);
            return _mapper.Map<PortalProfileDto>(profile);
        }

        public async Task<PortalProfileDto> Update(PortalProfileUpdateDto profile)
        {
            await _profileRepository.Update(_mapper.Map<Entities.ProfilePortal>(profile));
            await _profileMenuRepository.Delete(profile.Id);
            foreach (var pm in profile.ProfileMenu)
            {
                ProfileMenu newProfileMenu = new ProfileMenu()
                {
                    ProfileId = pm.ProfileId,
                    MenuId = pm.MenuId,
                    CanInsert= pm.CanInsert,
                    CanUpdate= pm.CanUpdate,
                    CanDelete= pm.CanDelete,
                };
                await _profileMenuRepository.Add(newProfileMenu);

            }
            return _mapper.Map<PortalProfileDto>(profile);
        }

        public async Task Delete(long id)
        {
            await _profileRepository.Delete(id);
        }

    }
}