
using System.Collections.Generic;

namespace SFA.DAS.OnBoardingTool.Infrastructure.Configuration
{
    public class OnBoardingToolFunctions
    {
        public string ServiceDeskId { get; set; }
        public string QueueId { get; set; }
        public string PreAuthorisedEmailDomain { get; set; }
        public List<string> PreApprovedRequesters { get; set; } = new List<string>();
        public string TriageStatusId { get; set; }
        public string ResolvedStatusId { get; set; }

        public override string ToString()
        {
            return $"ServiceDeskId: {ServiceDeskId}, QueueId: {QueueId}, PreAuthorisedEmailDomain: {PreAuthorisedEmailDomain}, PreApprovedRequesters: {PreApprovedRequesters}, TriageStatusId: {TriageStatusId}, ResolvedStatusId: {ResolvedStatusId}";
        }
    }
}