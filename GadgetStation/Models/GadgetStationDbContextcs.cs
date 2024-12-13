using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using System.Collections.Generic;

namespace GadgetStation.Models
{
    public class GadgetStationDbContextcs : DbContext
    {
        public GadgetStationDbContextcs(DbContextOptions<GadgetStationDbContextcs> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Product and Category relationship (many-to-one)
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order and OrderDetail relationship (one-to-many)
            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderDetails)
                .WithOne(od => od.Order)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure other relationships and constraints
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(p => p.Price)
                      .HasColumnType("decimal(18,2)")
                      .IsRequired();

                entity.HasCheckConstraint("CK_Product_Price", "[Price] > 0");

                entity.Property(p => p.Stock)
                      .IsRequired();

                entity.HasCheckConstraint("CK_Product_Stock", "[Stock] >= 0");
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.Property(o => o.TotalAmount)
                      .HasColumnType("decimal(18,2)")
                      .IsRequired();
            });

            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.Property(od => od.Price)
                      .HasColumnType("decimal(18,2)")
                      .IsRequired();
            });

            // Add Seed Data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Categories
            modelBuilder.Entity<Category>().HasData(
                new Category
                {
                    CategoryId = 1,
                    Name = "Smartphones"
                },
                new Category
                {
                    CategoryId = 2,
                    Name = "Laptops"
                }
            );

            // Seed Users
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = 1,
                    Name = "Admin User",
                    Email = "admin@store.com",
                    Password = "admin123",
                    Role = "Admin"
                },
                new User
                {
                    UserId = 2,
                    Name = "Customer User",
                    Email = "customer@store.com",
                    Password = "customer123",
                    Role = "Customer"
                }
            );

            // Seed Products
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    ProductId = 1,
                    Name = "Smartphone",
                    Description = "High-performance smartphone with cutting-edge features.",
                    Price = 699.99m,
                    Stock = 50,
                    ImageUrl = "/images/products/smartphone.jpg",
                    CategoryId = 1
                },
                new Product
                {
                    ProductId = 2,
                    Name = "Laptop",
                    Description = "Lightweight and powerful laptop for work and gaming.",
                    Price = 1299.99m,
                    Stock = 30,
                    ImageUrl = "/images/products/laptop.jpg",
                    CategoryId = 2
                }
            );
        }
    }
}
