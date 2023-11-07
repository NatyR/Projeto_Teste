using Portal.API.Dto.Faq;
using Portal.API.Dto.Banner;
using Portal.API.Dto.Manual;
using Portal.API.Dto.Menu;
using Portal.API.Dto.Profile;
using Portal.API.Dto.Sistema;
using Portal.API.Entities;
using Portal.API.Dto.Configuration;
using Portal.API.Dto.Notification;
using Portal.API.Dto.ProfileMenu;
using Portal.API.Dto.Report;

namespace Portal.API.Common.Mapper
{
    public class Dto2Entity : AutoMapper.Profile
    {
        public Dto2Entity()
        {
            CreateMap<FaqDto, Faq>();
            CreateMap<FaqAddDto, Faq>();
            CreateMap<FaqUpdateDto, Faq>();

            CreateMap<BannerDto, Banner>();
            CreateMap<BannerAddDto, Banner>();
            CreateMap<BannerUpdateDto, Banner>();

            CreateMap<ManualDto, Manual>();
            CreateMap<ManualAddDto, Manual>();
            CreateMap<ManualUpdateDto, Manual>();

            CreateMap<MenuDto, Menu>();
            CreateMap<MenuAddDto, Menu>();
            CreateMap<MenuUpdateDto, Menu>();

            CreateMap<PortalProfileDto, ProfilePortal>();
            CreateMap<PortalProfileAddDto, ProfilePortal>();
            CreateMap<PortalProfileUpdateDto, ProfilePortal>();
            CreateMap<PortalProfileUpdateDto, PortalProfileDto>();

            CreateMap<ProfileMenuDto, ProfileMenu>();

            CreateMap<SistemaDto, Sistema>();
            CreateMap<SistemaAddDto, Sistema>();
            CreateMap<SistemaUpdateDto, Sistema>();
            
            CreateMap<NotificationDto, Notification>();
            CreateMap<NotificationAddDto, Notification>();
            CreateMap<NotificationUpdateDto, Notification>();
            CreateMap<NotificationTypeDto, NotificationType>();


            CreateMap<ConfigurationDto, Configuration>();
            CreateMap<ConfigurationUpdateDto, Configuration>();

            CreateMap<PainelGerencialDto, PainelGerencial>();
        }
    }
}
