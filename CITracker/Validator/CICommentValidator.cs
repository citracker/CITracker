using FluentValidation;
using Shared.Request;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CITracker.Validator
{
    public class CICommentValidator : AbstractValidator<CICommentRequest>
    {
        private static readonly Regex HtmlRegex =
        new Regex("<.*?>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public CICommentValidator()
        {
            RuleFor(x => x.ProjectId).GreaterThan(0).WithMessage("A comment can only be added to a valid project.");
            //RuleFor(x => x.Comment)
            //.NotEmpty().WithMessage("At least one comment is required")
            //.Must(team => team != null && team.Count > 0).WithMessage("Comment list cannot be empty");

            //// Validate each item in the list
            //RuleForEach(x => x.Comment)
            //    .SetValidator(new CommentValidator())
            //    .When(x => x.Comment != null);


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
    //public class CommentValidator : AbstractValidator<Commentz>
    //{
    //    private static readonly Regex HtmlRegex =
    //    new Regex("<.*?>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    //    public CommentValidator()
    //    {
    //        RuleFor(x => x.Comment)
    //            .NotEmpty().WithMessage("A comment is required");

    //        RuleFor(x => x.Date)
    //            .NotEmpty().WithMessage("Kindly select a valid date");

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
