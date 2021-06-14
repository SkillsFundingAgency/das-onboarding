using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.OnBoardingTool.Infrastructure.Models
{
    public class Links
    {
        public string Context { get; set; }
        public string Next { get; set; }
        public string Prev { get; set; }
        public string JiraRest { get; set; }
        public AvatarUrls AvatarUrls { get; set; }
        public string Self { get; set; }
    }
}
