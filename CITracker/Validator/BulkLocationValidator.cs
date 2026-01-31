using FluentValidation;
using Shared.Request;

namespace CITracker.Validator
{
    public class BulkLocationValidator : AbstractValidator<BulkLocation>
    {
        public BulkLocationValidator()
        {
            RuleFor(x => x.Country).NotEmpty().WithMessage("A Country is Required");

            RuleFor(x => x.Facility).NotEmpty().WithMessage("A Facility is Required");

            RuleFor(x => x.Department).NotEmpty().WithMessage("A Department is Required");
        }
    }

}
