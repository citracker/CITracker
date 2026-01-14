using FluentValidation;
using Shared.Request;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CITracker.Validator
{
    public class CIRequestValidator : AbstractValidator<CIRequest>
    {
        private static readonly Regex HtmlRegex =
        new Regex("<.*?>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public CIRequestValidator()
        {
            RuleFor(x => x.Title).NotEmpty().WithMessage("Title is required.");
            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("StartDate is required.")
                .LessThan(x => x.EndDate).WithMessage("StartDate must be earlier than EndDate.");
            RuleFor(x => x.EndDate)
                .NotEmpty().WithMessage("EndDate is required.")
                .GreaterThan(x => x.StartDate).WithMessage("EndDate must be later than StartDate.");
            RuleFor(x => x.Priority).NotEmpty().WithMessage("Priority is required.");
            RuleFor(x => x.BusinessObjectiveAlignment).NotEmpty().WithMessage("At least one Business Objective Alignment is required.");
            RuleFor(x => x.ProblemStatement).NotEmpty().WithMessage("ProblemStatement is required.");
            RuleFor(x => x.Methodology).NotEmpty().WithMessage("Methodology is required.");
            RuleFor(x => x.Certification).NotEmpty().WithMessage("Certification is required.");
            RuleFor(x => x.TotalExpectedRevenue).NotEmpty().WithMessage("TotalExpectedRevenue is required.");
            RuleFor(x => x.Currency).NotEmpty().WithMessage("Currency is required.");
            RuleFor(x => x.Status).NotEmpty().WithMessage("Project Status is required.");
            RuleFor(x => x.Phase).NotEmpty().WithMessage("Project Phase is required.");
            RuleFor(x => x.CountryId).GreaterThan(0).WithMessage("Country is Required.");
            RuleFor(x => x.FacilityId).GreaterThan(0).WithMessage("Facility is Required.");
            RuleFor(x => x.DepartmentId).GreaterThan(0).WithMessage("Department is Required.");
            RuleFor(x => x.SupportingValueStream).NotEmpty().WithMessage("Supporting Value Stream is required.");

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
    }
}
