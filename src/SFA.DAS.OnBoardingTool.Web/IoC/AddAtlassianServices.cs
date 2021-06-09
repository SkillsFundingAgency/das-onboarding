using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.OnBoardingTool.Application;
using SFA.DAS.OnBoardingTool.Infrastructure.Api;

namespace SFA.DAS.OnBoardingTool.Web.IoC
{
    public static class AddAtlassianServicesExtensions
    {
        public static void AddAtlassianServices(
            this IServiceCollection services, 
            IConfiguration configuration
        )
        {
            services.AddTransient<IApiClient, Infrastructure.Api.Atlassian.ApiClient>();
            services.AddTransient<IAtlassianService, SFA.DAS.OnBoardingTool.Application.Atlassian.AtlassianService>();
        }
    }
}