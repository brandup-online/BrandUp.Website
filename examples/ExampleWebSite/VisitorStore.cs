using BrandUp.Website;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExampleWebSite
{
    public class VisitorStore : IVisitorStore
    {
        readonly Dictionary<string, Customer> items = new Dictionary<string, Customer>();
        readonly Dictionary<string, string> userIds = new Dictionary<string, string>();

        public Task<IVisitor> CreateNewAsync(string websiteId)
        {
            if (websiteId == null)
                throw new ArgumentNullException(nameof(websiteId));

            var item = new Customer
            {
                WebsiteId = websiteId
            };

            items.Add(item.Id, item);

            return Task.FromResult<IVisitor>(item);
        }
        public Task<IVisitor> FindByIdAsync(string id)
        {
            items.TryGetValue(id.ToLower(), out Customer item);

            return Task.FromResult<IVisitor>(item);
        }
        public Task SetWebsiteAsync(IVisitor visitor, string websiteId)
        {
            var v = visitor as Customer ?? throw new ArgumentException();

            v.WebsiteId = websiteId;

            return Task.CompletedTask;
        }
        public Task SetUserAsync(IVisitor visitor, string userId)
        {
            var v = visitor as Customer ?? throw new ArgumentException();

            v.UserId = userId;

            if (!string.IsNullOrEmpty(userId))
                userIds[userId] = v.Id;
            else if (!string.IsNullOrEmpty(v.UserId))
                userIds.Remove(v.UserId);

            return Task.CompletedTask;
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
        public Task<IVisitor> FindByUserIdAsync(string id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            if (!userIds.TryGetValue(id, out string visitorId))
                return Task.FromResult<IVisitor>(null);

            return Task.FromResult<IVisitor>(items[visitorId]);
        }

        public int Count => items.Count;
    }

    public class Customer : IVisitor
    {
        public string Id { get; } = Guid.NewGuid().ToString().ToLower();
        public string WebsiteId { get; set; }
        public DateTime LastVisitDate { get; set; }
        public string UserId { get; set; }
    }
}