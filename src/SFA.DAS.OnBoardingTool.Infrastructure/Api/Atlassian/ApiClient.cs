using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.OnBoardingTool.Domain;
using SFA.DAS.OnBoardingTool.Infrastructure.Configuration;
using SFA.DAS.OnBoardingTool.Infrastructure.Extensions;

namespace SFA.DAS.OnBoardingTool.Infrastructure.Api.Atlassian
{
    public class ApiClient : IApiClient
    {
        private readonly ILogger<ApiClient> _log;
        private readonly AtlassianApi _config;

        public ApiClient(ILogger<ApiClient> log, IOptions<AtlassianApi> config)
        {
            _log = log;
            _config = config.Value;
        }

        public async Task<string> GetUserRequests(string serviceDeskId, string queueId, IDictionary<string,string> queryParams = null) => 
            await Get(CreateUriBuilder($"{_config.BaseUrl}servicedeskapi/servicedesk/{serviceDeskId}/queue/{queueId}/issue", queryParams).Uri);

        public async Task<string> GetRequestTransitions(string issueId, IDictionary<string,string> queryParams = null) => 
            await Get(CreateUriBuilder($"{_config.BaseUrl}api/3/issue/{issueId}/transitions", queryParams).Uri);

        public async Task<string> GetUserGroups(IDictionary<string,string> queryParams = null) => 
            await Get(CreateUriBuilder($"{_config.BaseUrl}api/3/user/groups", queryParams).Uri);

        public async Task<string> GetUser(string accountId) => 
            await Get(CreateUriBuilder($"{_config.BaseUrl}api/3/user", new Dictionary<string, string>
            {
                {"accountId", accountId}
            }).Uri);

        public async Task<string> GetUserByUsername(string username) => 
            await Get(CreateUriBuilder($"{_config.BaseUrl}api/3/user/search",new Dictionary<string, string>
            {
                {"query", username}
            }).Uri);

        public async Task PerformTransition(string issueId, StringContent content, IDictionary<string,string> queryParams = null) => 
            await Post(CreateUriBuilder($"{_config.BaseUrl}api/3/issue/{issueId}/transitions", queryParams).Uri, content);

        public async Task CreateUser(User user) =>
            await Post(CreateUriBuilder($"{_config.BaseUrl}api/3/user").Uri, new StringContent(
                $"{{ \"notification\": \"true\", \"emailAddress\": \"{user.Email}\", \"displayName\": \"{user.Fullname}\", \"name\": \"{user.Fullname}\" }}",
                Encoding.UTF8,
                "application/json"
            ));

        public async Task DeleteUser(User user) => 
            await Delete(CreateUriBuilder($"{_config.BaseUrl}api/3/user", new Dictionary<string, string>
            {
                {"accountId", user.AccountId}
            }).Uri);

        private async Task<string> Get(Uri uri) =>
            await Try(async () => 
            {
                using var cli = CreateHttpClient();

                var resp = await cli.GetAsync(uri);

                resp.EnsureSuccessStatusCode();

                return await resp.Content.ReadAsStringAsync();
            });

        private async Task Post(Uri uri, StringContent content) => 
            await Try(async () => 
            {
                using var cli = CreateHttpClient();  

                var resp = await cli.PostAsync(uri, content);

                resp.EnsureSuccessStatusCode();

                return null;
            });

        private async Task Delete(Uri uri) => 
            await Try(async () => 
            {
                using var cli = CreateHttpClient();  

                var resp = await cli.DeleteAsync(uri);

                resp.EnsureSuccessStatusCode();

                return null;
            });

        private async Task<string> Try(Func<Task<string>> func)
        {
            try
            {
                return await func();
            }
            catch (HttpRequestException ex)
            {
                _log.LogCritical($"A non success status code was returned by the JIRA Cloud api: {ex.ToString()}");
                throw;
            }
        }

        private HttpClient AddDefaultHeaders(HttpClient cli) 
        {
            cli.DefaultRequestHeaders
                    .Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/json"));

            cli.DefaultRequestHeaders
                    .Authorization = new AuthenticationHeaderValue("Basic", GetCredentials());

            return cli;
        }

        private HttpClient CreateHttpClient() => AddDefaultHeaders(new HttpClient());

        private string GetCredentials() => Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(_config.Credentials));

        private static UriBuilder CreateUriBuilder(string uriString, IDictionary<string, string> queryParams = null) => new UriBuilder(uriString).BuildQueryString(queryParams);
    }
}
