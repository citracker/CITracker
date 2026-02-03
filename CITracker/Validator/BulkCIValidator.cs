using FluentValidation;
using Shared.Enumerations;
using Shared.Request;
using Shared.Utilities;

namespace CITracker.Validator
{
    public class BulkCIValidator : AbstractValidator<BulkCI>
    {
        public BulkCIValidator()
        {
            RuleFor(x => x.Title).NotEmpty().WithMessage("A Title is Required");

            RuleFor(x => x.Country).NotEmpty().WithMessage("A Country is Required");

            RuleFor(x => x.Facility).NotEmpty().WithMessage("A Facility is Required");

            RuleFor(x => x.Department).NotEmpty().WithMessage("A Department is Required");

            RuleFor(x => x.ProblemStatement).NotEmpty().WithMessage("A ProblemStatement is Required");

            RuleFor(x => x.EndDate).NotEmpty().WithMessage("An EndDate is Required")
                .Must(v => Utils.BeValidIsoDate(v)).WithMessage("Date must be in yyyy-MM-dd format");

            RuleFor(x => x.Priority).NotEmpty().WithMessage("A Priority is Required")
                .Must(v => Utils.PriorityAllowedValues.Contains(v)).WithMessage("Priority must be one of: High, Medium or Low");

            RuleFor(x => x.StartDate).NotEmpty().WithMessage("A StartDate is Required")
                .Must(v => Utils.BeValidIsoDate(v)).WithMessage("Date must be in yyyy-MM-dd format");

            RuleFor(x => x.Status).NotEmpty().WithMessage("A Status is Required")
                .IsEnumName(typeof(Status), caseSensitive: true).WithMessage("Invalid status value. Allowed values are: PROPOSED, INITIATED, COMPLETED, CLOSED, CANCELLED");

            RuleFor(x => x.BusinessObjectiveAlignment).NotEmpty().WithMessage("A BusinessObjectiveAlignment is Required");

            RuleFor(x => x.Methodology).NotEmpty().WithMessage("A Methodology is Required");

            RuleFor(x => x.Certification).NotEmpty().WithMessage("A Certification is Required")
                .Must(v => Utils.CertificationAllowedValues.Contains(v)).WithMessage("Certification must be one of: Green Belt', 'Black Belt', 'PMP', Not Applicable");

            RuleFor(x => x.Currency).NotEmpty().WithMessage("A Currency is Required")
                .Length(3).WithMessage("Currency must be in the ISO 4217 format (e.g USD, GBP, NGN etc..)");

            RuleFor(x => x.Phase).InclusiveBetween(1, 10).WithMessage("Phase must be between 1 and 10.");

            RuleFor(x => x.SupportingValueStream).NotEmpty().WithMessage("A SupportingValueStream is Required");

        }
    }

}
