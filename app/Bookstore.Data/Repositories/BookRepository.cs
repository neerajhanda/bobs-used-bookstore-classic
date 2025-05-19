using Bookstore.Domain;
using Bookstore.Domain.Models;
using Bookstore.Domain.Repositories;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Bookstore.Domain.Repositories
{
    public interface IPaginatedList<T>
    {
        int PageIndex { get; }
        int PageSize { get; }
        int TotalCount { get; }
        int TotalPages { get; }
        IList<T> Items { get; }
        bool HasPreviousPage { get; }
        bool HasNextPage { get; }
        Task PopulateAsync();
    }
}

namespace Bookstore.Domain
{
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

namespace Bookstore.Domain.Models
{
    public class Book
    {
        public static int LowBookThreshold { get; } = 5;
        public int Id { get; set; }
        public string Name { get; set; }
        public string Author { get; set; }
        public string ISBN { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string CoverImageUrl { get; set; }
        public int GenreId { get; set; }
        public int PublisherId { get; set; }
        public int BookTypeId { get; set; }
        public int ConditionId { get; set; }
        public virtual Genre Genre { get; set; }
        public virtual Publisher Publisher { get; set; }
        public virtual BookType BookType { get; set; }
        public virtual Condition Condition { get; set; }
    }

    public class Genre
    {
        public int Id { get; set; }
        public string Text { get; set; }
    }

    public class Publisher
    {
        public int Id { get; set; }
        public string Text { get; set; }
    }

    public class BookType
    {
        public int Id { get; set; }
        public string Text { get; set; }
    }

    public class Condition
    {
        public int Id { get; set; }
        public string Text { get; set; }
    }

    public class BookStatistics
    {
        public int LowStock { get; set; }
        public int OutOfStock { get; set; }
        public int StockTotal { get; set; }
    }
}

// Define local interface if it's missing from the Domain project
namespace Bookstore.Domain.Repositories
{
    public interface IBookRepository
    {
        Task<Book> GetAsync(int id);
        Task<Bookstore.Domain.Repositories.IPaginatedList<Book>> ListAsync(BookFilters filters, int pageIndex, int pageSize);
        Task<Bookstore.Domain.Repositories.IPaginatedList<Book>> ListAsync(string searchString, string sortBy, int pageIndex, int pageSize);
        Task AddAsync(Book book);
        Task UpdateAsync(Book book);
        Task SaveChangesAsync();
        Task<BookStatistics> GetStatisticsAsync();
    }
}

namespace Bookstore.Data.Repositories
{
public class PaginatedList<T> : Bookstore.Domain.Repositories.IPaginatedList<T>
    {
        private readonly IQueryable<T> _query;
        private readonly int _pageIndex;
        private readonly int _pageSize;
        private int _totalCount;
        private int _totalPages;
        private IList<T> _items;

        public PaginatedList(IQueryable<T> query, int pageIndex, int pageSize)
        {
            _query = query;
            _pageIndex = pageIndex;
            _pageSize = pageSize;
            _items = new List<T>();
        }

        public async Task PopulateAsync()
        {
            _totalCount = await _query.CountAsync();
            _totalPages = (int)Math.Ceiling(_totalCount / (double)_pageSize);

            var startRow = (_pageIndex - 1) * _pageSize;
            _items = await _query.Skip(startRow).Take(_pageSize).ToListAsync();
        }

        public int PageIndex => _pageIndex;
        public int PageSize => _pageSize;
        public int TotalCount => _totalCount;
        public int TotalPages => _totalPages;
        public IList<T> Items => _items;
        public bool HasPreviousPage => _pageIndex > 1;
        public bool HasNextPage => _pageIndex < _totalPages;
    }

    public class BookRepository : Bookstore.Domain.Repositories.IBookRepository
    {
        private readonly ApplicationDbContext dbContext;

        public BookRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Bookstore.Domain.Models.Book> GetAsync(int id)
        {
            return await dbContext.Books
                .Include("Genre")
                .Include("Publisher")
                .Include("BookType")
                .Include("Condition")
                .SingleAsync(x => x.Id == id);
        }

    public async Task<Bookstore.Domain.Repositories.IPaginatedList<Bookstore.Domain.Models.Book>> ListAsync(BookFilters filters, int pageIndex, int pageSize)
    {
        var query = dbContext.Books.AsQueryable();

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
            query = query.Where(x => x.Quantity <= Bookstore.Domain.Models.Book.LowBookThreshold);
        }

        query = query
            .Include(x => x.Genre)
            .Include(x => x.Publisher)
            .Include(x => x.BookType)
            .Include(x => x.Condition);

        var result = new PaginatedList<Bookstore.Domain.Models.Book>(query, pageIndex, pageSize);

        await result.PopulateAsync();

        return result;
    }

        public async Task<Bookstore.Domain.Repositories.IPaginatedList<Bookstore.Domain.Models.Book>> ListAsync(string searchString, string sortBy, int pageIndex, int pageSize)
        {
            var query = dbContext.Books.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(x => x.Name.Contains(searchString) ||
                                         x.Genre.Text.Contains(searchString) ||
                                         x.BookType.Text.Contains(searchString) ||
                                         x.ISBN.Contains(searchString) ||
                                         x.Publisher.Text.Contains(searchString));
            }

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
                    query = query.OrderBy(x => x.Name);
                    break;
            }

            query = query
                .Include(x => x.Genre)
                .Include(x => x.Publisher)
                .Include(x => x.BookType)
                .Include(x => x.Condition);

            var result = new PaginatedList<Bookstore.Domain.Models.Book>(query, pageIndex, pageSize);

            await result.PopulateAsync();

            return result;
        }

        public async Task AddAsync(Bookstore.Domain.Models.Book book)
        {
            await Task.Run(() => dbContext.Books.Add(book));
        }

        public async Task UpdateAsync(Bookstore.Domain.Models.Book book)
        {
            var existing = await dbContext.Books.FindAsync(book.Id);

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

        public async Task<Bookstore.Domain.Models.BookStatistics> GetStatisticsAsync()
        {
            return await dbContext.Books
                .GroupBy(x => 1)
                .Select(x => new Bookstore.Domain.Models.BookStatistics
                {
                    LowStock = x.Count(y => y.Quantity > 0 && y.Quantity < Bookstore.Domain.Models.Book.LowBookThreshold),
                    OutOfStock = x.Count(y => y.Quantity == 0),
                    StockTotal = x.Count()
                }).SingleOrDefaultAsync();
        }
    }
}