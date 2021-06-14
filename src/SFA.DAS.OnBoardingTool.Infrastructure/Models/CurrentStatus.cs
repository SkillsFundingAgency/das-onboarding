using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.OnBoardingTool.Infrastructure.Models
{
    public class CurrentStatus
    {
        public string Status { get; set; }
        public string StatusCategory { get; set; }
        public StatusDate StatusDate { get; set; }
    }
}
