
namespace SFA.DAS.OnBoardingTool.Infrastructure.Api.Atlassian
{
    public class PagedDTOCustomerRequestDTO
    {
        public int Size { get; set; }
        public int Start { get; set; }
        public int Limit { get; set; }
        public bool IsLastPage { get; set; }
        public CustomerRequestDTO[] Values { get; set; }
    }
}