using FluentValidation;
using Shared.Request;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CITracker.Validator
{
    public class CITeamRequestValidator : AbstractValidator<CITeamRequest>
    {
        private static readonly Regex HtmlRegex =
        new Regex("<.*?>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public CITeamRequestValidator()
        {
            RuleFor(x => x.ProjectId).GreaterThan(0).WithMessage("A team member can only be added to a valid project.");
            RuleFor(x => x.Team)
            .NotEmpty().WithMessage("At least one team member is required")
            .Must(team => team != null && team.Count > 0).WithMessage("Team list cannot be empty");

            // Validate each item in the list
            RuleForEach(x => x.Team)
                .SetValidator(new TeamMembersValidator())
                .When(x => x.Team != null);

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
    public class TeamMembersValidator : AbstractValidator<TeamMembers>
    {
        private static readonly Regex HtmlRegex =
        new Regex("<.*?>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public TeamMembersValidator()
        {
            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Team member role is required");

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("Kindly select a valid team member");

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
