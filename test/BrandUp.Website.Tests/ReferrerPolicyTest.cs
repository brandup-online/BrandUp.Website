namespace BrandUp.Website
{
    public class ReferrerPolicyTest
    {
        [Theory]
        [InlineData(ReferrerPolicy.None, null)]
        [InlineData(ReferrerPolicy.NoReferrer, "no-referrer")]
        [InlineData(ReferrerPolicy.NoReferrerWhenDowngrade, "no-referrer-when-downgrade")]
        [InlineData(ReferrerPolicy.Origin, "origin")]
        [InlineData(ReferrerPolicy.OriginWhenCrossOrigin, "origin-when-cross-origin")]
        [InlineData(ReferrerPolicy.SameOrigin, "same-origin")]
        [InlineData(ReferrerPolicy.StrictOrigin, "strict-origin")]
        [InlineData(ReferrerPolicy.StrictOriginWhenCrossOrigin, "strict-origin-when-cross-origin")]
        [InlineData(ReferrerPolicy.UnsafeUrl, "unsafe-url")]
        public void ToHeaderValue(ReferrerPolicy policy, string? expected)
        {
            Assert.Equal(expected, policy.ToHeaderValue());
        }

        [Fact]
        public void ToHeaderValue_UnknownValue_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ((ReferrerPolicy)100).ToHeaderValue());
        }
    }
}
