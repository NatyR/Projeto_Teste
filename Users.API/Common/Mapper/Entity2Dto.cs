using Users.API.Dto;
using Users.API.Dto.Acesso;
using Users.API.Dto.Login;
using Users.API.Dto.User;
using Users.API.Dto.Profile;
using Users.API.Dto.Sistema;
using Users.API.Entities;
using AutoMapper;
using Users.API.Common.Dto;


namespace Users.API.Common.Mapper
{
    public class Entity2Dto : AutoMapper.Profile
    {
        public Entity2Dto()
        {
            CreateMap<User, UserDto>();
            CreateMap<UserShop, UserShopDto>();
            CreateMap<ResponseDto<UserMenuDto>, UserMenu>();
            CreateMap<ResponseDto<User>, ResponseDto<UserDto>>();
            CreateMap<Entities.Profile, ProfileDto>();
            CreateMap<Sistema, SistemaDto>();
            CreateMap<Acesso, AcessoDto>();
            CreateMap<ResponseDto<Acesso>, ResponseDto<AcessoDto>>();
            CreateMap<Login, LoginDto>();
            CreateMap<ResponseDto<Login>, ResponseDto<LoginDto>>();

        }
    }
}
