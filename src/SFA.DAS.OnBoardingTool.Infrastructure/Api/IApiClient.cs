using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SFA.DAS.OnBoardingTool.Domain;
using SFA.DAS.OnBoardingTool.Infrastructure.Api.Atlassian;

namespace SFA.DAS.OnBoardingTool.Infrastructure.Api
{
    public interface IApiClient
    {
        Task<string> GetUserRequests(string serviceDeskId, string queueId, IDictionary<string,string> queryParams = null);
        Task CreateUser(User user);
        Task<string> GetUser(string accountId);
        Task<string> GetUserByUsername(string username);
        Task DeleteUser(User user);
        Task<string> GetRequestTransitions(string issueId, IDictionary<string,string> queryParams = null);
        Task PerformTransition(string issueId, StringContent content, IDictionary<string,string> queryParams = null);
        Task<string> GetUserGroups(IDictionary<string,string> queryParams = null);
    }
}