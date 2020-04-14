using System;
using System.Threading.Tasks;

namespace BrandUp.Website
{
    public interface IVisitorStore
    {
        Task<IVisitor> FindByIdAsync(string id);
        Task<IVisitor> CreateNewAsync(string websiteId);
        Task UpdateLastVisitDateAsync(IVisitor visitor, DateTime dateTime);
        Task SetWebsiteAsync(IVisitor visitor, string websiteId);
        Task DeleteAsync(IVisitor visitor);
    }
}