# Apprenticeship Service Onboarding Utility

<img src="https://avatars.githubusercontent.com/u/9841374?s=200&v=4" align="right" alt="UK Government logo">

[![Build Status](https://dev.azure.com/sfa-gov-uk/Digital%20Apprenticeship%20Service/_apis/build/status/das-onboarding?repoName=SkillsFundingAgency%2Fdas-onboarding&branchName=refs%2Fpull%2F1%2Fmerge)](https://dev.azure.com/sfa-gov-uk/Digital%20Apprenticeship%20Service/_build/latest?definitionId=2440&repoName=SkillsFundingAgency%2Fdas-onboarding&branchName=refs%2Fpull%2F1%2Fmerge)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=_projectId_&metric=alert_status)](https://sonarcloud.io/dashboard?id=SkillsFundingAgency_das-onboarding)
[![Jira Project](https://img.shields.io/badge/Jira-Project-blue)](https://skillsfundingagency.atlassian.net/secure/RapidBoard.jspa?rapidView=564&projectKey=QUAL)
[![Confluence Project](https://img.shields.io/badge/Confluence-Project-blue)](https://skillsfundingagency.atlassian.net/wiki/spaces/NDL/pages/1838612769/Service+Maturity)
[![License](https://img.shields.io/badge/license-MIT-lightgrey.svg?longCache=true&style=flat-square)](https://en.wikipedia.org/wiki/MIT_License)

The on-boarding utility automatically creates and removes users from the Apprenticeship Services Atlassian Cloud instance and is driven by requests created in the [Demand Management](https://skillsfundingagency.atlassian.net/jira/servicedesk/projects/DM/) ServiceDesk project using the [Demand Management Portal form](https://skillsfundingagency.atlassian.net/servicedesk/customer/portal/1/group/1/create/14).

It's main goal is to remove the manual intevention required to create users when access requests are made for people with email addresses on pre-approved domains or requests are received from approved requesters for users on non pre-approved email domains.

## How It Works

The on-boarding utility is a background processor running as a number of timer triggered Azure functions that polls a Jira Cloud ServiceDesk project using a Restful Api for requests to create or remove users from the same Atlassian instance. Depending on the type of action in the ServiceDesk request the processor will either create the user with the details from the request or remove them. Once the action has been completed the ServiceDesk ticket is transitioned to Resolved (Done).

During the create action, the email address of the new user is checked against a list of pre-approved email domains and created if it matches. If the email address does not match a pre-approved domain the requesting user is checked against a list of pre-approved requesters and created if it matches. Finally the create process will also look at the due date that has been added to the request and only create the user if the due date is current or past.

## 🚀 Installation

### Pre-Requisites

* A clone of this repository
* An understanding of the [das-employer-config](https://github.com/SkillsFundingAgency/das-employer-config) pattern
* An IDE capable of editing and running Azure Functions
* An API key for an Atlassian Jira instance

### Config

This utility uses the standard Apprenticeship Service configuration. All configuration can be found in the [das-employer-config repository](https://github.com/SkillsFundingAgency/das-employer-config).

You will need to add a local.settings.json file with a layout as below

local.settings.json file
```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopment=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",    
    "ConfigNames": "SFA.DAS.OnBoardingTool.Functions",
    "ConfigurationStorageConnectionString": "UseDevelopment=true",
    "Environment": "LOCAL"
  }
}
```

The Azure Table Storage config will need to be configured like this with the appropriate values:

Row Key: SFA.DAS.OnBoardingTool.Functions_1.0

Partition Key: LOCAL

Data:

```json
{    
    "CreateUserTimerSchedule": "*/30 * * * * *",
    "RevokeUserTimerSchedule": "*/30 * * * * *",
    "OnBoardingToolFunctions": {
        "ServiceDeskId": "",
        "QueueId": "",
        "PreAuthorisedEmailDomain": "",
        "PreApprovedRequesters": [],
        "TriageStatusId": "",
        "ResolvedStatusId": ""
    }, 
   "AtlassianApi": {
        "BaseUrl": "",
        "Credentials": ""
     }
}
```

## 🔗 External Dependencies

This utility uses Atlassian Jira and Jira ServiceDesk Restful Api's. To successfully operate you will need to have access to those Restful Api's in either the Apprenticeship Services Atlassian instance or one that you control for development purposes

The Api documentation can currently be found here:

* https://developer.atlassian.com/cloud/jira/platform/rest/v3/
* https://developer.atlassian.com/cloud/jira/service-desk/rest/intro/

## Technologies

* .NetCore 3.1
* Azure Functions V3
* NLog
* Azure Table Storage
* NUnit
* Moq
* FluentAssertions

## 🐛 Known Issues

There are currently no known issues