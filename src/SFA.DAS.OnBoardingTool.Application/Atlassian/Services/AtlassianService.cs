using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.OnBoardingTool.Domain;
using SFA.DAS.OnBoardingTool.Infrastructure.Api;
using System.Text.Json;
using SFA.DAS.OnBoardingTool.Infrastructure.Api.Atlassian;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using SFA.DAS.OnBoardingTool.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using FluentValidation;
using SFA.DAS.OnBoardingTool.Infrastructure.Validators;

namespace SFA.DAS.OnBoardingTool.Application.Atlassian
{
    public class AtlassianService : IAtlassianService
    {
        private readonly IApiClient _apiClient;
        private readonly ILogger<AtlassianService> _log;
        private readonly OnBoardingToolFunctions _config;
        private readonly AbstractValidator<Value> _validator;
        
        public AtlassianService(
            IApiClient apiClient,
            ILogger<AtlassianService> log,
            IOptions<OnBoardingToolFunctions> config,
            AbstractValidator<Value> validator)
        {
            _config = config.Value;
            _apiClient = apiClient;
            _log = log;         
            _validator = validator;
        }

        public async Task ProcessCreateUserRequests()
        {
            _log.LogInformation(_config.ToString());
            _log.LogInformation($"Processing create user requests");

            var requests = await GetUserRequests();

            if(requests.Size == 0)
            {
                _log.LogInformation($"No create user requests to process");
                return;
            }

            var tasks = new List<Task>();

            foreach (var request in requests.Values
                .AsEnumerable()
                .Where(r => r.Fields.Customfield_12105.Value.Equals("grant", StringComparison.InvariantCultureIgnoreCase)))
            {                
                var email = request.Fields.Customfield_12104;

                _log.LogInformation($"Processing request for email {email} from reporter {request.Fields.Reporter.DisplayName}");

                var result = _validator.Validate(request, opts => opts.IncludeRuleSets(ValueValidator.CanCreate));
                if(!result.IsValid)
                {
                    _log.LogInformation($"Request failed validation {string.Join(',', result.Errors.Select(err => err.ErrorMessage).ToArray())}");
                    continue;
                }
                    
                tasks.Add(CreateUser(request, new User { Username = email,  Email = email })
                .ContinueWith(TransitionRequest, TaskContinuationOptions.OnlyOnRanToCompletion));
            }

            Task.WaitAll(tasks.ToArray());

            _log.LogInformation($"Finished processing create requests");
        }

        public async Task ProcessRevokeUserRequests()
        {
            _log.LogInformation(_config.ToString());
            _log.LogInformation($"Processing revoke user requests");

            var requests = await GetUserRequests();

            if(requests.Size == 0)
            {
                _log.LogInformation($"No revoke user requests to process");
                return;
            }

            var tasks = new List<Task>();

            foreach (var request in requests.Values
                .AsEnumerable()
                .Where(r => r.Fields.Customfield_12105.Value.Equals("revoke", StringComparison.InvariantCultureIgnoreCase)))
            {                
                var email = request.Fields.Customfield_12104;

                _log.LogInformation($"Processing request for email {email} from reporter {request.Fields.Reporter.DisplayName}");

                var result = _validator.Validate(request, opts => opts.IncludeRuleSets(ValueValidator.CanRevoke));
                if(!result.IsValid)
                {
                    _log.LogInformation($"Request failed validation {string.Join(',', result.Errors.Select(err => err.ErrorMessage).ToArray())}");
                    continue;
                }
                    
                var user = await GetUser(email);

                if(string.IsNullOrEmpty(user.AccountId))
                {
                    _log.LogInformation($"Could not find user with username {email}");
                    continue;
                }

                tasks.Add(RevokeUser(request, new User { AccountId = user.AccountId, Username = email, Email = email })
                .ContinueWith(TransitionRequest, TaskContinuationOptions.OnlyOnRanToCompletion));
            }

            Task.WaitAll(tasks.ToArray());

            _log.LogInformation($"Finished revoke processing requests");
        }        

        public async Task<UserDTO> GetUser(string username)
        {
            try
            {
                var result = await _apiClient.GetUserByUsername(username);

                var array = JsonSerializer.Deserialize<UserDTO[]>(result, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                    });

                return array.FirstOrDefault();
                }
            catch(Exception ex)
            {
                _log.LogError(ex, $"Could not find user: {username}");
            }

            return new UserDTO();
        }

        public async Task<(string, bool)> CreateUser(Value request, User user)
        {
            try
            {
                await _apiClient.CreateUser(user);
            }
            catch (System.Exception)
            {
                return (request.Key, false);
            }

            return (request.Key, true);
        }

        public async Task<QueueItems> GetUserRequests()
        {
            _log.LogInformation($"Retrieving {_config.ServiceDeskId}");
            _log.LogInformation($"Retrieving {_config.QueueId}");

            Debug.Assert(!string.IsNullOrEmpty(_config.ServiceDeskId) && !string.IsNullOrEmpty(_config.QueueId));

            var result = await _apiClient.GetUserRequests(_config.ServiceDeskId, _config.QueueId);

            try
            {
                var x = JsonSerializer.Deserialize<QueueItems>(result, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return x;
            }
            catch (System.Exception ex)
            {
                _log.LogCritical($"Could not get user requests, failed with exception: {ex.ToString()}");
                throw;
            }            
        }    

        private void TransitionRequest(Task<(string, bool)> task)
        {
            if(task.Result.Item2)
            {
                _log.LogInformation($"Performing request transition to triaging");

                _apiClient.PerformTransition(task.Result.Item1, GetTriagingStringContent()).Wait();
                    
                _log.LogInformation($"Performing request transition to resolved");

                _apiClient.PerformTransition(task.Result.Item1, GetResolvedStringContent()).Wait();
            }
        }

        private async Task<(string, bool)> RevokeUser(Value request, User user)
        {
            try
            {                
                await _apiClient.DeleteUser(user);
            }
            catch (System.Exception)
            {
                return (request.Key, false);
            }

            return (request.Key, true);
        }

        private StringContent GetTriagingStringContent() => new StringContent(
                                    $"{{ \"transition\": {{ \"id\": \"{_config.TriageStatusId}\" }} }}",
                                    System.Text.Encoding.UTF8,
                                    "application/json"
                                );

        private StringContent GetResolvedStringContent() => new StringContent(
                                    $"{{ \"transition\": {{ \"id\": \"{_config.ResolvedStatusId}\" }}, \"fields\": {{ \"resolution\": {{ \"name\": \"Done\" }} }} }}",
                                    System.Text.Encoding.UTF8,
                                    "application/json"
                                );
    }
}
