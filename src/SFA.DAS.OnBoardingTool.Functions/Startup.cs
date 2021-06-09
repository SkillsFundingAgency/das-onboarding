using SFA.DAS.Configuration.AzureTableStorage;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SFA.DAS.OnBoardingTool.Infrastructure.Configuration;
using SFA.DAS.OnBoardingTool.Functions.StartupExtensions;

[assembly: FunctionsStartup(typeof(SFA.DAS.OnBoardingTool.Functions.Startup))]
namespace SFA.DAS.OnBoardingTool.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddNLog();

            var serviceProvider = builder.Services.BuildServiceProvider();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            var configBuilder = new ConfigurationBuilder()
                .AddConfiguration(configuration)
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables();

#if DEBUG
            configBuilder.AddJsonFile("local.settings.json", true);
#endif

            configBuilder.AddAzureTableStorage(options =>
            {
                options.ConfigurationKeys = configuration["ConfigNames"].Split(",");
                options.StorageConnectionString = configuration["ConfigurationStorageConnectionString"];
                options.EnvironmentName = configuration["Environment"];
                options.PreFixConfigurationKeys = false;
            });

            var config = configBuilder.Build();
            builder.Services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), config));
            
            builder.Services.AddOptions();
            builder.Services.Configure<OnBoardingToolFunctions>(config.GetSection("OnBoardingToolFunctions"));
            builder.Services.Configure<AtlassianApi>(config.GetSection("AtlassianApi"));
            builder.Services.AddServices();
        }
    }
}