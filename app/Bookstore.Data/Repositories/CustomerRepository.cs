using System.Data.Entity;
using System.Threading.Tasks;
using System.Linq;
using System;
using Bookstore.Domain;
using Bookstore.Domain.Models;

namespace Bookstore.Domain.Interfaces
{
    public interface ICustomerRepository
    {
        Task AddAsync(Bookstore.Domain.Models.Customer customer);
        Task<Bookstore.Domain.Models.Customer> GetAsync(int id);
        Task<Bookstore.Domain.Models.Customer> GetAsync(string sub);
        Task SaveChangesAsync();
    }
}

namespace Bookstore.Data.Repositories
{

    public class CustomerRepository : Bookstore.Domain.Interfaces.ICustomerRepository
    {
        private readonly ApplicationDbContext dbContext;

        public CustomerRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task AddAsync(Bookstore.Domain.Models.Customer customer)
        {
            dbContext.Customer.Add(customer);
            await Task.CompletedTask;
        }

    public async Task<Bookstore.Domain.Models.Customer> GetAsync(int id)
    {
        return await dbContext.Customer.FindAsync(id);
    }

    public async Task<Bookstore.Domain.Models.Customer> GetAsync(string sub)
    {
        return await dbContext.Customer.FirstOrDefaultAsync(x => x.Sub == sub);
    }

        public async Task SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }
    }
}