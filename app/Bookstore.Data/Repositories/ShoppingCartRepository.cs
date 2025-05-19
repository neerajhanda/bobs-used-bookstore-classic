using Bookstore.Domain;
using Bookstore.Domain.Interfaces;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Linq;
using System.Collections.Generic;

namespace Bookstore.Domain
{
    public class ShoppingCart
    {
        public string CorrelationId { get; set; }
        public ICollection<ShoppingCartItem> ShoppingCartItems { get; set; } = new List<ShoppingCartItem>();
    }

    public class ShoppingCartItem
    {
        public Bookstore.Domain.Models.Book Book { get; set; }
    }
}

namespace Bookstore.Domain.Interfaces
{
    public interface IShoppingCartRepository
    {
        Task AddAsync(ShoppingCart shoppingCart);
        Task<ShoppingCart> GetAsync(string correlationId);
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

        async Task Bookstore.Domain.Interfaces.IShoppingCartRepository.AddAsync(ShoppingCart shoppingCart)
        {
            await Task.Run(() => dbContext.ShoppingCart.Add(shoppingCart));
        }

        async Task<ShoppingCart> Bookstore.Domain.Interfaces.IShoppingCartRepository.GetAsync(string correlationId)
        {
            return await dbContext.ShoppingCart
                .Include(x => x.ShoppingCartItems)
                .Include(x => x.ShoppingCartItems.Select(y => y.Book))
                .SingleOrDefaultAsync(x => x.CorrelationId == correlationId);
        }

        async Task Bookstore.Domain.Interfaces.IShoppingCartRepository.SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }
    }
}