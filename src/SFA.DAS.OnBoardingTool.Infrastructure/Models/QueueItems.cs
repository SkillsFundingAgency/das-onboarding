using System;
using System.Collections.Generic;

namespace SFA.DAS.OnBoardingTool.Infrastructure.Api.Atlassian
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

    public class Issuetype
    {
        public string Self { get; set; }
        public string Id { get; set; }
        public string Description { get; set; }
        public string IconUrl { get; set; }
        public string Name { get; set; }
        public bool Subtask { get; set; }
        public int AvatarId { get; set; }
    }

    public class AvatarUrls
    {
        public string _48x48 { get; set; }
        public string _24x24 { get; set; }
        public string _16x16 { get; set; }
        public string _32x32 { get; set; }
    }

    public class Reporter
    {
        public string Name { get; set; }
        public string Key { get; set; }
        public string EmailAddress { get; set; }
        public string DisplayName { get; set; }
        public bool Active { get; set; }
        public string TimeZone { get; set; }
        //public Links _links { get; set; }
    }

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

    public class Icon
    {
        public string Id { get; set; }
        public Links _links { get; set; }
    }

    public class StatusDate
    {
        public string Iso8601 { get; set; }
        public string Jira { get; set; }
        public string Friendly { get; set; }
        public long EpochMillis { get; set; }
    }

    public class CurrentStatus
    {
        public string Status { get; set; }
        public string StatusCategory { get; set; }
        public StatusDate StatusDate { get; set; }
    }

    public class Customfield11100
    {
        //public Links _links { get; set; }
        public RequestType RequestType { get; set; }
        public CurrentStatus CurrentStatus { get; set; }
    }

    public class Customfield12056
    {
        public string Self { get; set; }
        public string Value { get; set; }
        public string Id { get; set; }
    }

    public class Customfield12105
    {
        public string Self { get; set; }
        public string Value { get; set; }
        public string Id { get; set; }
    }



    public class Status
    {
        public string Self { get; set; }
        public string Description { get; set; }
        public string IconUrl { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }
        public StatusCategory StatusCategory { get; set; }
    }

    public class StatusCategory
    {
        public string Self { get; set; }
        public int Id { get; set; }
        public string Key { get; set; }
        public string ColorName { get; set; }
        public string Name { get; set; }
    }

    public class Value
    {
        public string Id { get; set; }
        public string Self { get; set; }
        public string Key { get; set; }
        public Fields Fields { get; set; }
    }

    public class QueueItems
    {
        //public List<object> _expands { get; set; }
        public int Size { get; set; }
        public int Start { get; set; }
        public int Limit { get; set; }
        public bool IsLastPage { get; set; }
        //public Links _links { get; set; }
        public List<Value> Values { get; set; }
    }


}