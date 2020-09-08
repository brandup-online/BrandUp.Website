using System;
using System.Threading.Tasks;

namespace BrandUp.Website
{
    public interface IVisitorStore
    {
        Task<IVisitor> FindByIdAsync(string id);
        Task<IVisitor> CreateAsync(DateTime dateTime);
        Task UpdateLastVisitDateAsync(IVisitor visitor, DateTime dateTime);
        Task DeleteAsync(IVisitor visitor);
    }
}