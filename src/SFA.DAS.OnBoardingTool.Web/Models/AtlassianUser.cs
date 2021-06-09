using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.OnBoardingTool.Web.Models
{
    public class AtlassianUser
    {
        [RegularExpression(@"([a-zA-Z0-9_\-\.]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5})")]
        public string Email { get; set; }
    }
}