using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Website.Identity
{
    public interface IAccessProvider
    {
        Task<string> GetUserIdAsync(CancellationToken cancellationToken = default);
        Task<bool> CheckAccessAsync(CancellationToken cancellationToken = default);
    }

    public class EmptyAccessProvider : IAccessProvider
    {
        public Task<bool> CheckAccessAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        public Task<string> GetUserIdAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<string>(null);
        }
    }
}