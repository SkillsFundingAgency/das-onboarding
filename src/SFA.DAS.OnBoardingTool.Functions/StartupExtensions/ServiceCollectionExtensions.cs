
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SFA.DAS.OnBoardingTool.Application;
using SFA.DAS.OnBoardingTool.Application.Atlassian;
using SFA.DAS.OnBoardingTool.Infrastructure.Validators;
using SFA.DAS.OnBoardingTool.Infrastructure.Api;
using SFA.DAS.OnBoardingTool.Infrastructure.Api.Atlassian;
using FluentValidation;
using NLog.Extensions.Logging;
using SFA.DAS.OnBoardingTool.Functions.Configuration;

namespace SFA.DAS.OnBoardingTool.Functions.StartupExtensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddSingleton<IApiClient, ApiClient>();
            services.AddSingleton<IAtlassianService, AtlassianService>();
            services.AddTransient<AbstractValidator<Value>, ValueValidator>();

            return services;
        }

        public static IServiceCollection AddNLog(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddLogging((options) =>
            {
                options.AddFilter(typeof(Startup).Namespace, LogLevel.Information);
                options.SetMinimumLevel(LogLevel.Trace);
                options.AddNLog(new NLogProviderOptions
                {
                    CaptureMessageTemplates = true,
                    CaptureMessageProperties = true
                });
                options.AddConsole();

                NLogConfiguration.ConfigureNLog();
            });

            return serviceCollection;
        }
    }
}