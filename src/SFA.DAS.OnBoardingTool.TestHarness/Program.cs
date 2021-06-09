using System;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.OnBoardingTool.Application;
using SFA.DAS.OnBoardingTool.Application.Atlassian;
using SFA.DAS.OnBoardingTool.Infrastructure.Api;
using SFA.DAS.OnBoardingTool.Infrastructure.Api.Atlassian;
using SFA.DAS.OnBoardingTool.Infrastructure.Configuration;
using SFA.DAS.OnBoardingTool.Infrastructure.Validators;

namespace SFA.DAS.OnBoardingTool.TestHarness
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .AddAzureTableStorage("SFA.DAS.OnBoardingTool.Functions");

            var configuration = builder.Build();

            var serviceProvider = new ServiceCollection()
                .AddLogging(b => 
                {
                    b.AddConsole();
                })
                .AddOptions()          
                .Configure<OnBoardingToolFunctions>(configuration.GetSection("SFA.DAS.OnBoardingTool.Functions:OnBoardingToolFunctions"))
                .Configure<AtlassianApi>(configuration.GetSection("SFA.DAS.OnBoardingTool.Functions:AtlassianApi"))
                .AddSingleton<IAtlassianService, AtlassianService>()
                .AddSingleton<IApiClient, ApiClient>()
                .AddSingleton<AbstractValidator<Value>, ValueValidator>()
                .BuildServiceProvider();

            ConsoleKey key = ConsoleKey.Escape;

            var service = serviceProvider.GetRequiredService<IAtlassianService>();

            while(key != ConsoleKey.Q)
            {
                Console.Clear();
                Console.WriteLine("C - Process New User Queue");
                Console.WriteLine("R - Process Revoke User Queue");
                Console.WriteLine("G - GetUser");
                Console.WriteLine("Q - Quit");
                
                key = Console.ReadKey().Key;
                
                switch(key)
                {
                    case ConsoleKey.C:
                        await service.ProcessCreateUserRequests();
                        break;
                    case ConsoleKey.R:
                        await service.ProcessRevokeUserRequests();
                        break;
                    case ConsoleKey.G:
                        Console.WriteLine("Enter username:");
                        await service.GetUser(Console.ReadLine());
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
