using Microsoft.Extensions.DependencyInjection;
using Portal.API.Common.Helpers;
using Portal.API.Common.Helpers.Interfaces;
using Portal.API.Common.Repositories;
using Portal.API.Common.Repositories.Interfaces;
using Portal.API.Integrations.Bullla.Interfaces;
using Portal.API.Integrations.Bullla;
using Portal.API.Integrations.Interfaces;
using Portal.API.Integrations.Ploomes;
using Portal.API.Interfaces.Repositories;
using Portal.API.Interfaces.Services;
using Portal.API.Repositories;
using Portal.API.Service;
using Portal.API.Common.Attributes;

namespace Portal.API.Common.Extensions
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
            services.AddTransient<AuditAttribute>();
            services.AddScoped<IHttpServicePortal, HttpServicePortal>();

            services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
            services.AddScoped<IBannerRepository, BannerRepository>();
            services.AddScoped<IFaqRepository, FaqRepository>();
            services.AddScoped<IManualRepository, ManualRepository>();
            services.AddScoped<IMenuRepository, MenuRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IDashboardRepository, DashboardRepository>();
            services.AddScoped<IMenuRepository, MenuRepository>();
            services.AddScoped<IProfileRepositoryPortal, ProfileRepositoryPortal>();
            services.AddScoped<IProfileMenuPortalRepository, ProfileMenuPortalRepository>();
            services.AddScoped<ISistemaRepository, SistemaRepository>();
            services.AddTransient<IAcessoRepository, AcessoRepository>();
            services.AddScoped<IReportRepository, ReportRepository>();

            services.AddScoped<IBannerService, BannerService>();
            services.AddScoped<IBlogService, BlogService>();
            services.AddScoped<IFaqService, FaqService>();
            services.AddScoped<IManualService, ManualService>();
            services.AddScoped<IMenuService, MenuService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IConfigurationService, ConfigurationService>();
            services.AddScoped<IMenuService, MenuService>();
            services.AddScoped<IProfileServicePortal, ProfileServicePortal>();
            services.AddScoped<ISistemaService, SistemaService>();
            services.AddScoped<IReportService, ReportService>();

            services.AddScoped<IPloomesIntegration, PloomesIntegration>();
            services.AddScoped<IBulllaIntegration, BulllaIntegration>();

            services.AddScoped<IRequestHelper, RequestHelper>();
            services.AddScoped<IIntegrationRepositoryBase, IntegrationRepositoryBase>();
        }
    }
}
