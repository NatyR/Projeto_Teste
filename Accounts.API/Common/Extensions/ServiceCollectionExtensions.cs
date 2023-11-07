using Accounts.API.Common.Attributes;
using Accounts.API.Integrations.AwsS3;
using Accounts.API.Interfaces.Repositories;
using Accounts.API.Interfaces.Services;
using Accounts.API.Repositories;
using Accounts.API.Services;
using Microsoft.Extensions.DependencyInjection;
using Accounts.API.Integrations.BulllaEmpresa.Interfaces;
using Accounts.API.Integrations.BulllaEmpresa;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Accounts.API.Common.Helpers.Interfaces;
using Accounts.API.Common.Helpers;
using Accounts.API.Common.Repositories.Interfaces;
using Accounts.API.Common.Repositories;
using Accounts.API.Integrations.As;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Users.API.Interfaces.Services;
using Users.API.Services;
using Users.API.Interfaces.Repositories;
using Users.API.Repositories;
using Portal.API.Service;
using Portal.API.Interfaces.Services;
using Portal.API.Interfaces.Repositories;
using Portal.API.Repositories;

namespace Accounts.API.Common.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Método responsável por realizar o bind de dependências
        /// </summary>
        /// <param name="services">Coleção de serviços do startup</param>
        public static void AddDependencyInjections(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddTransient<AuditAttribute>();
            services.AddScoped<IHttpServiceAccounts, HttpServiceAccounts>();
            services.AddScoped<IHttpService, HttpService>();

            /* Services */
            services.AddTransient<IAccountService, AccountService>();
            services.AddTransient<IBranchService, BranchService>();
            services.AddTransient<IShopService, ShopService>();
            services.AddTransient<ICostCenterService, CostCenterService>();
            services.AddTransient<ILimitRequestService, LimitRequestService>();
            services.AddTransient<ISolicitationService, SolicitationService>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IAuthService, AuthService>();
            services.AddTransient<IProfileService, ProfileService>();
            services.AddTransient<IProfileServicePortal, ProfileServicePortal>();
            services.AddTransient<INotificationService, NotificationService>();

            



            /* Repositories */
            services.AddTransient<IAccountRepository, AccountRepository>();
            services.AddTransient<IBranchRepository, BranchRepository>();
            services.AddTransient<IShopRepository, ShopRepository>();
            services.AddTransient<ICostCenterRepository, CostCenterRepository>();
            services.AddTransient<ILimitRequestRepository, LimitRequestRepository>();
            services.AddTransient<IAccessRepositoryAccounts, AcessoRepositoryAccounts>();
            services.AddTransient<ISolicitationRepository, SolicitationRepository>();
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IUserShopRepository, UserShopRepository>();
            services.AddTransient<ILoginRepository, LoginRepository>();
            services.AddTransient<IProfileRepository, ProfileRepository>();
            services.AddTransient<IProfileRepositoryPortal, ProfileRepositoryPortal>();
            services.AddTransient<IProfileMenuPortalRepository, ProfileMenuPortalRepository>();
            services.AddTransient<INotificationRepository, NotificationRepository>();



            
            services.AddScoped<IBulllaEmpresaIntegration, BulllaEmpresaIntegration>();
            services.AddScoped<IAsIntegration, AsIntegration>();

            services.AddScoped<IRequestHelper, RequestHelper>();
            services.AddScoped<IIntegrationRepositoryBase, IntegrationRepositoryBase>();


        }
    }
}
