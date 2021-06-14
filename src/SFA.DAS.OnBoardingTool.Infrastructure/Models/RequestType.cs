using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.OnBoardingTool.Infrastructure.Models
{
    public class RequestType
    {
        public List<string> _expands { get; set; }
        public string Id { get; set; }
        public Links _links { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string HelpText { get; set; }
        public string IssueTypeId { get; set; }
        public string ServiceDeskId { get; set; }
        public List<string> GroupIds { get; set; }
        public Icon Icon { get; set; }
    }
}
