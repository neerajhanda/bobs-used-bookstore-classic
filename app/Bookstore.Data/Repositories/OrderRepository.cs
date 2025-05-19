using Bookstore.Domain;
using Bookstore.Domain.Repositories;
using Bookstore.Domain.Interfaces;
using Bookstore.Domain.Orders;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

// Define a local Book class to avoid the namespace conflict
namespace Bookstore.Data.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public object BookType { get; set; }
        public object Condition { get; set; }
        public object Genre { get; set; }
        public object Publisher { get; set; }
    }
}

namespace Bookstore.Domain.Interfaces
{
    public interface IPaginatedList<T>
    {
        int PageIndex { get; }
        int PageSize { get; }
        int TotalCount { get; }
        int TotalPages { get; }
        List<T> Items { get; }
        bool HasPreviousPage { get; }
        bool HasNextPage { get; }
        Task PopulateAsync();
    }
}

namespace Bookstore.Domain.Repositories
{
    public interface IOrderRepository
    {
        Task AddAsync(Bookstore.Domain.Orders.Order order);
        Task<Bookstore.Domain.Orders.Order> GetAsync(int id);
        Task<Bookstore.Domain.Orders.Order> GetAsync(int id, string sub);
        Task<IEnumerable<Bookstore.Data.Models.Book>> ListBestSellingBooksAsync(int count);
        Task<Bookstore.Data.OrderStatistics> GetStatisticsAsync();
        Task<Bookstore.Domain.Interfaces.IPaginatedList<Bookstore.Domain.Orders.Order>> ListAsync(Bookstore.Domain.Orders.OrderFilters filters, int pageIndex, int pageSize);
        Task<IEnumerable<Bookstore.Domain.Orders.Order>> ListAsync(string sub);
        Task SaveChangesAsync();
    }
}

namespace Bookstore.Domain.Orders
{
    public enum OrderStatus
    {
        Pending,
        Ordered
    }

    public class Order
    {
        public int Id { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime DeliveryDate { get; set; }
        public Customer Customer { get; set; }
        public Address Address { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
    }

    public class Customer
    {
        public string Sub { get; set; }
    }

    public class Address
    {
    }

    public class OrderItem
    {
        public int BookId { get; set; }
        public Bookstore.Data.Models.Book Book { get; set; }
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
    public class OrderStatistics
    {
        public int PendingOrders { get; set; }
        public int PastDueOrders { get; set; }
        public int OrdersThisMonth { get; set; }
        public int OrdersTotal { get; set; }
    }

    public class PaginatedList<T> : Bookstore.Domain.Interfaces.IPaginatedList<T>
    {
        private IQueryable<T> source;
        private int _pageIndex;
        private int _pageSize;
        private int _totalCount;
        private int _totalPages;
        private List<T> _items;

        public PaginatedList(IQueryable<T> source, int pageIndex, int pageSize)
        {
            this.source = source;
            _pageIndex = pageIndex;
            _pageSize = pageSize;
            _items = new List<T>();
        }

        public int PageIndex => _pageIndex;
        public int PageSize => _pageSize;
        public int TotalCount => _totalCount;
        public int TotalPages => _totalPages;
        public List<T> Items => _items;
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        public async Task PopulateAsync()
        {
            _totalCount = await source.CountAsync();
            _totalPages = (int)Math.Ceiling(_totalCount / (double)_pageSize);

            var items = await source.Skip((_pageIndex - 1) * _pageSize)
                .Take(_pageSize).ToListAsync();

            _items.AddRange(items);
        }
    }
}

namespace Bookstore.Data.Repositories
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Bookstore.Domain.Orders.Order> Orders { get; set; }
        public DbSet<Bookstore.Domain.Orders.OrderItem> OrderItem { get; set; }
    }

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
                .Include("OrderItems.Book")
                .Include("OrderItems.Book.BookType")
                .Include("OrderItems.Book.Condition")
                .Include("OrderItems.Book.Genre")
                .Include("OrderItems.Book.Publisher")
                .SingleOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Bookstore.Domain.Orders.Order> GetAsync(int id, string sub)
        {
            return await dbContext.Orders.SingleOrDefaultAsync(x => x.Id == id && x.Customer.Sub == sub);
        }

        public async Task<IEnumerable<Bookstore.Data.Models.Book>> ListBestSellingBooksAsync(int count)
        {
            return await dbContext.OrderItem
                .GroupBy(x => x.BookId)
                .OrderByDescending(x => x.Count())
                .Select(x => x.FirstOrDefault().Book)
                .Take(count)
                .ToListAsync();
        }

        public async Task<OrderStatistics> GetStatisticsAsync()
        {
            var startOfMonth = DateTime.UtcNow.StartOfMonth();

            return await dbContext.Orders
                .GroupBy(x => 1)
                .Select(x => new OrderStatistics
                {
                    PendingOrders = x.Count(y => y.OrderStatus == OrderStatus.Pending),
                    PastDueOrders = x.Count(y => y.OrderStatus == OrderStatus.Ordered && y.DeliveryDate < DateTime.UtcNow),
                    OrdersThisMonth = x.Count(y => y.CreatedOn >= startOfMonth),
                    OrdersTotal = x.Count()
                }).SingleOrDefaultAsync();
        }

        async Task<Bookstore.Domain.Interfaces.IPaginatedList<Bookstore.Domain.Orders.Order>> IOrderRepository.ListAsync(OrderFilters filters, int pageIndex, int pageSize)
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

            var result = new PaginatedList<Bookstore.Domain.Orders.Order>(query, pageIndex, pageSize);

            await result.PopulateAsync();

            return result;
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