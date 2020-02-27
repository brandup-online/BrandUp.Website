using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Website.Pages
{
    public interface IPageNavigationProvider
    {
        Task InitializeAsync(CancellationToken cancellationToken = default);
        Task BuildApplicationClientDataAsync(Dictionary<string, object> clientData, CancellationToken cancellationToken = default);
        Task BuildNavigationClientDataAsync(Dictionary<string, object> clientData, CancellationToken cancellationToken = default);
        Task BuildPageClientDataAsync(Dictionary<string, object> clientData, CancellationToken cancellationToken = default);
    }
}