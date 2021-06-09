using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.OnBoardingTool.Application;

namespace SFA.DAS.OnBoardingTool.Functions
{
    public class CreateAtlassianUsers
    {
        private readonly IAtlassianService _atlassianService;

        public CreateAtlassianUsers(IAtlassianService atlassianService)
        {
            _atlassianService = atlassianService;
        }

        [FunctionName("CreateAtlassianUsers")]
        public async Task Run([TimerTrigger("%CreateUserTimerSchedule%")]TimerInfo timer, ILogger log)
        {
            log.LogInformation($"Create Users C# Timer trigger function executed at: {DateTime.Now}");

            await _atlassianService.ProcessCreateUserRequests();
        }
    }
}
