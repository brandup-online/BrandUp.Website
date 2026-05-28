using Microsoft.Extensions.Options;

namespace BrandUp.Website
{
    public class WebsiteOptions
    {
        public string Host { get; set; } = "localhost";
        public List<string> Aliases { get; set; }
        public string CookiesPrefix { get; set; }
        public string ProtectionPurpose { get; set; } = "BrandUp.Website";
        public bool RedirectToHttps { get; set; } = true;

        public void Validate()
        {
            var errors = GetValidationErrors().ToList();
            if (errors.Count > 0)
                throw new System.InvalidOperationException(string.Join(" ", errors));
        }

        internal IEnumerable<string> GetValidationErrors()
        {
            if (string.IsNullOrEmpty(Host))
                yield return $"Не задан параметр {nameof(Host)}.";

            if (string.IsNullOrEmpty(CookiesPrefix))
                yield return $"Не задан параметр {nameof(CookiesPrefix)}.";

            if (string.IsNullOrEmpty(ProtectionPurpose))
                yield return $"Не задан параметр {nameof(ProtectionPurpose)}.";
        }
    }

    sealed class WebsiteOptionsValidator : IValidateOptions<WebsiteOptions>
    {
        public ValidateOptionsResult Validate(string name, WebsiteOptions options)
        {
            var errors = options.GetValidationErrors().ToList();
            if (errors.Count > 0)
                return ValidateOptionsResult.Fail(errors);

            return ValidateOptionsResult.Success;
        }
    }
}