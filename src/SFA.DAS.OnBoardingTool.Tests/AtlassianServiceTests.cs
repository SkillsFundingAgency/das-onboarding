using System.Threading.Tasks;
using NUnit.Framework;
using SFA.DAS.OnBoardingTool.Application.Atlassian;
using SFA.DAS.OnBoardingTool.Infrastructure.Api.Atlassian;
using Moq;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using SFA.DAS.OnBoardingTool.Infrastructure.Configuration;
using SFA.DAS.OnBoardingTool.Infrastructure.Api;
using SFA.DAS.OnBoardingTool.Domain;
using System.Net.Http;
using System;
using FluentValidation;
using SFA.DAS.OnBoardingTool.Infrastructure.Validators;

namespace SFA.DAS.OnBoardingTool.Tests
{
    public class AtlassianServiceTests
    {
        private Mock<ILogger<AtlassianService>> _log = new Mock<ILogger<AtlassianService>>();
        private Mock<ILogger<ApiClient>> _apiClientLog = new Mock<ILogger<ApiClient>>();
        private Mock<IOptions<OnBoardingToolFunctions>> _config = new Mock<IOptions<OnBoardingToolFunctions>>();
        private Mock<IApiClient> _apiClient = new Mock<IApiClient>();
        private AbstractValidator<Value> _validator; 

        [SetUp]
        public void Setup()
        {
            _config.SetupGet(x => x.Value).Returns(new OnBoardingToolFunctions
            {
                ServiceDeskId = "TST",
                QueueId = "1",
                PreAuthorisedEmailDomain = "test.com",
                PreApprovedRequesters = new List<string>() { "test user" },
                TriageStatusId = "1",
                ResolvedStatusId = "2"
            });

            _apiClient = new Mock<IApiClient>();
            _validator = new ValueValidator(_config.Object);
        }

        [Test]
        public async Task NoUserRequests_CorrectlyReturnsWithNoFurtherProcessing()
        {
            _apiClient.Setup(x => x.GetUserRequests("TST", "1", null)).ReturnsAsync("{\"size\":0,\"start\":0,\"limit\":50,\"isLastPage\":true,\"_links\":{\"self\":\"\",\"base\":\"\",\"context\":\"\"},\"values\":[]}");
            _apiClient.Setup(x => x.CreateUser(It.IsAny<User>()));
            _apiClient.Setup(x => x.PerformTransition("1", It.IsAny<StringContent>(), null));

           var service = new AtlassianService(_apiClient.Object, _log.Object, _config.Object, _validator);

           await service.ProcessCreateUserRequests();

           _apiClient.Verify(x => x.GetUserRequests("TST", "1", null), Times.Once);
           _apiClient.Verify(x => x.CreateUser(It.IsAny<User>()), Times.Never);
           _apiClient.Verify(x => x.PerformTransition(It.IsAny<string>(), It.IsAny<StringContent>(), null), Times.Never);
           
        }

        [Test]
        public async Task OneUserRequest_ProcessedCorrectly()
        {
            _apiClient.Setup(x => x.GetUserRequests("TST", "1", null)).ReturnsAsync("{\"size\":1,\"start\":0,\"limit\":50,\"isLastPage\":true,\"_links\":{\"self\":\"\",\"base\":\"\",\"context\":\"\"},\"values\":[{\"id\":\"125243\",\"self\":\"\",\"key\":\"TST-1\",\"fields\":{\"summary\":\"Add / Remove user (Jira/Confluence)\",\"created\":\"2021-05-24T16:04:16.880+0100\",\"duedate\":\"\",\"reporter\":{\"self\":\"https://skillsfundingagency.atlassian.net/rest/api/2/user?accountId=5c73cd628adc243ea060ebe9\",\"accountId\":\"5c73cd628adc243ea060ebe9\",\"emailAddress\":\"x.y@education.gov.uk\",\"avatarUrls\":{\"48x48\":\"\",\"24x24\":\"\",\"16x16\":\"\",\"32x32\":\"\"},\"displayName\":\"T User\",\"active\":true,\"timeZone\":\"Europe/London\",\"accountType\":\"atlassian\"},\"issuelinks\":[],\"customfield_11100\":{\"_links\":{\"jiraRest\":\"\",\"web\":\"\",\"self\":\"\",\"agent\":\"\"},\"requestType\":{\"_expands\":[\"field\"],\"id\":\"14\",\"_links\":{\"self\":\"\"},\"name\":\"Add / Remove Jira/Confluence access\",\"description\":\"Request a new account for a new starter, revoke account for a leaver or a reactivation for a user who's account was deactivated due to lack of use\",\"helpText\":\"BEFORE raising this request, please ensure you've read [THIS|https://skillsfundingagency.atlassian.net/wiki/spaces/DM/pages/106966530/How+to+Request+access+to+Jira+Confluence] page on user account requests.Thanks\",\"issueTypeId\":\"10501\",\"serviceDeskId\":\"1\",\"groupIds\":[\"1\"],\"icon\":{\"id\":\"16227\",\"_links\":{\"iconUrls\":{\"48x48\":\"\",\"24x24\":\"\",\"16x16\":\"\",\"32x32\":\"\"}}}},\"currentStatus\":{\"status\":\"In the queue\",\"statusCategory\":\"NEW\",\"statusDate\":{\"iso8601\":\"2021-05-24T16:04:16+0100\",\"jira\":\"2021-05-24T16:04:16.880+0100\",\"friendly\":\"Monday 4:04 PM\",\"epochMillis\":1621868656880}}},\"customfield_12056\":{\"self\":\"\",\"value\":\"Apprenticeship Service\",\"id\":\"1234\"},\"updated\":\"2021-05-24T16:04:18.001+0100\",\"customfield_12105\":{\"self\":\"\",\"value\":\"Grant\",\"id\":\"1234\"},\"status\":{\"self\":\"\",\"description\":\"\",\"iconUrl\":\"\",\"name\":\"In the queue\",\"id\":\"13000\",\"statusCategory\":{\"self\":\"\",\"id\":2,\"key\":\"new\",\"colorName\":\"blue-gray\",\"name\":\"To Do\"}},\"customfield_12104\":\"t.u@test.com\"}}]}");
            _apiClient.Setup(x => x.CreateUser(It.IsAny<User>()));
            _apiClient.Setup(x => x.PerformTransition("1", It.IsAny<StringContent>(), null));

           var service = new AtlassianService(_apiClient.Object, _log.Object, _config.Object, _validator);

           await service.ProcessCreateUserRequests();

           _apiClient.Verify(x => x.GetUserRequests("TST", "1", null), Times.Once);
           _apiClient.Verify(x => x.CreateUser(It.IsAny<User>()), Times.Once);
           _apiClient.Verify(x => x.PerformTransition("TST-1", It.IsAny<StringContent>(), null), Times.Exactly(2));
           
        }

        [Test]
        public async Task WhenDueDateIsInTheFuture_UserIsNotCreated()
        {
            _apiClient.Setup(x => x.GetUserRequests("TST", "1", null)).ReturnsAsync("{\"size\":1,\"start\":0,\"limit\":50,\"isLastPage\":true,\"_links\":{\"self\":\"\",\"base\":\"\",\"context\":\"\"},\"values\":[{\"id\":\"125243\",\"self\":\"\",\"key\":\"TST-1\",\"fields\":{\"summary\":\"Add / Remove user (Jira/Confluence)\",\"created\":\"2021-05-24T16:04:16.880+0100\",\"duedate\":\"" + DateTime.Now.AddDays(1).ToString("yyyy-MM-dd") + "\",\"reporter\":{\"self\":\"https://skillsfundingagency.atlassian.net/rest/api/2/user?accountId=5c73cd628adc243ea060ebe9\",\"accountId\":\"5c73cd628adc243ea060ebe9\",\"emailAddress\":\"x.y@education.gov.uk\",\"avatarUrls\":{\"48x48\":\"\",\"24x24\":\"\",\"16x16\":\"\",\"32x32\":\"\"},\"displayName\":\"T User\",\"active\":true,\"timeZone\":\"Europe/London\",\"accountType\":\"atlassian\"},\"issuelinks\":[],\"customfield_11100\":{\"_links\":{\"jiraRest\":\"\",\"web\":\"\",\"self\":\"\",\"agent\":\"\"},\"requestType\":{\"_expands\":[\"field\"],\"id\":\"14\",\"_links\":{\"self\":\"\"},\"name\":\"Add / Remove Jira/Confluence access\",\"description\":\"Request a new account for a new starter, revoke account for a leaver or a reactivation for a user who's account was deactivated due to lack of use\",\"helpText\":\"BEFORE raising this request, please ensure you've read [THIS|https://skillsfundingagency.atlassian.net/wiki/spaces/DM/pages/106966530/How+to+Request+access+to+Jira+Confluence] page on user account requests.Thanks\",\"issueTypeId\":\"10501\",\"serviceDeskId\":\"1\",\"groupIds\":[\"1\"],\"icon\":{\"id\":\"16227\",\"_links\":{\"iconUrls\":{\"48x48\":\"\",\"24x24\":\"\",\"16x16\":\"\",\"32x32\":\"\"}}}},\"currentStatus\":{\"status\":\"In the queue\",\"statusCategory\":\"NEW\",\"statusDate\":{\"iso8601\":\"2021-05-24T16:04:16+0100\",\"jira\":\"2021-05-24T16:04:16.880+0100\",\"friendly\":\"Monday 4:04 PM\",\"epochMillis\":1621868656880}}},\"customfield_12056\":{\"self\":\"\",\"value\":\"Apprenticeship Service\",\"id\":\"1234\"},\"updated\":\"2021-05-24T16:04:18.001+0100\",\"customfield_12105\":{\"self\":\"\",\"value\":\"Grant\",\"id\":\"1234\"},\"status\":{\"self\":\"\",\"description\":\"\",\"iconUrl\":\"\",\"name\":\"In the queue\",\"id\":\"13000\",\"statusCategory\":{\"self\":\"\",\"id\":2,\"key\":\"new\",\"colorName\":\"blue-gray\",\"name\":\"To Do\"}},\"customfield_12104\":\"t.u@test.com\"}}]}");
            _apiClient.Setup(x => x.CreateUser(It.IsAny<User>()));
            _apiClient.Setup(x => x.PerformTransition("1", It.IsAny<StringContent>(), null));

           var service = new AtlassianService(_apiClient.Object, _log.Object, _config.Object, _validator);

           await service.ProcessCreateUserRequests();

           _apiClient.Verify(x => x.GetUserRequests("TST", "1", null), Times.Once);
           _apiClient.Verify(x => x.CreateUser(It.IsAny<User>()), Times.Never);
           _apiClient.Verify(x => x.PerformTransition("TST-1", It.IsAny<StringContent>(), null), Times.Never);
        }

        [Test]
        public async Task WhenDueDateIsToday_UserIsCreated()
        {
            _apiClient.Setup(x => x.GetUserRequests("TST", "1", null)).ReturnsAsync("{\"size\":1,\"start\":0,\"limit\":50,\"isLastPage\":true,\"_links\":{\"self\":\"\",\"base\":\"\",\"context\":\"\"},\"values\":[{\"id\":\"125243\",\"self\":\"\",\"key\":\"TST-1\",\"fields\":{\"summary\":\"Add / Remove user (Jira/Confluence)\",\"created\":\"2021-05-24T16:04:16.880+0100\",\"duedate\":\"" + DateTime.Now.ToString("yyyy-MM-dd") + "\",\"reporter\":{\"self\":\"https://skillsfundingagency.atlassian.net/rest/api/2/user?accountId=5c73cd628adc243ea060ebe9\",\"accountId\":\"5c73cd628adc243ea060ebe9\",\"emailAddress\":\"x.y@education.gov.uk\",\"avatarUrls\":{\"48x48\":\"\",\"24x24\":\"\",\"16x16\":\"\",\"32x32\":\"\"},\"displayName\":\"T User\",\"active\":true,\"timeZone\":\"Europe/London\",\"accountType\":\"atlassian\"},\"issuelinks\":[],\"customfield_11100\":{\"_links\":{\"jiraRest\":\"\",\"web\":\"\",\"self\":\"\",\"agent\":\"\"},\"requestType\":{\"_expands\":[\"field\"],\"id\":\"14\",\"_links\":{\"self\":\"\"},\"name\":\"Add / Remove Jira/Confluence access\",\"description\":\"Request a new account for a new starter, revoke account for a leaver or a reactivation for a user who's account was deactivated due to lack of use\",\"helpText\":\"BEFORE raising this request, please ensure you've read [THIS|https://skillsfundingagency.atlassian.net/wiki/spaces/DM/pages/106966530/How+to+Request+access+to+Jira+Confluence] page on user account requests.Thanks\",\"issueTypeId\":\"10501\",\"serviceDeskId\":\"1\",\"groupIds\":[\"1\"],\"icon\":{\"id\":\"16227\",\"_links\":{\"iconUrls\":{\"48x48\":\"\",\"24x24\":\"\",\"16x16\":\"\",\"32x32\":\"\"}}}},\"currentStatus\":{\"status\":\"In the queue\",\"statusCategory\":\"NEW\",\"statusDate\":{\"iso8601\":\"2021-05-24T16:04:16+0100\",\"jira\":\"2021-05-24T16:04:16.880+0100\",\"friendly\":\"Monday 4:04 PM\",\"epochMillis\":1621868656880}}},\"customfield_12056\":{\"self\":\"\",\"value\":\"Apprenticeship Service\",\"id\":\"1234\"},\"updated\":\"2021-05-24T16:04:18.001+0100\",\"customfield_12105\":{\"self\":\"\",\"value\":\"Grant\",\"id\":\"1234\"},\"status\":{\"self\":\"\",\"description\":\"\",\"iconUrl\":\"\",\"name\":\"In the queue\",\"id\":\"13000\",\"statusCategory\":{\"self\":\"\",\"id\":2,\"key\":\"new\",\"colorName\":\"blue-gray\",\"name\":\"To Do\"}},\"customfield_12104\":\"t.u@test.com\"}}]}");
            _apiClient.Setup(x => x.CreateUser(It.IsAny<User>()));
            _apiClient.Setup(x => x.PerformTransition("1", It.IsAny<StringContent>(), null));

           var service = new AtlassianService(_apiClient.Object, _log.Object, _config.Object, _validator);

           await service.ProcessCreateUserRequests();

           _apiClient.Verify(x => x.GetUserRequests("TST", "1", null), Times.Once);
           _apiClient.Verify(x => x.CreateUser(It.IsAny<User>()), Times.Once);
           _apiClient.Verify(x => x.PerformTransition("TST-1", It.IsAny<StringContent>(), null), Times.Exactly(2));
        }

        [Test]
        public async Task WhenReporterIsPreApproved_UserIsCreated()
        {
            _apiClient.Setup(x => x.GetUserRequests("TST", "1", null)).ReturnsAsync("{\"size\":1,\"start\":0,\"limit\":50,\"isLastPage\":true,\"_links\":{\"self\":\"\",\"base\":\"\",\"context\":\"\"},\"values\":[{\"id\":\"125243\",\"self\":\"\",\"key\":\"TST-1\",\"fields\":{\"summary\":\"Add / Remove user (Jira/Confluence)\",\"created\":\"2021-05-24T16:04:16.880+0100\",\"duedate\":\"\",\"reporter\":{\"self\":\"https://skillsfundingagency.atlassian.net/rest/api/2/user?accountId=5c73cd628adc243ea060ebe9\",\"accountId\":\"5c73cd628adc243ea060ebe9\",\"emailAddress\":\"x.y@education.gov.uk\",\"avatarUrls\":{\"48x48\":\"\",\"24x24\":\"\",\"16x16\":\"\",\"32x32\":\"\"},\"displayName\":\"Test User\",\"active\":true,\"timeZone\":\"Europe/London\",\"accountType\":\"atlassian\"},\"issuelinks\":[],\"customfield_11100\":{\"_links\":{\"jiraRest\":\"\",\"web\":\"\",\"self\":\"\",\"agent\":\"\"},\"requestType\":{\"_expands\":[\"field\"],\"id\":\"14\",\"_links\":{\"self\":\"\"},\"name\":\"Add / Remove Jira/Confluence access\",\"description\":\"Request a new account for a new starter, revoke account for a leaver or a reactivation for a user who's account was deactivated due to lack of use\",\"helpText\":\"BEFORE raising this request, please ensure you've read [THIS|https://skillsfundingagency.atlassian.net/wiki/spaces/DM/pages/106966530/How+to+Request+access+to+Jira+Confluence] page on user account requests.Thanks\",\"issueTypeId\":\"10501\",\"serviceDeskId\":\"1\",\"groupIds\":[\"1\"],\"icon\":{\"id\":\"16227\",\"_links\":{\"iconUrls\":{\"48x48\":\"\",\"24x24\":\"\",\"16x16\":\"\",\"32x32\":\"\"}}}},\"currentStatus\":{\"status\":\"In the queue\",\"statusCategory\":\"NEW\",\"statusDate\":{\"iso8601\":\"2021-05-24T16:04:16+0100\",\"jira\":\"2021-05-24T16:04:16.880+0100\",\"friendly\":\"Monday 4:04 PM\",\"epochMillis\":1621868656880}}},\"customfield_12056\":{\"self\":\"\",\"value\":\"Apprenticeship Service\",\"id\":\"1234\"},\"updated\":\"2021-05-24T16:04:18.001+0100\",\"customfield_12105\":{\"self\":\"\",\"value\":\"Grant\",\"id\":\"1234\"},\"status\":{\"self\":\"\",\"description\":\"\",\"iconUrl\":\"\",\"name\":\"In the queue\",\"id\":\"13000\",\"statusCategory\":{\"self\":\"\",\"id\":2,\"key\":\"new\",\"colorName\":\"blue-gray\",\"name\":\"To Do\"}},\"customfield_12104\":\"t.u@nopreauthorised.com\"}}]}");
            _apiClient.Setup(x => x.CreateUser(It.IsAny<User>()));
            _apiClient.Setup(x => x.PerformTransition("1", It.IsAny<StringContent>(), null));

           var service = new AtlassianService(_apiClient.Object, _log.Object, _config.Object, _validator);

           await service.ProcessCreateUserRequests();

           _apiClient.Verify(x => x.GetUserRequests("TST", "1", null), Times.Once);
           _apiClient.Verify(x => x.CreateUser(It.IsAny<User>()), Times.Once);
           _apiClient.Verify(x => x.PerformTransition("TST-1", It.IsAny<StringContent>(), null), Times.Exactly(2));
        }

        [Test]
        public async Task WhenEmailAddresIsPreAuthed_UserIsCreated()
        {
            _apiClient.Setup(x => x.GetUserRequests("TST", "1", null)).ReturnsAsync("{\"size\":1,\"start\":0,\"limit\":50,\"isLastPage\":true,\"_links\":{\"self\":\"\",\"base\":\"\",\"context\":\"\"},\"values\":[{\"id\":\"125243\",\"self\":\"\",\"key\":\"TST-1\",\"fields\":{\"summary\":\"Add / Remove user (Jira/Confluence)\",\"created\":\"2021-05-24T16:04:16.880+0100\",\"duedate\":\"\",\"reporter\":{\"self\":\"https://skillsfundingagency.atlassian.net/rest/api/2/user?accountId=5c73cd628adc243ea060ebe9\",\"accountId\":\"5c73cd628adc243ea060ebe9\",\"emailAddress\":\"x.y@education.gov.uk\",\"avatarUrls\":{\"48x48\":\"\",\"24x24\":\"\",\"16x16\":\"\",\"32x32\":\"\"},\"displayName\":\"T User\",\"active\":true,\"timeZone\":\"Europe/London\",\"accountType\":\"atlassian\"},\"issuelinks\":[],\"customfield_11100\":{\"_links\":{\"jiraRest\":\"\",\"web\":\"\",\"self\":\"\",\"agent\":\"\"},\"requestType\":{\"_expands\":[\"field\"],\"id\":\"14\",\"_links\":{\"self\":\"\"},\"name\":\"Add / Remove Jira/Confluence access\",\"description\":\"Request a new account for a new starter, revoke account for a leaver or a reactivation for a user who's account was deactivated due to lack of use\",\"helpText\":\"BEFORE raising this request, please ensure you've read [THIS|https://skillsfundingagency.atlassian.net/wiki/spaces/DM/pages/106966530/How+to+Request+access+to+Jira+Confluence] page on user account requests.Thanks\",\"issueTypeId\":\"10501\",\"serviceDeskId\":\"1\",\"groupIds\":[\"1\"],\"icon\":{\"id\":\"16227\",\"_links\":{\"iconUrls\":{\"48x48\":\"\",\"24x24\":\"\",\"16x16\":\"\",\"32x32\":\"\"}}}},\"currentStatus\":{\"status\":\"In the queue\",\"statusCategory\":\"NEW\",\"statusDate\":{\"iso8601\":\"2021-05-24T16:04:16+0100\",\"jira\":\"2021-05-24T16:04:16.880+0100\",\"friendly\":\"Monday 4:04 PM\",\"epochMillis\":1621868656880}}},\"customfield_12056\":{\"self\":\"\",\"value\":\"Apprenticeship Service\",\"id\":\"1234\"},\"updated\":\"2021-05-24T16:04:18.001+0100\",\"customfield_12105\":{\"self\":\"\",\"value\":\"Grant\",\"id\":\"1234\"},\"status\":{\"self\":\"\",\"description\":\"\",\"iconUrl\":\"\",\"name\":\"In the queue\",\"id\":\"13000\",\"statusCategory\":{\"self\":\"\",\"id\":2,\"key\":\"new\",\"colorName\":\"blue-gray\",\"name\":\"To Do\"}},\"customfield_12104\":\"t.u@test.com\"}}]}");
            _apiClient.Setup(x => x.CreateUser(It.IsAny<User>()));
            _apiClient.Setup(x => x.PerformTransition("1", It.IsAny<StringContent>(), null));

           var service = new AtlassianService(_apiClient.Object, _log.Object, _config.Object, _validator);

           await service.ProcessCreateUserRequests();

           _apiClient.Verify(x => x.GetUserRequests("TST", "1", null), Times.Once);
           _apiClient.Verify(x => x.CreateUser(It.IsAny<User>()), Times.Once);
           _apiClient.Verify(x => x.PerformTransition("TST-1", It.IsAny<StringContent>(), null), Times.Exactly(2));
        }

        [Test]
        public async Task WhenNeitherEmailAddresIsPreAuthedAndTheReporterPreApproved_UserIsNotCreated()
        {
            _apiClient.Setup(x => x.GetUserRequests("TST", "1", null)).ReturnsAsync("{\"size\":1,\"start\":0,\"limit\":50,\"isLastPage\":true,\"_links\":{\"self\":\"\",\"base\":\"\",\"context\":\"\"},\"values\":[{\"id\":\"125243\",\"self\":\"\",\"key\":\"TST-1\",\"fields\":{\"summary\":\"Add / Remove user (Jira/Confluence)\",\"created\":\"2021-05-24T16:04:16.880+0100\",\"duedate\":\"\",\"reporter\":{\"self\":\"https://skillsfundingagency.atlassian.net/rest/api/2/user?accountId=5c73cd628adc243ea060ebe9\",\"accountId\":\"5c73cd628adc243ea060ebe9\",\"emailAddress\":\"x.y@education.gov.uk\",\"avatarUrls\":{\"48x48\":\"\",\"24x24\":\"\",\"16x16\":\"\",\"32x32\":\"\"},\"displayName\":\"Not preapproved\",\"active\":true,\"timeZone\":\"Europe/London\",\"accountType\":\"atlassian\"},\"issuelinks\":[],\"customfield_11100\":{\"_links\":{\"jiraRest\":\"\",\"web\":\"\",\"self\":\"\",\"agent\":\"\"},\"requestType\":{\"_expands\":[\"field\"],\"id\":\"14\",\"_links\":{\"self\":\"\"},\"name\":\"Add / Remove Jira/Confluence access\",\"description\":\"Request a new account for a new starter, revoke account for a leaver or a reactivation for a user who's account was deactivated due to lack of use\",\"helpText\":\"BEFORE raising this request, please ensure you've read [THIS|https://skillsfundingagency.atlassian.net/wiki/spaces/DM/pages/106966530/How+to+Request+access+to+Jira+Confluence] page on user account requests.Thanks\",\"issueTypeId\":\"10501\",\"serviceDeskId\":\"1\",\"groupIds\":[\"1\"],\"icon\":{\"id\":\"16227\",\"_links\":{\"iconUrls\":{\"48x48\":\"\",\"24x24\":\"\",\"16x16\":\"\",\"32x32\":\"\"}}}},\"currentStatus\":{\"status\":\"In the queue\",\"statusCategory\":\"NEW\",\"statusDate\":{\"iso8601\":\"2021-05-24T16:04:16+0100\",\"jira\":\"2021-05-24T16:04:16.880+0100\",\"friendly\":\"Monday 4:04 PM\",\"epochMillis\":1621868656880}}},\"customfield_12056\":{\"self\":\"\",\"value\":\"Apprenticeship Service\",\"id\":\"1234\"},\"updated\":\"2021-05-24T16:04:18.001+0100\",\"customfield_12105\":{\"self\":\"\",\"value\":\"Grant\",\"id\":\"1234\"},\"status\":{\"self\":\"\",\"description\":\"\",\"iconUrl\":\"\",\"name\":\"In the queue\",\"id\":\"13000\",\"statusCategory\":{\"self\":\"\",\"id\":2,\"key\":\"new\",\"colorName\":\"blue-gray\",\"name\":\"To Do\"}},\"customfield_12104\":\"t.u@notpreauthorised.com\"}}]}");
            _apiClient.Setup(x => x.CreateUser(It.IsAny<User>()));
            _apiClient.Setup(x => x.PerformTransition("1", It.IsAny<StringContent>(), null));

           var service = new AtlassianService(_apiClient.Object, _log.Object, _config.Object, _validator);

           await service.ProcessCreateUserRequests();

           _apiClient.Verify(x => x.GetUserRequests("TST", "1", null), Times.Once);
           _apiClient.Verify(x => x.CreateUser(It.IsAny<User>()), Times.Never);
           _apiClient.Verify(x => x.PerformTransition("TST-1", It.IsAny<StringContent>(), null), Times.Never);
        }

//

        [Test]
        public async Task WhenDueDateIsInTheFuture_UserIsNotRevoked()
        {
            _apiClient.Setup(x => x.GetUserRequests("TST", "1", null)).ReturnsAsync("{\"size\":1,\"start\":0,\"limit\":50,\"isLastPage\":true,\"_links\":{\"self\":\"\",\"base\":\"\",\"context\":\"\"},\"values\":[{\"id\":\"125243\",\"self\":\"\",\"key\":\"TST-1\",\"fields\":{\"summary\":\"Add / Remove user (Jira/Confluence)\",\"created\":\"2021-05-24T16:04:16.880+0100\",\"duedate\":\"" + DateTime.Now.AddDays(1).ToString("yyyy-MM-dd") + "\",\"reporter\":{\"self\":\"https://skillsfundingagency.atlassian.net/rest/api/2/user?accountId=5c73cd628adc243ea060ebe9\",\"accountId\":\"5c73cd628adc243ea060ebe9\",\"emailAddress\":\"x.y@education.gov.uk\",\"avatarUrls\":{\"48x48\":\"\",\"24x24\":\"\",\"16x16\":\"\",\"32x32\":\"\"},\"displayName\":\"T User\",\"active\":true,\"timeZone\":\"Europe/London\",\"accountType\":\"atlassian\"},\"issuelinks\":[],\"customfield_11100\":{\"_links\":{\"jiraRest\":\"\",\"web\":\"\",\"self\":\"\",\"agent\":\"\"},\"requestType\":{\"_expands\":[\"field\"],\"id\":\"14\",\"_links\":{\"self\":\"\"},\"name\":\"Add / Remove Jira/Confluence access\",\"description\":\"Request a new account for a new starter, revoke account for a leaver or a reactivation for a user who's account was deactivated due to lack of use\",\"helpText\":\"BEFORE raising this request, please ensure you've read [THIS|https://skillsfundingagency.atlassian.net/wiki/spaces/DM/pages/106966530/How+to+Request+access+to+Jira+Confluence] page on user account requests.Thanks\",\"issueTypeId\":\"10501\",\"serviceDeskId\":\"1\",\"groupIds\":[\"1\"],\"icon\":{\"id\":\"16227\",\"_links\":{\"iconUrls\":{\"48x48\":\"\",\"24x24\":\"\",\"16x16\":\"\",\"32x32\":\"\"}}}},\"currentStatus\":{\"status\":\"In the queue\",\"statusCategory\":\"NEW\",\"statusDate\":{\"iso8601\":\"2021-05-24T16:04:16+0100\",\"jira\":\"2021-05-24T16:04:16.880+0100\",\"friendly\":\"Monday 4:04 PM\",\"epochMillis\":1621868656880}}},\"customfield_12056\":{\"self\":\"\",\"value\":\"Apprenticeship Service\",\"id\":\"1234\"},\"updated\":\"2021-05-24T16:04:18.001+0100\",\"customfield_12105\":{\"self\":\"\",\"value\":\"Revoke\",\"id\":\"1234\"},\"status\":{\"self\":\"\",\"description\":\"\",\"iconUrl\":\"\",\"name\":\"In the queue\",\"id\":\"13000\",\"statusCategory\":{\"self\":\"\",\"id\":2,\"key\":\"new\",\"colorName\":\"blue-gray\",\"name\":\"To Do\"}},\"customfield_12104\":\"t.u@test.com\"}}]}");            
            _apiClient.Setup(x => x.GetUserByUsername("t.u@test.com")).ReturnsAsync("[{\"accountId\": \"1234\"}]");
            _apiClient.Setup(x => x.DeleteUser(It.IsAny<User>()));
            _apiClient.Setup(x => x.PerformTransition("1", It.IsAny<StringContent>(), null));

           var service = new AtlassianService(_apiClient.Object, _log.Object, _config.Object, _validator);

           await service.ProcessRevokeUserRequests();

           _apiClient.Verify(x => x.GetUserRequests("TST", "1", null), Times.Once);
           _apiClient.Verify(x => x.GetUserByUsername("t.u@test.com"), Times.Never);
           _apiClient.Verify(x => x.DeleteUser(It.IsAny<User>()), Times.Never);
           _apiClient.Verify(x => x.PerformTransition("TST-1", It.IsAny<StringContent>(), null), Times.Never);
        }

        [Test]
        public async Task WhenDueDateIsToday_UserIsRevoked()
        {
            _apiClient.Setup(x => x.GetUserRequests("TST", "1", null)).ReturnsAsync("{\"size\":1,\"start\":0,\"limit\":50,\"isLastPage\":true,\"_links\":{\"self\":\"\",\"base\":\"\",\"context\":\"\"},\"values\":[{\"id\":\"125243\",\"self\":\"\",\"key\":\"TST-1\",\"fields\":{\"summary\":\"Add / Remove user (Jira/Confluence)\",\"created\":\"2021-05-24T16:04:16.880+0100\",\"duedate\":\"" + DateTime.Now.ToString("yyyy-MM-dd") + "\",\"reporter\":{\"self\":\"https://skillsfundingagency.atlassian.net/rest/api/2/user?accountId=5c73cd628adc243ea060ebe9\",\"accountId\":\"5c73cd628adc243ea060ebe9\",\"emailAddress\":\"x.y@education.gov.uk\",\"avatarUrls\":{\"48x48\":\"\",\"24x24\":\"\",\"16x16\":\"\",\"32x32\":\"\"},\"displayName\":\"T User\",\"active\":true,\"timeZone\":\"Europe/London\",\"accountType\":\"atlassian\"},\"issuelinks\":[],\"customfield_11100\":{\"_links\":{\"jiraRest\":\"\",\"web\":\"\",\"self\":\"\",\"agent\":\"\"},\"requestType\":{\"_expands\":[\"field\"],\"id\":\"14\",\"_links\":{\"self\":\"\"},\"name\":\"Add / Remove Jira/Confluence access\",\"description\":\"Request a new account for a new starter, revoke account for a leaver or a reactivation for a user who's account was deactivated due to lack of use\",\"helpText\":\"BEFORE raising this request, please ensure you've read [THIS|https://skillsfundingagency.atlassian.net/wiki/spaces/DM/pages/106966530/How+to+Request+access+to+Jira+Confluence] page on user account requests.Thanks\",\"issueTypeId\":\"10501\",\"serviceDeskId\":\"1\",\"groupIds\":[\"1\"],\"icon\":{\"id\":\"16227\",\"_links\":{\"iconUrls\":{\"48x48\":\"\",\"24x24\":\"\",\"16x16\":\"\",\"32x32\":\"\"}}}},\"currentStatus\":{\"status\":\"In the queue\",\"statusCategory\":\"NEW\",\"statusDate\":{\"iso8601\":\"2021-05-24T16:04:16+0100\",\"jira\":\"2021-05-24T16:04:16.880+0100\",\"friendly\":\"Monday 4:04 PM\",\"epochMillis\":1621868656880}}},\"customfield_12056\":{\"self\":\"\",\"value\":\"Apprenticeship Service\",\"id\":\"1234\"},\"updated\":\"2021-05-24T16:04:18.001+0100\",\"customfield_12105\":{\"self\":\"\",\"value\":\"Revoke\",\"id\":\"1234\"},\"status\":{\"self\":\"\",\"description\":\"\",\"iconUrl\":\"\",\"name\":\"In the queue\",\"id\":\"13000\",\"statusCategory\":{\"self\":\"\",\"id\":2,\"key\":\"new\",\"colorName\":\"blue-gray\",\"name\":\"To Do\"}},\"customfield_12104\":\"t.u@test.com\"}}]}");
            _apiClient.Setup(x => x.GetUserByUsername("t.u@test.com")).ReturnsAsync("[{\"accountId\": \"1234\"}]");
            _apiClient.Setup(x => x.DeleteUser(It.IsAny<User>()));
            _apiClient.Setup(x => x.PerformTransition("1", It.IsAny<StringContent>(), null));

           var service = new AtlassianService(_apiClient.Object, _log.Object, _config.Object, _validator);

           await service.ProcessRevokeUserRequests();

           _apiClient.Verify(x => x.GetUserRequests("TST", "1", null), Times.Once);
           _apiClient.Verify(x => x.GetUserByUsername("t.u@test.com"), Times.Once);
           _apiClient.Verify(x => x.DeleteUser(It.IsAny<User>()), Times.Once);
           _apiClient.Verify(x => x.PerformTransition("TST-1", It.IsAny<StringContent>(), null), Times.Exactly(2));
        }
    }
}