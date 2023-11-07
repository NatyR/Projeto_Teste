using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Users.API.Common.Attributes;
using Users.API.Interfaces.Repositories;
using Users.API.Interfaces.Services;
using Users.API.Repositories;
using Users.API.Services;

namespace Users.API.Common.Extensions
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
            services.AddScoped<IHttpService,HttpService>();
            /* Services */
            //services.AddTransient<IUserService, UserService>();
            services.AddTransient<ISistemaService, SistemaService>();
            services.AddTransient<IProfileService, ProfileService>();
            services.AddTransient<IAuthService, AuthService>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<ILoginService, LoginService>();
            services.AddTransient<IAcessoService, AcessoService>();


            /* Repositories */
            //services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<ISistemaRepository, SistemaRepository>();
            services.AddTransient<IProfileRepository, ProfileRepository>();
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IUserShopRepository, UserShopRepository>();
            services.AddTransient<ILoginRepository, LoginRepository>();
            services.AddTransient<IAcessoRepository, AcessoRepository>();

        }
    }
}
