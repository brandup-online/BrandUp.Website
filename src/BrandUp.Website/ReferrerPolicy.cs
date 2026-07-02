namespace BrandUp.Website
{
    /// <summary>
    /// Политика отправки заголовка Referer браузером (значение заголовка ответа Referrer-Policy).
    /// </summary>
    public enum ReferrerPolicy
    {
        /// <summary>
        /// Заголовок Referrer-Policy не отправляется.
        /// </summary>
        None,
        /// <summary>
        /// Referer не отправляется никогда.
        /// </summary>
        NoReferrer,
        /// <summary>
        /// Полный URL, кроме перехода HTTPS→HTTP.
        /// </summary>
        NoReferrerWhenDowngrade,
        /// <summary>
        /// Всегда только origin, без пути и query.
        /// </summary>
        Origin,
        /// <summary>
        /// Внутри сайта — полный URL, на другие сайты — только origin.
        /// </summary>
        OriginWhenCrossOrigin,
        /// <summary>
        /// Полный URL только внутри своего сайта, на другие сайты — ничего.
        /// </summary>
        SameOrigin,
        /// <summary>
        /// Только origin, и только без деградации HTTPS→HTTP.
        /// </summary>
        StrictOrigin,
        /// <summary>
        /// Внутри сайта — полный URL, на другие сайты — origin, при HTTPS→HTTP — ничего.
        /// </summary>
        StrictOriginWhenCrossOrigin,
        /// <summary>
        /// Всегда полный URL. Небезопасно: утечка путей и query на сторонние сайты.
        /// </summary>
        UnsafeUrl
    }

    public static class ReferrerPolicyExtensions
    {
        /// <summary>
        /// Значение для заголовка Referrer-Policy. Для <see cref="ReferrerPolicy.None"/> возвращает null.
        /// </summary>
        public static string? ToHeaderValue(this ReferrerPolicy policy)
        {
            return policy switch
            {
                ReferrerPolicy.None => null,
                ReferrerPolicy.NoReferrer => "no-referrer",
                ReferrerPolicy.NoReferrerWhenDowngrade => "no-referrer-when-downgrade",
                ReferrerPolicy.Origin => "origin",
                ReferrerPolicy.OriginWhenCrossOrigin => "origin-when-cross-origin",
                ReferrerPolicy.SameOrigin => "same-origin",
                ReferrerPolicy.StrictOrigin => "strict-origin",
                ReferrerPolicy.StrictOriginWhenCrossOrigin => "strict-origin-when-cross-origin",
                ReferrerPolicy.UnsafeUrl => "unsafe-url",
                _ => throw new ArgumentOutOfRangeException(nameof(policy))
            };
        }
    }
}
