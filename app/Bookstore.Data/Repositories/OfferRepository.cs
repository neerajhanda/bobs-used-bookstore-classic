using Amazon.Auth.AccessControlPolicy;
using Bookstore.Domain;
using Bookstore.Domain.Repositories;
using Bookstore.Domain.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

// Offer, OfferStatus, and OfferFilters types
namespace Bookstore.Domain.Models
{
    public class Offer : Bookstore.Domain.Entity
    {
        public string BookName { get; set; }
        public string Author { get; set; }
        public int? ConditionId { get; set; }
        public int? GenreId { get; set; }
        public OfferStatus OfferStatus { get; set; }
        public DateTime CreatedOn { get; set; }
        public Customer Customer { get; set; }
        public Condition Condition { get; set; }
        public Genre Genre { get; set; }
        public BookType BookType { get; set; }
        public Publisher Publisher { get; set; }
    }

    public enum OfferStatus
    {
        PendingApproval,
        Approved,
        Rejected
    }
}

namespace Bookstore.Domain.Repositories
{
    public class OfferFilters
    {
        public string Author { get; set; }
        public string BookName { get; set; }
        public int? ConditionId { get; set; }
        public int? GenreId { get; set; }
        public OfferStatus? OfferStatus { get; set; }
    }
}

namespace Bookstore.Data
{
    // DbSet<Offer> already defined in main ApplicationDbContext class
public partial class ApplicationDbContext : DbContext
{
    // Removed duplicate property definition
}

// These classes should be defined in the Bookstore.Domain project
// Keep using existing types from the Domain project

public class PaginatedList<T> : Bookstore.Domain.IPaginatedList<T> where T : Bookstore.Domain.Entity
    {
        public List<T> Items { get; private set; }
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }
        public int TotalCount { get; private set; }
        public bool HasPreviousPage => PageIndex > 0;
        public bool HasNextPage => PageIndex < TotalPages - 1;

        public PaginatedList(IQueryable<T> source, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalCount = source.Count();
            TotalPages = (int)Math.Ceiling(TotalCount / (double)pageSize);
            Items = source.Skip(pageIndex * pageSize).Take(pageSize).ToList();
        }

        public List<T> GetPageList(int pageIndex)
        {
            return Items;
        }
    }
}

namespace Bookstore.Data.Repositories
{
    public class OfferStatistics
    {
        public int PendingOffers { get; set; }
        public int OffersThisMonth { get; set; }
        public int OffersTotal { get; set; }
    }

    public class OfferRepository : Bookstore.Domain.Repositories.IOfferRepository
    {
        private readonly ApplicationDbContext dbContext;

        public OfferRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<OfferStatistics> GetStatisticsAsync()
        {
            var startOfMonth = DateTime.UtcNow.StartOfMonth();

            return await dbContext.Set<Bookstore.Domain.Models.Offer>()
                .GroupBy(x => 1)
                .Select(x => new OfferStatistics
                {
                    PendingOffers = x.Count(y => y.OfferStatus == OfferStatus.PendingApproval),
                    OffersThisMonth = x.Count(y => y.CreatedOn >= startOfMonth),
                    OffersTotal = x.Count()
                }).SingleOrDefaultAsync();
        }

        public async Task AddAsync(Bookstore.Domain.Models.Offer offer)
        {
            await Task.Run(() => dbContext.Set<Bookstore.Domain.Models.Offer>().Add(offer));
        }

        public Task<Bookstore.Domain.Models.Offer> GetAsync(int id)
        {
            return dbContext.Set<Bookstore.Domain.Models.Offer>().Include(x => x.Customer).SingleOrDefaultAsync(x => x.Id == id);
        }

    public async Task<Bookstore.Domain.IPaginatedList<Bookstore.Domain.Models.Offer>> ListAsync(OfferFilters filters, int pageIndex, int pageSize)
        {
            var query = dbContext.Set<Bookstore.Domain.Models.Offer>().AsQueryable();

            if (!string.IsNullOrWhiteSpace(filters.Author))
            {
                query = query.Where(x => x.Author.Contains(filters.Author));
            }

            if (!string.IsNullOrWhiteSpace(filters.BookName))
            {
                query = query.Where(x => x.BookName.Contains(filters.BookName));
            }

            if (filters.ConditionId.HasValue)
            {
                query = query.Where(x => x.ConditionId == filters.ConditionId);
            }

            if (filters.GenreId.HasValue)
            {
                query = query.Where(x => x.GenreId == filters.GenreId);
            }

            if (filters.OfferStatus.HasValue)
            {
                query = query.Where(x => x.OfferStatus == filters.OfferStatus);
            }

            query = query.Include(x => x.Customer)
                .Include(x => x.Condition)
                .Include(x => x.Genre);



            // Using explicit cast to interface to avoid constructor conflicts
            // The PaginatedList constructor should handle populating the data internally
            var result = (Bookstore.Domain.IPaginatedList<Bookstore.Domain.Models.Offer>)new Bookstore.Data.PaginatedList<Bookstore.Domain.Models.Offer>(query, pageIndex, pageSize);

            return result;
        }

        public async Task<IEnumerable<Bookstore.Domain.Models.Offer>> ListAsync(string sub)
        {
            return await dbContext.Set<Bookstore.Domain.Models.Offer>()
                .Include(x => x.BookType)
                .Include(x => x.Genre)
                .Include(x => x.Condition)
                .Include(x => x.Publisher)
                .Where(x => x.Customer.Sub == sub)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }
    }
}