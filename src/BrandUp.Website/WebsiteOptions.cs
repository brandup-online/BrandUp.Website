using Microsoft.Extensions.Options;

namespace BrandUp.Website
{
    public class WebsiteOptions
    {
        public string Host { get; set; } = "localhost";
        public List<string>? Aliases { get; set; }
        public string? CookiesPrefix { get; set; }
        public string ProtectionPurpose { get; set; } = "BrandUp.Website";
        public bool RedirectToHttps { get; set; } = true;

        /// <summary>
        /// Политика для заголовка ответа Referrer-Policy. <see cref="ReferrerPolicy.None"/> — заголовок не отправляется.
        /// </summary>
        public ReferrerPolicy ReferrerPolicy { get; set; } = ReferrerPolicy.StrictOriginWhenCrossOrigin;

        /// <summary>
        /// Параметры query, которые добавляют реклама и аналитика: метки кампаний, идентификаторы кликов,
        /// кросс-доменный <c>_gl</c> Google-тега. Страницу они не меняют, поэтому в канонический адрес
        /// (<see cref="Pages.AppPageModel.CanonicalLink"/>) и <c>og:url</c> не попадают. Имена сравниваются
        /// с учётом регистра — так их пишут счётчики. Из конфигурации значения добавляются к списку
        /// по умолчанию, а не заменяют его; убрать значения по умолчанию можно только из кода.
        /// </summary>
        public List<string> TrackingQueryParameters { get; set; } =
        [
            "_gl",
            "_openstat",
            "utm_id",
            "utm_source",
            "utm_medium",
            "utm_campaign",
            "utm_content",
            "utm_term",
            "gclid",
            "gbraid",
            "wbraid",
            "msclkid",
            "yclid",
            "ysclid",
            "fbclid"
        ];

        public void Validate()
        {
            var errors = GetValidationErrors().ToArray();
            if (errors.Length > 0)
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
        public ValidateOptionsResult Validate(string? name, WebsiteOptions options)
        {
            var errors = options.GetValidationErrors().ToList();
            if (errors.Count > 0)
                return ValidateOptionsResult.Fail(errors);

            return ValidateOptionsResult.Success;
        }
    }
}