using Users.API.Interfaces.Services;
using Users.API.Dto.Profile;
using Users.API.Interfaces.Repositories;
using AutoMapper;
using System.Threading.Tasks;
using System.Collections.Generic;
using Users.API.Entities;

namespace Users.API.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IProfileRepository _profileRepository;
        //Automapper
        private readonly IMapper _mapper;

        public ProfileService(IProfileRepository profileRepository,
                              IMapper mapper)
        {
            _profileRepository = profileRepository;
            _mapper = mapper;
        }


        public async Task<IEnumerable<ProfileDto>> GetAll()
        {
            return _mapper.Map<IEnumerable<ProfileDto>>(await _profileRepository.GetAll());
        }

        public async Task<IEnumerable<ProfileDto>> GetBySistema(long sistemaId)
        {
            return _mapper.Map<IEnumerable<ProfileDto>>(await _profileRepository.GetBySistema(sistemaId));
        }

        public async Task<ProfileDto> Get(long id)
        {
            return _mapper.Map<ProfileDto>(await _profileRepository.Get(id));
        }

        public async Task<ProfileDto> Add(ProfileAddDto profile)
        {
            await _profileRepository.Add(_mapper.Map<Entities.Profile>(profile));
            return _mapper.Map<ProfileDto>(profile);
        }

        public async Task<ProfileDto> Update(ProfileUpdateDto profile)
        {
            await _profileRepository.Update(_mapper.Map<Entities.Profile>(profile));
            return _mapper.Map<ProfileDto>(profile);
        }

        public async Task Delete(long id)
        {
            await _profileRepository.Delete(id);
        }

    }
}