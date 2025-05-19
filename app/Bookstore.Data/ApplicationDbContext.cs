using System.ComponentModel.DataAnnotations.Schema;
using Bookstore.Domain;
using Bookstore.Domain.Models;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
// Order namespace has been moved, classes are now in Bookstore.Domain.Models namespace
// Cart namespace may have been moved or renamed
// using Bookstore.Domain.Cart;
// using Bookstore.Domain.Order;
// The Reference namespace doesn't exist in Bookstore.Domain
// ReferenceDataItem is likely in the main Bookstore.Domain namespace

namespace Bookstore.Data
{
    public partial class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(string connectionString) : base(connectionString) { }

        public DbSet<Address> Address { get; set; }

        public DbSet<Book> Book { get; set; }

        public DbSet<Bookstore.Domain.Customer> Customer { get; set; }

        public DbSet<Bookstore.Domain.Models.Order> Order { get; set; }

        public DbSet<ShoppingCart> ShoppingCart { get; set; }

        public DbSet<Bookstore.Domain.Models.OrderItem> OrderItem { get; set; }

        public DbSet<Bookstore.Domain.Offer> Offer { get; set; }

        public DbSet<Bookstore.Domain.ReferenceDataItem> ReferenceData { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Update to remove the pluralization to match the modern version
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();


            modelBuilder.Entity<Customer>().Property(x => x.Sub).HasColumnType("nvarchar").HasMaxLength(450);
            modelBuilder.Entity<Customer>().HasIndex(x => x.Sub).IsUnique();

            modelBuilder.Entity<Book>().HasRequired(x => x.Publisher).WithMany().HasForeignKey(x => x.PublisherId).WillCascadeOnDelete(false);
            modelBuilder.Entity<Book>().HasRequired(x => x.BookType).WithMany().HasForeignKey(x => x.BookTypeId).WillCascadeOnDelete(false);
            modelBuilder.Entity<Book>().HasRequired(x => x.Genre).WithMany().HasForeignKey(x => x.GenreId).WillCascadeOnDelete(false);
            modelBuilder.Entity<Book>().HasRequired(x => x.Condition).WithMany().HasForeignKey(x => x.ConditionId).WillCascadeOnDelete(false);

            modelBuilder.Entity<Bookstore.Domain.Offer>().HasRequired(x => x.Publisher).WithMany().HasForeignKey(x => x.PublisherId).WillCascadeOnDelete(false);
            modelBuilder.Entity<Bookstore.Domain.Offer>().HasRequired(x => x.BookType).WithMany().HasForeignKey(x => x.BookTypeId).WillCascadeOnDelete(false);
            modelBuilder.Entity<Bookstore.Domain.Offer>().HasRequired(x => x.Genre).WithMany().HasForeignKey(x => x.GenreId).WillCascadeOnDelete(false);
            modelBuilder.Entity<Bookstore.Domain.Offer>().HasRequired(x => x.Condition).WithMany().HasForeignKey(x => x.ConditionId).WillCascadeOnDelete(false);

            modelBuilder.Entity<Order>().HasRequired(x => x.Customer).WithMany().WillCascadeOnDelete(false);

            // Update the Refernce Data Table to Match the modern version
            modelBuilder.Entity<Bookstore.Domain.ReferenceDataItem>().ToTable("ReferenceData");

            modelBuilder.Entity<ShoppingCartItem>().HasKey(x => new { x.Id, x.ShoppingCartId });
            modelBuilder.Entity<ShoppingCartItem>().Property(x => x.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            Database.SetInitializer(new BookstoreDbInitializer());
        }
    }
}