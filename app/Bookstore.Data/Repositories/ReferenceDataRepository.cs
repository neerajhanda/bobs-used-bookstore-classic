using Bookstore.Domain;
using Bookstore.Domain.ReferenceData;
using Bookstore.Domain.Repositories;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Bookstore.Domain.Common
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
    }
}

namespace Bookstore.Domain.ReferenceData
{
    public class ReferenceDataFilters
    {
        public int? ReferenceDataType { get; set; }
    }

    public class ReferenceDataItem
    {
        public int Id { get; set; }
        public int DataType { get; set; }
        public string Value { get; set; }
    }
}

namespace Bookstore.Data
{
    // ReferenceData property is already defined elsewhere in another partial class definition
    public partial class ApplicationDbContext
    {
    }
}

namespace Bookstore.Domain.Repositories
{
    public interface IReferenceDataRepository
    {
        Task AddAsync(Bookstore.Domain.ReferenceData.ReferenceDataItem item);
        Task<Bookstore.Domain.ReferenceData.ReferenceDataItem> GetAsync(int id);
        Task<IEnumerable<Bookstore.Domain.ReferenceData.ReferenceDataItem>> FullListAsync();
        Task<Bookstore.Domain.Common.IPaginatedList<Bookstore.Domain.ReferenceData.ReferenceDataItem>> ListAsync(ReferenceDataFilters filters, int pageIndex, int pageSize);
        Task SaveChangesAsync();
    }
}

namespace Bookstore.Data.Repositories
{
    public class ReferenceDataRepository : IReferenceDataRepository
    {
        private readonly ApplicationDbContext dbContext;

        public ReferenceDataRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task AddAsync(Bookstore.Domain.ReferenceData.ReferenceDataItem item)
        {
            await Task.Run(() => dbContext.ReferenceData.Add(item));
        }

        public async Task<Bookstore.Domain.ReferenceData.ReferenceDataItem> GetAsync(int id)
        {
            return await dbContext.ReferenceData.FindAsync(id);
        }

        public async Task<IEnumerable<Bookstore.Domain.ReferenceData.ReferenceDataItem>> FullListAsync()
        {
            return await dbContext.ReferenceData.ToListAsync();
        }

        public async Task<Bookstore.Domain.Common.IPaginatedList<Bookstore.Domain.ReferenceData.ReferenceDataItem>> ListAsync(ReferenceDataFilters filters, int pageIndex, int pageSize)
        {
            var query = dbContext.ReferenceData.AsQueryable();

            if (filters.ReferenceDataType.HasValue)
            {
                query = query.Where(x => x.DataType == filters.ReferenceDataType.Value);
            }

            var result = new PaginatedListImplementation<Bookstore.Domain.ReferenceData.ReferenceDataItem>(query, pageIndex, pageSize);

            await result.PopulateAsync();

            return result;
        }

        public async Task SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }
    }

    // Using the existing implementation from another file
    internal class PaginatedListImplementation<T> : Bookstore.Domain.Common.IPaginatedList<T>
    {
        private readonly IQueryable<T> _source;
        public List<T> Items { get; private set; } = new List<T>();
        public int PageIndex { get; private set; }
        public int PageSize { get; private set; }
        public int TotalCount { get; private set; }
        public int TotalPages { get; private set; }

        public PaginatedListImplementation(IQueryable<T> source, int pageIndex, int pageSize)
        {
            _source = source;
            PageIndex = pageIndex;
            PageSize = pageSize;
        }

        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        public async Task PopulateAsync()
        {
            TotalCount = await _source.CountAsync();
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);

            Items = await _source
                .Skip((PageIndex - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }
    }
}