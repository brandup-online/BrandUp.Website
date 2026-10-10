namespace BrandUp.Website.Helpers
{
    public class SeoHelperTest
    {
        [Fact]
        public void IsBot_YandexBot()
        {
            var isBot = SeoHelper.IsBot("Mozilla/5.0 (compatible; YandexBot/3.0; +http://yandex.com/bots)", out SearchBotName searchBot);

            Assert.True(isBot);
            Assert.Equal(SearchBotName.Yandex, searchBot);
        }

        [Fact]
        public void IsBot_YandexAccessibilityBot()
        {
            var isBot = SeoHelper.IsBot("Mozilla/5.0 (compatible; YandexAccessibilityBot/3.0; +http://yandex.com/bots)", out SearchBotName searchBot);

            Assert.True(isBot);
            Assert.Equal(SearchBotName.Yandex, searchBot);
        }

        [Fact]
        public void IsBot_GoogleBot()
        {
            var isBot = SeoHelper.IsBot("Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)", out SearchBotName searchBot);

            Assert.True(isBot);
            Assert.Equal(SearchBotName.Google, searchBot);
        }

        [Fact]
        public void IsBot_GoogleBotImage()
        {
            var isBot = SeoHelper.IsBot("Googlebot-Image/1.0", out SearchBotName searchBot);

            Assert.True(isBot);
            Assert.Equal(SearchBotName.Google, searchBot);
        }

        [Fact]
        public void IsBot_MailRu()
        {
            var isBot = SeoHelper.IsBot("Mozilla/5.0 (compatible; Linux x86_64; Mail.RU_Bot/2.0; +//go.mail.ru/help/robots)", out SearchBotName searchBot);

            Assert.True(isBot);
            Assert.Equal(SearchBotName.Mail, searchBot);
        }

        [Fact]
        public void IsBot_Bing()
        {
            var isBot = SeoHelper.IsBot("Mozilla/5.0 (compatible; bingbot/2.0; +http://www.bing.com/bingbot.htm)", out SearchBotName searchBot);

            Assert.True(isBot);
            Assert.Equal(SearchBotName.Bing, searchBot);
        }

        [Theory]
        [InlineData("https://example.com/?_gl=1*r0v4yt*_ga*NzMz", "https://example.com/")]
        [InlineData("https://example.com/max?utm_source=yandex&utm_medium=cpc&yclid=123", "https://example.com/max")]
        [InlineData("https://example.com/blog?page=2&utm_source=tg", "https://example.com/blog?page=2")]
        [InlineData("https://example.com/blog?utm_source=tg&page=2&_gl=1*x", "https://example.com/blog?page=2")]
        public void RemoveQueryParameters_Removes(string url, string expected)
        {
            var result = SeoHelper.RemoveQueryParameters(new Uri(url), new WebsiteOptions().TrackingQueryParameters);

            Assert.Equal(expected, result.ToString());
        }

        [Theory]
        [InlineData("https://example.com/")]
        [InlineData("https://example.com/blog?page=2")]
        [InlineData("https://example.com/search?q=%D0%BC%D0%B0%D0%BA%D1%81&utm=1")]
        // имена сравниваются с учётом регистра
        [InlineData("https://example.com/?UTM_SOURCE=x")]
        public void RemoveQueryParameters_NothingToRemove_SameInstance(string url)
        {
            var uri = new Uri(url);

            Assert.Same(uri, SeoHelper.RemoveQueryParameters(uri, new WebsiteOptions().TrackingQueryParameters));
        }
    }
}