using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenericHostApp.Model
{
    public class BasketContext : DbContext
    {
        public DbSet<Basket> Baskets { get; set; }
        public DbSet<BasketItem> BasketItems { get; set; }

        public BasketContext(DbContextOptions<BasketContext> options) : base(options) {}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Basket>().SeedData(new Basket
            {
                CustomerId = 1,
                Id = 1
            });
            modelBuilder.Entity<BasketItem>().SeedData(new
            {
                Id = 1,
                Cost = 5m,
                Description = "Something that originally cost $5.",
                BasketId = 1
            });
        }
    }
}
