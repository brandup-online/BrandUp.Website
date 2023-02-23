namespace BrandUp.Website.Identity
{
    public interface IAccessProvider
    {
        /// <summary>
        /// Get current user ID.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<string> GetUserIdAsync(CancellationToken cancellationToken = default);
        /// <summary>
        /// Check is authenticated.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> IsAuthenticatedAsync(CancellationToken cancellationToken = default);
    }
}