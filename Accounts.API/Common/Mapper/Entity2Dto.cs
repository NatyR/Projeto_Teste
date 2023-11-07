using Accounts.API.Common.Dto;
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
    public class Entity2Dto : Profile
    {
        public Entity2Dto()
        {
            CreateMap<Account, AccountDto>();
            CreateMap<ResponseDto<Account>, ResponseDto<AccountDto>>();
            CreateMap<BlockedAccount, BlockedAccountDto>();
            CreateMap<ResponseDto<BlockedAccount>, ResponseDto<BlockedAccountDto>>();
            CreateMap<Branch, BranchDto>();
            CreateMap<Shop, ShopDto>();
            CreateMap<CostCenter, CostCenterDto>();
            CreateMap<GroupLimit, GroupLimitDto>();
            CreateMap<LimitRequest, LimitRequestDto>();
            CreateMap<ResponseDto<LimitRequest>, ResponseDto<LimitRequestDto>>();
            CreateMap<Solicitation, SolicitationDto>();
            CreateMap<ResponseDto<Solicitation>, ResponseDto<SolicitationDto>>();
            CreateMap<SolicitationType, SolicitationTypeDto>();
            CreateMap<Notification, NotificationDto>();
            CreateMap<StatusDismissal, StatusDismissalDto>();

        }
    }
}
