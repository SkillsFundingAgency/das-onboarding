
namespace SFA.DAS.OnBoardingTool.Infrastructure.Api.Atlassian
{
    public class CustomerRequestDTO
    {
        public string IssueId { get; set; }
        public string IssueKey { get; set; }
        public string RequestTypeId { get; set; }
        public CustomerRequestStatusDTO Status { get; set; }
        public UserDTO Reporter { get; set; }
        public CustomerRequestFieldValueDTO[] RequestFieldValues { get; set; }
    }
}