using Bookstore.Domain;
using Bookstore.Domain.Interfaces;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Linq;
using Bookstore.Data.Interfaces;
using Bookstore.Domain.Models; // Most likely the entities were moved to Models namespace

namespace Bookstore.Data.Interfaces
{
    public interface IShoppingCartRepository
    {
        Task AddAsync(Bookstore.Domain.Models.ShoppingCart shoppingCart);
        Task<Bookstore.Domain.Models.ShoppingCart> GetAsync(string correlationId);
        Task SaveChangesAsync();
    }
}

namespace Bookstore.Data.Repositories
{
    public class ShoppingCartRepository : IShoppingCartRepository
    {
        private readonly ApplicationDbContext dbContext;

        public ShoppingCartRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task AddAsync(Bookstore.Domain.Models.ShoppingCart shoppingCart)
        {
            await Task.Run(() => dbContext.Set<Bookstore.Domain.Models.ShoppingCart>().Add(shoppingCart));
        }

        public async Task<Bookstore.Domain.Models.ShoppingCart> GetAsync(string correlationId)
        {
            return await dbContext.Set<Bookstore.Domain.Models.ShoppingCart>()
                .Include(x => x.ShoppingCartItems)
                .Include(x => x.ShoppingCartItems.Select(y => y.Book))
                .SingleOrDefaultAsync(x => x.CorrelationId == correlationId);
        }

        public async Task SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }
    }
}