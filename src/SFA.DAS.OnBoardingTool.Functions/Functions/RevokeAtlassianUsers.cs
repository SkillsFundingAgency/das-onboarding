using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.OnBoardingTool.Application;

namespace SFA.DAS.OnBoardingTool.Functions
{
    public class RevokeAtlassianUsers
    {
        private readonly IAtlassianService _atlassianService;

        public RevokeAtlassianUsers(IAtlassianService atlassianService)
        {
            _atlassianService = atlassianService;
        }

        [FunctionName("RevokeAtlassianUsers")]
        public async Task Run([TimerTrigger("%RevokeUserTimerSchedule%")]TimerInfo timer, ILogger log)
        {
            log.LogInformation($"Revoke Users C# Timer trigger function executed at: {DateTime.Now}");

            await _atlassianService.ProcessRevokeUserRequests();
        }
    }
}
