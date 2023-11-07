using Accounts.API.Common.Dto.Account;
using Accounts.API.Common.Dto.Branch;
using Accounts.API.Common.Dto.CostCenter;
using Accounts.API.Common.Dto.LimitRequest;
using Accounts.API.Common.Dto.Shop;
using Accounts.API.Common.Dto.Solicitation;
using Accounts.API.Entities;
using AutoMapper;
using Portal.API.Dto.Notification;
using Portal.API.Entities;


namespace Accounts.API.Common.Mapper
{
    public class Dto2Entity : Profile
    {
        public Dto2Entity()
        {
            CreateMap<AccountDto, Account>();
            CreateMap<BlockedAccountDto, BlockedAccount>();
            CreateMap<AccountAddDto, Account>();
            CreateMap<BranchDto, Branch>();
            CreateMap<ShopDto, Shop>();
            CreateMap<CostCenterDto, CostCenter>();
            CreateMap<LimitRequestDto, LimitRequest>();
            CreateMap<SolicitationDto, Solicitation>();
            CreateMap<SolicitationTypeDto, SolicitationType>();

            CreateMap<NotificationDto, Notification>();
            CreateMap<NotificationAddDto, Notification>();
            CreateMap<NotificationTypeDto, NotificationType>();
            CreateMap<StatusDismissal, StatusDismissalDto>();
        }
    }
}
