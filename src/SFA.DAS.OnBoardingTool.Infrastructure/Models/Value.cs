using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.OnBoardingTool.Infrastructure.Models
{
    public class Value
    {
        public string Id { get; set; }
        public string Self { get; set; }
        public string Key { get; set; }
        public Fields Fields { get; set; }
    }
}
