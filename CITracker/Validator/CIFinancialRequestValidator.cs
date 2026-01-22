using FluentValidation;
using Shared.Request;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CITracker.Validator
{
    public class CIFinancialRequestValidator : AbstractValidator<CIFinancialRequest>
    {
        private static readonly Regex HtmlRegex =
        new Regex("<.*?>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public CIFinancialRequestValidator()
        {
            RuleFor(x => x.ProjectId).GreaterThan(0).WithMessage("A team member can only be added to a valid project.");
            RuleFor(x => x.Hard)
            .NotEmpty().WithMessage("At least one tea is required")
            .Must(team => team != null && team.Count > 0).WithMessage("Team list cannot be empty");

            // Validate each item in the list
            RuleFor(x => x)
            .Must(HasAtLeastOneList)
            .WithMessage("At least one of Hard or Soft saving must contain a value.");

            RuleFor(x => x)
            .Custom((model, context) =>
            {
                var stringProperties = model.GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.PropertyType == typeof(string));

                foreach (var prop in stringProperties)
                {
                    var value = prop.GetValue(model) as string;

                    if (!string.IsNullOrWhiteSpace(value) &&
                        HtmlRegex.IsMatch(value))
                    {
                        context.AddFailure(
                            prop.Name,
                            $"{prop.Name} must not contain HTML or script content."
                        );
                    }
                }
            });
        }

        private static bool HasAtLeastOneList(CIFinancialRequest model)
        {
            return (model.Hard?.Any() ?? false)
                || (model.Soft?.Any() ?? false);
        }
    }
    //public class SavingsValidator : AbstractValidator<Savings>
    //{
    //    private static readonly Regex HtmlRegex =
    //    new Regex("<.*?>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    //    public SavingsValidator()
    //    {
    //        RuleFor(x => x.Date)
    //            .NotEmpty().WithMessage("Team member role is required");

    //        RuleFor(x => x.UserId)
    //            .NotEmpty().WithMessage("Kindly select a valid team member");            

    //        RuleFor(x => x)
    //        .Custom((model, context) =>
    //        {
    //            var stringProperties = model.GetType()
    //                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
    //                .Where(p => p.PropertyType == typeof(string));

    //            foreach (var prop in stringProperties)
    //            {
    //                var value = prop.GetValue(model) as string;

    //                if (!string.IsNullOrWhiteSpace(value) &&
    //                    HtmlRegex.IsMatch(value))
    //                {
    //                    context.AddFailure(
    //                        prop.Name,
    //                        $"{prop.Name} must not contain HTML or script content."
    //                    );
    //                }
    //            }
    //        });
    //    }
    //}
}
