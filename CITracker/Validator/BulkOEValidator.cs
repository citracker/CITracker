using FluentValidation;
using Shared.Enumerations;
using Shared.Request;
using Shared.Utilities;

namespace CITracker.Validator
{
    public class BulkOEValidator : AbstractValidator<BulkOE>
    {
        public BulkOEValidator()
        {
            RuleFor(x => x.Title).NotEmpty().WithMessage("A Title is Required");

            RuleFor(x => x.Country).NotEmpty().WithMessage("A Country is Required");

            RuleFor(x => x.Facility).NotEmpty().WithMessage("A Facility is Required");

            RuleFor(x => x.Department).NotEmpty().WithMessage("A Department is Required");

            RuleFor(x => x.Currency).NotEmpty().WithMessage("A Department is Required")
                .Length(3).WithMessage("Currency must be in the ISO 4217 format (e.g USD, GBP, NGN etc..)");

            RuleFor(x => x.Description).NotEmpty().WithMessage("A Description is Required");

            RuleFor(x => x.StartDate).NotEmpty().WithMessage("A StartDate is Required")
                .Must(v => Utils.BeValidIsoDate(v)).WithMessage("Date must be in yyyy-MM-dd format");

            RuleFor(x => x.EndDate).NotEmpty().WithMessage("An EndDate is Required")
                .Must(v => Utils.BeValidIsoDate(v)).WithMessage("Date must be in yyyy-MM-dd format");

            RuleFor(x => x.ExecutiveSponsorEmailAddress).NotEmpty().WithMessage("An ExecutiveSponsorEmailAddress is Required")
                .EmailAddress().WithMessage("ExecutiveSponsorEmailAddress must be a valid email address");

            RuleFor(x => x.SponsorEmailAddress).NotEmpty().WithMessage("An SponsorEmailAddress is Required")
                .EmailAddress().WithMessage("SponsorEmailAddress must be a valid email address");

            RuleFor(x => x.FacilitatorEmailAddress).NotEmpty().WithMessage("An FacilitatorEmailAddress is Required")
                .EmailAddress().WithMessage("FacilitatorEmailAddress must be a valid email address");

            RuleFor(x => x.SavingsClassification).NotEmpty().WithMessage("A SavingsClassification is Required")
                .Must(v => Utils.SavingsClassificationAllowedValues.Contains(v)).WithMessage("SavingsClassification must be one of: Cost Out, Cost Savings, Revenue or Cost Avoidance");

            RuleFor(x => x.IsCarryOverProject).NotEmpty().WithMessage("You need to Identify if this is a Carry Over Project")
                .Must(v => Utils.CarryOverAllowedValues.Contains(v)).WithMessage("IsCarryOverProject must be one of: Yes or No");

            RuleFor(x => x.Priority).NotEmpty().WithMessage("A Priority is Required")
                .Must(v => Utils.PriorityAllowedValues.Contains(v)).WithMessage("Priority must be one of: High, Medium or Low");

            RuleFor(x => x.Status).NotEmpty().WithMessage("A Status is Required")
                .IsEnumName(typeof(Status), caseSensitive: true).WithMessage("Invalid status value. Allowed values are: PROPOSED, INITIATED, COMPLETED, CLOSED, CANCELLED");

        }
    }

}
