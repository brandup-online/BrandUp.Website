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
    }
}