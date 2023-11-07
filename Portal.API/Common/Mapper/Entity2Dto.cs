using Portal.API.Dto.Faq;
using Portal.API.Dto.Banner;
using Portal.API.Dto.Manual;
using Portal.API.Entities;
using Portal.API.Common.Dto;
using Portal.API.Dto.Configuration;
using Portal.API.Dto.Notification;
using Portal.API.Dto.Menu;
using Portal.API.Dto.Profile;
using Portal.API.Dto.Sistema;
using Portal.API.Dto.ProfileMenu;
using Portal.API.Dto.Report;

namespace Portal.API.Common.Mapper
{
    public class Entity2Dto : AutoMapper.Profile
    {
        public Entity2Dto()
        {
            CreateMap<Faq, FaqDto>();
            CreateMap<Faq, FaqAddDto>();
            CreateMap<Faq, FaqUpdateDto>();
            CreateMap<ResponseDto<Faq>, ResponseDto<FaqDto>>();

            CreateMap<Banner, BannerDto>();
            CreateMap<Banner, BannerAddDto>();
            CreateMap<Banner, BannerUpdateDto>();
            CreateMap<ResponseDto<Banner>, ResponseDto<BannerDto>>();

            CreateMap<Manual, ManualDto>();
            CreateMap<Manual, ManualAddDto>();
            CreateMap<Manual, ManualUpdateDto>();
            CreateMap<ResponseDto<Manual>, ResponseDto<ManualDto>>();

            CreateMap<Menu, MenuDto>();
            CreateMap<Menu, GroupMenuDto>();
            CreateMap<Menu, MenuAddDto>();
            CreateMap<Menu, MenuUpdateDto>();
            CreateMap<ResponseDto<Menu>, ResponseDto<MenuDto>>();

            CreateMap<ProfilePortal, PortalProfileDto>();
            CreateMap<ProfilePortal, PortalProfileAddDto>();
            CreateMap<ProfilePortal, PortalProfileUpdateDto>();
            CreateMap<ResponseDto<ProfilePortal>, ResponseDto<PortalProfileDto>>();

            CreateMap<ProfileMenu, ProfileMenuDto>();

            CreateMap<Sistema, SistemaDto>();
            CreateMap<Sistema, SistemaAddDto>();
            CreateMap<Sistema, SistemaUpdateDto>();
            CreateMap<ResponseDto<Sistema>, ResponseDto<SistemaDto>>();

            CreateMap<Notification, NotificationDto>();
            CreateMap<ResponseDto<Notification>, ResponseDto<NotificationDto>>();
            CreateMap<NotificationType, NotificationTypeDto>();


            CreateMap<Configuration, ConfigurationDto>();
            CreateMap<Configuration, ConfigurationUpdateDto>();

            CreateMap<ReportCartao, ReportCartaoDto>();
            CreateMap<ReportContas, ReportContasDto>();

            CreateMap<PainelGerencial, PainelGerencialDto>();
            CreateMap<ResponseDto<PainelGerencial>, ResponseDto<PainelGerencialDto>>();

        }
    }
}
