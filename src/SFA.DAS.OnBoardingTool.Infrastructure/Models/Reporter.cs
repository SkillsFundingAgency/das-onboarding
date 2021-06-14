using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.OnBoardingTool.Infrastructure.Models
{
    public class Reporter
    {
        public string Name { get; set; }
        public string Key { get; set; }
        public string EmailAddress { get; set; }
        public string DisplayName { get; set; }
        public bool Active { get; set; }
        public string TimeZone { get; set; }
    }
}
