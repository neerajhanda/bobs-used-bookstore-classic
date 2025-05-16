using Amazon.Auth.AccessControlPolicy;
using Bookstore.Domain;
using Bookstore.Domain.Repositories;
using Bookstore.Domain.Interfaces;
using Bookstore.Domain.Repositories.Interfaces;
using Bookstore.Domain.Repositories.Interfaces.Offers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Bookstore.Data.Repositories
{
    public class OfferRepository : IOfferRepository
    {
        private readonly ApplicationDbContext dbContext;

        public OfferRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Bookstore.Domain.OfferStatistics> GetStatisticsAsync()
        {
            var startOfMonth = DateTime.UtcNow.StartOfMonth();

        return await dbContext.Set<Bookstore.Domain.Offer>()
                .GroupBy(x => 1)
                .Select(x => new Bookstore.Domain.OfferStatistics
                {
                    PendingOffers = x.Count(y => y.OfferStatus == OfferStatus.PendingApproval),
                    OffersThisMonth = x.Count(y => y.CreatedOn >= startOfMonth),
                    OffersTotal = x.Count()
                }).SingleOrDefaultAsync();
        }

public async Task AddAsync(Bookstore.Domain.Offer offer)
{
    await Task.Run(() => dbContext.Set<Bookstore.Domain.Offer>().Add(offer));
}

public Task<Bookstore.Domain.Offer> GetAsync(int id)
{
    return dbContext.Set<Bookstore.Domain.Offer>().Include(x => x.Customer).SingleOrDefaultAsync(x => x.Id == id);
}

public async Task<IList<Bookstore.Domain.Offer>> ListAsync(Bookstore.Domain.OfferFilters filters, int pageIndex, int pageSize)
        {
            var query = dbContext.Set<Bookstore.Domain.Offer>().AsQueryable();

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

            var pagedQuery = query.Skip(pageIndex * pageSize).Take(pageSize);
            return await pagedQuery.ToListAsync();
        }

public async Task<IEnumerable<Bookstore.Domain.Offer>> ListAsync(string sub)
        {
        return await dbContext.Set<Bookstore.Domain.Offer>()
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