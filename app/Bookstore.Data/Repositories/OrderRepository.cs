using Bookstore.Domain;
using Bookstore.Domain.Repositories;
using System.Data.Entity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Bookstore.Data;
using Bookstore.Data.Extensions;
using System.Data.Entity.Infrastructure;
using Bookstore.Domain.Orders;

namespace Bookstore.Data.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime OneSecondToMidnight(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 23, 59, 59, dateTime.Kind);
        }
    }
}

// These classes are defined in the Bookstore.Domain project
// and referenced via project reference

namespace Bookstore.Domain.Repositories
{
    public interface IOrderRepository
    {
        Task AddAsync(Bookstore.Domain.Orders.Order order);
        Task<Bookstore.Domain.Orders.Order> GetAsync(int id);
        Task<Bookstore.Domain.Orders.Order> GetAsync(int id, string sub);
        Task<IEnumerable<Bookstore.Domain.Book>> ListBestSellingBooksAsync(int count);
        Task<OrderStatistics> GetStatisticsAsync();
        Task<IEnumerable<Bookstore.Domain.Orders.Order>> ListAsync(OrderFilters filters, int pageIndex, int pageSize);
        Task<IEnumerable<Bookstore.Domain.Orders.Order>> ListAsync(string sub);
        Task SaveChangesAsync();
    }
}

namespace Bookstore.Domain.Repositories
{
    public class OrderStatistics
    {
        public int PendingOrders { get; set; }
        public int PastDueOrders { get; set; }
        public int OrdersThisMonth { get; set; }
        public int OrdersTotal { get; set; }
    }

    public class OrderFilters
    {
        public OrderStatus? OrderStatusFilter { get; set; }
        public DateTime? OrderDateFromFilter { get; set; }
        public DateTime? OrderDateToFilter { get; set; }
    }
}

namespace Bookstore.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Bookstore.Domain.Orders.Order> Orders { get; set; }
        public DbSet<Bookstore.Domain.Orders.OrderItem> OrderItems { get; set; }
    }
}

namespace Bookstore.Data.Repositories
{
public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext dbContext;

        public OrderRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

public async Task AddAsync(Bookstore.Domain.Orders.Order order)
{
    await Task.Run(() => dbContext.Orders.Add(order));
}

public async Task<Bookstore.Domain.Orders.Order> GetAsync(int id)
{
            return await dbContext.Orders
                .Include(x => x.Customer)
                .Include(x => x.Address)
                .Include(x => x.OrderItems)
                .Include(x => x.OrderItems.Select(y => y.Book))
                .Include(x => x.OrderItems.Select(y => y.Book.BookType))
                .Include(x => x.OrderItems.Select(y => y.Book.Condition))
                .Include(x => x.OrderItems.Select(y => y.Book.Genre))
                .Include(x => x.OrderItems.Select(y => y.Book.Publisher))
                .SingleOrDefaultAsync(x => x.Id == id);
        }

public async Task<Bookstore.Domain.Orders.Order> GetAsync(int id, string sub)
{
    return await dbContext.Orders.SingleOrDefaultAsync(x => x.Id == id && x.Customer.Sub == sub);
}

public async Task<IEnumerable<Bookstore.Domain.Book>> ListBestSellingBooksAsync(int count)
        {
            return await dbContext.OrderItems
                .GroupBy(x => x.BookId)
                .OrderByDescending(x => x.Count())
                .Select(x => x.FirstOrDefault().Book)
                .Take(count)
                .ToListAsync();
        }

        public async Task<OrderStatistics> GetStatisticsAsync()
        {
            var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            return await dbContext.Orders
                .GroupBy(x => 1)
                .Select(x => new OrderStatistics
                {
                    PendingOrders = x.Count(y => y.OrderStatus == Bookstore.Domain.Orders.OrderStatus.Pending),
                    PastDueOrders = x.Count(y => y.OrderStatus == Bookstore.Domain.Orders.OrderStatus.Ordered && y.DeliveryDate < DateTime.UtcNow),
                    OrdersThisMonth = x.Count(y => y.CreatedOn >= startOfMonth),
                    OrdersTotal = x.Count()
                }).SingleOrDefaultAsync();
        }

public async Task<IEnumerable<Bookstore.Domain.Orders.Order>> ListAsync(OrderFilters filters, int pageIndex, int pageSize)
        {
            var query = dbContext.Orders.AsQueryable();

            if (filters.OrderStatusFilter.HasValue)
            {
                query = query.Where(x => x.OrderStatus == filters.OrderStatusFilter);
            }

            if (filters.OrderDateFromFilter.HasValue)
            {
                query = query.Where(x => x.CreatedOn >= filters.OrderDateFromFilter);
            }

            if (filters.OrderDateToFilter.HasValue)
            {
                var filterData = filters.OrderDateToFilter.Value.OneSecondToMidnight();
                query = query.Where(x => x.CreatedOn < filterData );
            }

            query = query
                .Include(x => x.Customer)
                .Include(x => x.OrderItems)
                .Include(x => x.OrderItems.Select(y => y.Book));

            // Apply paging directly
            var orders = await query.Skip(pageIndex * pageSize).Take(pageSize).ToListAsync();

            return orders;
        }

public async Task<IEnumerable<Bookstore.Domain.Orders.Order>> ListAsync(string sub)
        {
            return await dbContext.Orders
                .Include(x => x.OrderItems)
                .Include(x => x.OrderItems.Select(y => y.Book))
                .Where(x => x.Customer.Sub == sub)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }
    }
}