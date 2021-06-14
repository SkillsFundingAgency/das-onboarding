using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.OnBoardingTool.Infrastructure.Models
{
    public class StatusDate
    {
        public string Iso8601 { get; set; }
        public string Jira { get; set; }
        public string Friendly { get; set; }
        public long EpochMillis { get; set; }
    }
}
