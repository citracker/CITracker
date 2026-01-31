using FluentValidation;
using Shared.Request;

namespace CITracker.Validator
{
    public class BulkUserValidator : AbstractValidator<BulkUser>
    {
        public BulkUserValidator()
        {
            RuleFor(x => x.EmailAddress)
                .NotEmpty()
                .WithMessage("An Email Address is Required")
                .EmailAddress()
                .WithMessage("A valid Email Address is Required");

            RuleFor(x => x.Name).NotEmpty().WithMessage("User's Name is Required");
        }
    }

}
