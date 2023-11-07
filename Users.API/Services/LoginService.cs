using Users.API.Interfaces.Services;
using Users.API.Dto.Login;
using Users.API.Interfaces.Repositories;
using AutoMapper;
using System.Threading.Tasks;
using System.Collections.Generic;
using Users.API.Entities;
using Users.API.Common.Dto;

namespace Users.API.Services
{
    public class LoginService : ILoginService
    {
        private readonly ILoginRepository _loginRepository;
        //Automapper
        private readonly IMapper _mapper;

        public LoginService(ILoginRepository loginRepository,
                              IMapper mapper)
        {
            _loginRepository = loginRepository;
            _mapper = mapper;
        }


        public async Task<LoginDto> Get(long id)
        {
            return _mapper.Map<LoginDto>(await _loginRepository.Get(id));
        }

        public async Task<ResponseDto<LoginDto>> GetAllPaged(LoginFilterDto filterOptions , int limit, int skip, string order)
        {
            return _mapper.Map<ResponseDto<LoginDto>>(await _loginRepository.GetAllPaged(filterOptions, limit, skip, order));
        }
    }
}