using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Bookstore.Domain;
using Bookstore.Domain.Interfaces;
using Bookstore.Domain.Repositories;
using Bookstore.Domain.Repositories.Interfaces;

namespace Bookstore.Domain
{
    public class ReferenceDataItem
    {
        public int Id { get; set; }
        public int DataType { get; set; }
        public string Name { get; set; }
    }

    public class ReferenceDataFilters
    {
        public int? ReferenceDataType { get; set; }
    }

    public interface IPaginatedList<T>
    {
        List<T> Items { get; }
        int PageIndex { get; }
        int PageSize { get; }
        int TotalCount { get; }
        int TotalPages { get; }
        bool HasPreviousPage { get; }
        bool HasNextPage { get; }
    }
}

// Define the missing interface
namespace Bookstore.Domain.Repositories.Interfaces
{
    public interface IReferenceDataRepository
    {
        Task AddAsync(Bookstore.Domain.ReferenceDataItem item);
        Task<Bookstore.Domain.ReferenceDataItem> GetAsync(int id);
        Task<IEnumerable<Bookstore.Domain.ReferenceDataItem>> FullListAsync();
        Task<IPaginatedList<Bookstore.Domain.ReferenceDataItem>> ListAsync(Bookstore.Domain.ReferenceDataFilters filters, int pageIndex, int pageSize);
        Task SaveChangesAsync();
    }
}

namespace Bookstore.Data.Repositories
{
    // Renamed to avoid duplicate definition
    public class ReferenceDataPaginatedList<T> : List<T>, IPaginatedList<T>
    {
        public int PageIndex { get; private set; }
        public int PageSize { get; private set; }
        public int TotalCount { get; private set; }
        public int TotalPages { get; private set; }
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        private IQueryable<T> _source;

        public ReferenceDataPaginatedList(IQueryable<T> source, int pageIndex, int pageSize)
        {
            _source = source;
            PageIndex = pageIndex;
            PageSize = pageSize;
        }

        public async Task PopulateAsync()
        {
            TotalCount = await Task.Run(() => _source.Count());
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);

            var items = await Task.Run(() => _source
                .Skip((PageIndex - 1) * PageSize)
                .Take(PageSize)
                .ToList());

            this.AddRange(items);
        }
    }

    public class ReferenceDataRepository : Bookstore.Domain.Repositories.Interfaces.IReferenceDataRepository
    {
        private readonly ApplicationDbContext dbContext;

        public ReferenceDataRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task AddAsync(Bookstore.Domain.ReferenceDataItem item)
        {
            await Task.Run(() => dbContext.ReferenceData.Add(item));
        }

        public async Task<Bookstore.Domain.ReferenceDataItem> GetAsync(int id)
        {
            return await dbContext.ReferenceData.FindAsync(id);
        }

        public async Task<IEnumerable<Bookstore.Domain.ReferenceDataItem>> FullListAsync()
        {
            return await dbContext.ReferenceData.ToListAsync();
        }

        public async Task<IPaginatedList<Bookstore.Domain.ReferenceDataItem>> ListAsync(Bookstore.Domain.ReferenceDataFilters filters, int pageIndex, int pageSize)
        {
            var query = dbContext.ReferenceData.AsQueryable();

            if (filters.ReferenceDataType.HasValue)
            {
                query = query.Where(x => x.DataType == filters.ReferenceDataType.Value);
            }

            var result = new ReferenceDataPaginatedList<Bookstore.Domain.ReferenceDataItem>(query, pageIndex, pageSize);

            await result.PopulateAsync();

            return result;
        }

        public async Task SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }
    }
}