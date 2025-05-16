using Bookstore.Domain;
using Bookstore.Domain.Interfaces;
using Bookstore.Domain.Interfaces.Repositories;
using Bookstore.Domain.Models;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Entity.Spatial;
using Bookstore.Data.Models;

namespace Bookstore.Data.Repositories
{
    public class AddressRepository : IAddressRepository
    {
        private readonly ApplicationDbContext dbContext;

        public AddressRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

    public async Task DeleteAsync(string sub, int id)
    {
        var address = await dbContext.Set<Models.Address>().SingleOrDefaultAsync(x => x.Customer.Sub == sub && x.Id == id);

        if (address == null) return;

        address.IsActive = false;
    }

    public async Task<Models.Address> GetAsync(string sub, int id)
    {
        return await dbContext.Set<Models.Address>().SingleOrDefaultAsync(x => x.Customer.Sub == sub && x.Id == id && x.IsActive == true);
    }

    public async Task<IEnumerable<Models.Address>> ListAsync(string sub)
    {
        return await dbContext.Set<Models.Address>().Where(x => x.Customer.Sub == sub && x.IsActive == true).ToListAsync();
    }

    public async Task AddAsync(Models.Address address)
    {
        await Task.Run(() => dbContext.Set<Models.Address>().Add(address));
    }

        public async Task SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }
    }
}