using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Microsoft.Extensions.Options;
using SFA.DAS.OnBoardingTool.Infrastructure.Api.Atlassian;
using SFA.DAS.OnBoardingTool.Infrastructure.Configuration;

namespace SFA.DAS.OnBoardingTool.Infrastructure.Validators
{    
    public class ValueValidator : AbstractValidator<Value>
    {
        private readonly OnBoardingToolFunctions _config;
        public const string CanCreate = "CanCreate";
        public const string CanRevoke = "CanRevoke";

        public ValueValidator(IOptions<OnBoardingToolFunctions> options)
        {
            _config = options.Value;

            RuleSet(CanCreate, SetupCreateRules);
            RuleSet(CanRevoke, SetupRevokeRules);
        }

        public void SetupCreateRules()
        {
                RuleFor(value => value.Fields.Reporter.DisplayName)
                    .NotNull()
                    .NotEmpty()
                    .WithMessage("The reporters name is null or empty")
                ;

                RuleFor(value => value.Fields.Customfield_12104)
                    .NotNull()
                    .NotEmpty()
                    .Must(email => email.Contains(_config.PreAuthorisedEmailDomain))
                    .Unless(value => _config.PreApprovedRequesters.Any(ftr => ftr.Equals(value.Fields.Reporter.DisplayName, System.StringComparison.InvariantCultureIgnoreCase)))
                    .WithMessage("Either the email is not part of a pre-authorised domain or the reporter is not pre-approved to add users from a non-authorised domain")
                ;

                RuleFor(value => value.Fields.Customfield_12105.Value)
                    .NotNull()
                    .NotEmpty()
                    .Must(accessType => accessType.Equals("grant", StringComparison.InvariantCultureIgnoreCase))
                    .WithMessage("Access type request is not 'Grant'")
                ;

                RuleFor(value => value.Fields.DueDate)                    
                    .Must(dueDate => (string.IsNullOrEmpty(dueDate) ? DateTime.Now : DateTime.Parse(dueDate)).Date == DateTime.Now.Date)
                    .WithMessage("The due date is in the future")
                ;
        }

        public void SetupRevokeRules()
        {
            RuleFor(value => value.Fields.Reporter.DisplayName)
                    .NotNull()
                    .NotEmpty()
                    .WithMessage("The reporters name is null or empty")
                ;                

                RuleFor(value => value.Fields.Customfield_12105.Value)
                    .NotNull()
                    .NotEmpty()
                    .Must(accessType => accessType.Equals("revoke", StringComparison.InvariantCultureIgnoreCase))
                    .WithMessage("Access type request is not 'Revoke'")
                ;

                RuleFor(value => value.Fields.DueDate)                    
                    .Must(dueDate => (string.IsNullOrEmpty(dueDate) ? DateTime.Now : DateTime.Parse(dueDate)).Date == DateTime.Now.Date)
                    .WithMessage("The due date is in the future")
                ;
        }
    }
}