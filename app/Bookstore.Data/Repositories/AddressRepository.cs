using Bookstore.Domain;
using Bookstore.Domain.Models;
using Bookstore.Domain.Repositories;
using Bookstore.Domain.Interfaces;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using Bookstore.Data.Models;
using DataAddress = Bookstore.Data.Models.Address;

namespace Bookstore.Data.Repositories
{
    public class AddressRepository : Bookstore.Domain.Repositories.IAddressRepository
    {
        private readonly ApplicationDbContext dbContext;

        public AddressRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task DeleteAsync(string sub, int id)
        {
            var address = await dbContext.Addresses.SingleOrDefaultAsync(x => x.Customer.Sub == sub && x.Id == id);

            if (address == null) return;

            address.IsActive = false;
        }

        public async Task<Bookstore.Domain.Models.Address> GetAsync(string sub, int id)
        {
            var dataAddress = await dbContext.Addresses.SingleOrDefaultAsync(x => x.Customer.Sub == sub && x.Id == id && x.IsActive == true);
            return dataAddress != null ? MapToModelAddress(dataAddress) : null;
        }

        public async Task<IEnumerable<Bookstore.Domain.Models.Address>> ListAsync(string sub)
        {
            var addresses = await dbContext.Addresses.Where(x => x.Customer.Sub == sub && x.IsActive == true).ToListAsync();
            return addresses.Select(MapToModelAddress);
        }

        public async Task AddAsync(Bookstore.Domain.Models.Address address)
        {
            var dataAddress = MapToDataAddress(address);
            await Task.Run(() => dbContext.Addresses.Add(dataAddress));
        }

        private Bookstore.Domain.Models.Address MapToModelAddress(DataAddress dataAddress)
        {
            // Map Data.Models.Address to Domain.Models.Address
            // This is a simplified implementation - adjust properties as needed
            return new Bookstore.Domain.Models.Address
            {
                // Map properties from dataAddress to domain address
                Id = dataAddress.Id,
                // Add other properties as needed
            };
        }

        private DataAddress MapToDataAddress(Bookstore.Domain.Models.Address domainAddress)
        {
            // Map Domain.Models.Address to Data.Models.Address
            // This is a simplified implementation - adjust properties as needed
            return new DataAddress
            {
                // Map properties from domainAddress to data address
                Id = domainAddress.Id,
                IsActive = true,
                // Add other properties as needed
            };
        }

        public async Task SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }
    }
}