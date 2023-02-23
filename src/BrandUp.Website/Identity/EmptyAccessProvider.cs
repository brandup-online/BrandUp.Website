namespace BrandUp.Website.Identity
{
    public class EmptyAccessProvider : IAccessProvider
    {
        public Task<bool> IsAuthenticatedAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        public Task<string> GetUserIdAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<string>(null);
        }
    }
}