using AutoMapper;
using Users.API.Dto.Acesso;
using Users.API.Dto.Login;
using Users.API.Dto.Profile;
using Users.API.Dto.Sistema;
using Users.API.Dto.User;
using Users.API.Entities;

namespace Users.API.Common.Mapper
{
    public class Dto2Entity : AutoMapper.Profile
    {
        public Dto2Entity()
        {
            CreateMap<UserDto, User>();
            CreateMap<UserAddDto, User>();
            CreateMap<UserUpdateDto, User>();
            CreateMap<UserStatusDto, User>();
            CreateMap<UserShopDto, UserShop>();
            CreateMap<UserMenuDto, UserMenu>();
            CreateMap<ProfileDto, Entities.Profile>();
            CreateMap<ProfileAddDto, Entities.Profile>();
            CreateMap<ProfileUpdateDto, Entities.Profile>();
            CreateMap<SistemaDto, Sistema>();
            CreateMap<SistemaAddDto, Sistema>();
            CreateMap<SistemaUpdateDto, Sistema>();
            CreateMap<AcessoDto, Acesso>();
            CreateMap<LoginDto, Login>();
        }
    }
}
