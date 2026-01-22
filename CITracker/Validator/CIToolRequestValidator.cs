using FluentValidation;
using Shared.Request;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CITracker.Validator
{
    public class CIToolRequestValidator : AbstractValidator<CIToolRequest>
    {
        private static readonly Regex HtmlRegex =
        new Regex("<.*?>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public CIToolRequestValidator()
        {
            RuleFor(x => x.ProjectId).GreaterThan(0).WithMessage("A team member can only be added to a valid project.");
            RuleFor(x => x.Tool).NotEmpty().WithMessage("At least one tool is required");
            RuleFor(x => x.Methodology).NotEmpty().WithMessage("Selected Methodology is required");


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
