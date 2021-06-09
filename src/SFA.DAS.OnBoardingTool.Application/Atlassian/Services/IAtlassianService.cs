using System.Threading.Tasks;
using SFA.DAS.OnBoardingTool.Domain;
using SFA.DAS.OnBoardingTool.Infrastructure.Api.Atlassian;

namespace SFA.DAS.OnBoardingTool.Application
{
    public interface IAtlassianService
    {     
        Task ProcessCreateUserRequests();
        Task ProcessRevokeUserRequests();
        Task<QueueItems> GetUserRequests();
        Task<UserDTO> GetUser(string username);
        Task<(string, bool)> CreateUser(Value request, User user);
    }
}