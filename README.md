# Apprenticeship Service Onboarding Utility

This project contains Azure functions that will create and delete users in the ESFA Atlassian instance from requests raised in the Demand Management Jira Service Desk project.

## Getting Started

* Clone this repository
* Get the latest config from the [das-employer-config](https://github.com/SkillsFundingAgency/das-employer-config) repository and ensure it is available in the Azure Storage Emulator with a partition key of "LOCAL" and a RowKey of "SFA.DAS.OnBoardingTool.Functions_1.0"
* Set the SFA.DAS.OnBoardingTool.Functions project as the startup project
* Set local.settings.json as follows:

```
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

### Prerequisites

* An understanding of the [das-employer-config](https://github.com/SkillsFundingAgency/das-employer-config) pattern
* An IDE capable of editing and running Azure Functions
* An API key for an Atlassian Jira instance
* Azure Functions tools

## Testing

Tests are available in the SFA.DAS.OnBoardingTool.Tests project. A TestHarness console app is also available to allow testing without having to rely on the Functions

## Known Issues

There are currently no known issues

## License

This project is distributed under the [MIT License](https://github.com/SkillsFundingAgency/das-onboarding/blob/main/LICENSE)