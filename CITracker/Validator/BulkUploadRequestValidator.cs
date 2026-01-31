using FluentValidation;
using Shared.Request;

namespace CITracker.Validator
{
    public class BulkUploadRequestValidator : AbstractValidator<BulkUploadRequest>
    {
        public BulkUploadRequestValidator()
        {
            RuleFor(x => x.UploadType)
                .InclusiveBetween(1, 3)
                .WithMessage("Invalid upload type");

            RuleFor(x => x.Rows)
                .NotEmpty()
                .WithMessage("No rows submitted");

            RuleFor(x => x.Rows)
                .SetValidator(new BulkRowValidator());
        }
    }

    public class BulkRowValidator : AbstractValidator<List<Dictionary<string, string>>>
    {
        public BulkRowValidator()
        {
            RuleFor(x => x)
                .NotEmpty()
                .WithMessage("No rows submitted");

            RuleForEach(x => x)
                .SetValidator(new SingleRowValidator());

            RuleFor(x => x)
                .Must(HaveNoDuplicates)
                .WithMessage("Duplicate rows detected in upload");
        }

        private bool HaveNoDuplicates(
            List<Dictionary<string, string>> rows)
        {
            if (rows == null || rows.Count == 0)
                return true;

            var normalizedRows = rows.Select(r =>
                string.Join("|",
                    r
                    .OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase)
                    .Select(v => v.Value?.Trim() ?? string.Empty)
                )
            );

            return normalizedRows.Distinct().Count() == normalizedRows.Count();
        }
    }

    public class SingleRowValidator
    : AbstractValidator<Dictionary<string, string>>
    {
        public SingleRowValidator()
        {
            RuleFor(r => r)
                .Must(r => r.Values.All(v => !string.IsNullOrWhiteSpace(v)))
                .WithMessage("All columns must have values");
        }
    }
}
