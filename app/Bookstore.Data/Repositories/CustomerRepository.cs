using Bookstore.Domain;
using Bookstore.Domain.Models;
using Bookstore.Domain.Interfaces;
using System.Linq;
using System.Data.Entity;
using System.Threading.Tasks;
using System;

namespace Bookstore.Data.Repositories
{
    public interface ICustomerRepository
    {
        Task AddAsync(Customer customer);
        Task<Customer> GetAsync(int id);
        Task<Customer> GetAsync(string sub);
        Task SaveChangesAsync();
    }

    public class CustomerRepository : ICustomerRepository
    {
        private readonly ApplicationDbContext dbContext;

        public CustomerRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task AddAsync(Customer customer)
        {
            await Task.Run(() => dbContext.Customers.Add(customer));
        }

        public async Task<Customer> GetAsync(int id)
        {
            return await dbContext.Customers.FindAsync(id);
        }

        public async Task<Customer> GetAsync(string sub)
        {
            return await dbContext.Customers.SingleOrDefaultAsync(x => x.Sub == sub);
        }

        public async Task SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }
    }
}