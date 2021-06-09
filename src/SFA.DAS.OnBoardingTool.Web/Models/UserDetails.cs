 using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.OnBoardingTool.Web.Models
{
    public class UserDetails
    {
        [Required]
        public string Firstname { get; set; }
        [Required]
        public string Lastname { get; set; }
        [Required]
        [RegularExpression(@"([a-zA-Z0-9_\-\.]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5})")]
        public string PersonalEmail { get; set; }
        public AtlassianUser AtlassianUser { get; set; }
        public bool Atlassian { get; set; }
        public bool GitHub { get; set; }
        public GitHubUser GitHubUser { get; set; }
    }
}
