using Bookstore.Domain;
using Bookstore.Domain.Books;
using Bookstore.Domain.Repositories;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Bookstore.Data
{
    public class BookStatistics
    {
        public int LowStock { get; set; }
        public int OutOfStock { get; set; }
        public int StockTotal { get; set; }
    }
}

namespace Bookstore.Data.Repositories
{
    public interface IPaginatedList<T> : IEnumerable<T>
    {
        int PageIndex { get; }
        int PageSize { get; }
        int TotalCount { get; }
        int TotalPages { get; }
        bool HasPreviousPage { get; }
        bool HasNextPage { get; }
    }

    public class PaginatedList<T> : IPaginatedList<T>
    {
        private readonly IQueryable<T> _source;
        private readonly List<T> _items;

        public PaginatedList(IQueryable<T> source, int pageIndex, int pageSize)
        {
            _source = source;
            PageIndex = pageIndex;
            PageSize = pageSize;
            _items = new List<T>();
        }

        public int PageIndex { get; }
        public int PageSize { get; }
        public int TotalCount { get; private set; }
        public int TotalPages { get; private set; }
        public bool HasPreviousPage => (PageIndex > 0);
        public bool HasNextPage => (PageIndex < TotalPages - 1);

        public async Task PopulateAsync()
        {
            TotalCount = await _source.CountAsync();
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);

            _items.Clear();
            _items.AddRange(await _source.Skip(PageIndex * PageSize).Take(PageSize).ToListAsync());
        }

        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
    }
}

// Interface IBookRepository should be defined in the Domain project
// Adding temporary interface definition until it's available in Domain project
namespace Bookstore.Domain.Books
{
    public class Book
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Author { get; set; }
        public string ISBN { get; set; }
        public string CoverImageUrl { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int ConditionId { get; set; }
        public int BookTypeId { get; set; }
        public int GenreId { get; set; }
        public int PublisherId { get; set; }

        public dynamic Genre { get; set; }
        public dynamic Publisher { get; set; }
        public dynamic BookType { get; set; }
        public dynamic Condition { get; set; }

        public const int LowBookThreshold = 5;
    }

    public class BookFilters
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public int? ConditionId { get; set; }
        public int? BookTypeId { get; set; }
        public int? GenreId { get; set; }
        public int? PublisherId { get; set; }
        public bool LowStock { get; set; }
    }
}

namespace Bookstore.Domain.Repositories
{
    public interface IBookRepository
    {
        Task<Bookstore.Domain.Books.Book> GetAsync(int id);
        Task<IPaginatedList<Bookstore.Domain.Books.Book>> ListAsync(Bookstore.Domain.Books.BookFilters filters, int pageIndex, int pageSize);
        Task<IPaginatedList<Bookstore.Domain.Books.Book>> ListAsync(string searchString, string sortBy, int pageIndex, int pageSize);
        Task AddAsync(Bookstore.Domain.Books.Book book);
        Task UpdateAsync(Bookstore.Domain.Books.Book book);
        Task SaveChangesAsync();
        Task<Bookstore.Data.BookStatistics> GetStatisticsAsync();
    }
}

namespace Bookstore.Data.Repositories
{
public class BookRepository : IBookRepository
    {
        private readonly ApplicationDbContext dbContext;

        public BookRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Bookstore.Domain.Books.Book> GetAsync(int id)
        {
            return await dbContext.Set<Bookstore.Domain.Books.Book>()
                .Include("Genre")
                .Include("Publisher")
                .Include("BookType")
                .Include("Condition")
                .SingleAsync(x => x.Id == id);
        }

        public async Task<IPaginatedList<Bookstore.Domain.Books.Book>> ListAsync(Bookstore.Domain.Books.BookFilters filters, int pageIndex, int pageSize)
        {
            var query = dbContext.Set<Bookstore.Domain.Books.Book>().AsQueryable();

            if (!string.IsNullOrWhiteSpace(filters.Name))
            {
                query = query.Where(x => x.Name.Contains(filters.Name));
            }

            if (!string.IsNullOrWhiteSpace(filters.Author))
            {
                query = query.Where(x => x.Author.Contains(filters.Author));
            }

            if (filters.ConditionId.HasValue)
            {
                query = query.Where(x => x.ConditionId == filters.ConditionId);
            }

            if (filters.BookTypeId.HasValue)
            {
                query = query.Where(x => x.BookTypeId == filters.BookTypeId);
            }

            if (filters.GenreId.HasValue)
            {
                query = query.Where(x => x.GenreId == filters.GenreId);
            }

            if (filters.PublisherId.HasValue)
            {
                query = query.Where(x => x.PublisherId == filters.PublisherId);
            }

            if (filters.LowStock)
            {
                query = query.Where(x => x.Quantity <= Bookstore.Domain.Books.Book.LowBookThreshold);
            }

            query = query
                .Include(x => x.Genre)
                .Include(x => x.Publisher)
                .Include(x => x.BookType)
                .Include(x => x.Condition);

            var result = new PaginatedList<Bookstore.Domain.Books.Book>(query, pageIndex, pageSize);

            await result.PopulateAsync();

            return result;
        }

        public async Task<IPaginatedList<Bookstore.Domain.Books.Book>> ListAsync(string searchString, string sortBy, int pageIndex, int pageSize)
        {
            var query = dbContext.Set<Bookstore.Domain.Books.Book>().AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(x => x.Name.Contains(searchString) ||
                                         x.Genre.Text.Contains(searchString) ||
                                         x.BookType.Text.Contains(searchString) ||
                                         x.ISBN.Contains(searchString) ||
                                         x.Publisher.Text.Contains(searchString));
            };

            switch (sortBy)
            {
                case "Name":
                    query = query.OrderBy(x => x.Name);
                    break;

                case "PriceAsc":
                    query = query.OrderBy(x => x.Price);
                    break;

                case "PriceDesc":
                    query = query.OrderByDescending(x => x.Price);
                    break;

                default:
                    query.OrderBy(x => x.Name);
                    break;
            }

            var result = new PaginatedList<Bookstore.Domain.Books.Book>(query, pageIndex, pageSize);

            await result.PopulateAsync();

            return result;
        }

        public async Task AddAsync(Bookstore.Domain.Books.Book book)
        {
            await Task.Run(() => dbContext.Set<Bookstore.Domain.Books.Book>().Add(book));
        }

        public async Task UpdateAsync(Bookstore.Domain.Books.Book book)
        {
            var existing = await dbContext.Set<Bookstore.Domain.Books.Book>().FindAsync(book.Id);

            dbContext.Entry(existing).CurrentValues.SetValues(book);

            if (string.IsNullOrWhiteSpace(book.CoverImageUrl))
            {
                dbContext.Entry(existing).Property(x => x.CoverImageUrl).IsModified = false;
            }
        }

        public async Task SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }

        public async Task<Bookstore.Data.BookStatistics> GetStatisticsAsync()
        {
            return await dbContext.Set<Bookstore.Domain.Books.Book>()
                .GroupBy(x => 1)
                .Select(x => new Bookstore.Data.BookStatistics
                {
                    LowStock = x.Count(y => y.Quantity > 0 && y.Quantity < Bookstore.Domain.Books.Book.LowBookThreshold),
                    OutOfStock = x.Count(y => y.Quantity == 0),
                    StockTotal = x.Count()
                }).SingleOrDefaultAsync();
        }
    }
}