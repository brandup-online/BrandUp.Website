using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BrandUp.Website.Visitors;

namespace ExampleWebSite
{
    public class VisitorStore : IVisitorStore
    {
        readonly Dictionary<string, Customer> items = new Dictionary<string, Customer>();
        readonly Dictionary<string, string> userIds = new Dictionary<string, string>();

        public Task<IVisitor> CreateAsync(DateTime dateTime)
        {
            var item = new Customer
            {
                LastVisitDate = dateTime
            };

            items.Add(item.Id, item);

            return Task.FromResult<IVisitor>(item);
        }
        public Task<IVisitor> FindByIdAsync(string id)
        {
            items.TryGetValue(id.ToLower(), out Customer item);

            return Task.FromResult<IVisitor>(item);
        }
        public Task UpdateLastVisitDateAsync(IVisitor visitor, DateTime dateTime)
        {
            var v = visitor as Customer ?? throw new ArgumentException();

            v.LastVisitDate = dateTime;

            return Task.CompletedTask;
        }
        public Task DeleteAsync(IVisitor visitor)
        {
            if (visitor == null)
                throw new ArgumentNullException(nameof(visitor));

            items.Remove(visitor.Id.ToLower());

            return Task.CompletedTask;
        }

        public int Count => items.Count;
    }

    public class Customer : IVisitor
    {
        public string Id { get; } = Guid.NewGuid().ToString().ToLower();
        public DateTime LastVisitDate { get; set; }
        public string Yclid { get; set; }
    }
}