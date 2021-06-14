using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.OnBoardingTool.Infrastructure.Models
{
    public class Fields
    {
        public string Summary { get; set; }
        public Issuetype Issuetype { get; set; }
        public string DueDate { get; set; }
        public string Created { get; set; }
        public Reporter Reporter { get; set; }
        public List<object> Issuelinks { get; set; }
        public Customfield11100 Customfield_11100 { get; set; }
        public Customfield12056 Customfield_12056 { get; set; }
        public string Updated { get; set; }
        public Customfield12105 Customfield_12105 { get; set; }
        public string Customfield_12104 { get; set; }
        public Status status { get; set; }
    }
}
