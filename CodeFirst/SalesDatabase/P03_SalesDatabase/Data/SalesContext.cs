using Microsoft.EntityFrameworkCore;
using P03_SalesDatabase.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace P03_SalesDatabase.Data
{
    public class SalesContext : DbContext
    {
        public SalesContext()
        {
        }

        public SalesContext(DbContextOptions options) : base(options)
        {
        }

        DbSet<Product> Products { get; set; }
        DbSet<Customer> Customers { get; set; }
        DbSet<Store> Stores { get; set; }
        DbSet<Sale> Sales { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder
                    .UseSqlServer("Server=.;Database=SalesDatabase;Integrated Security=true;");
            }

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Sale>(s => s.HasKey(s => s.SaleId));

            modelBuilder.Entity<Sale>(s =>
            s.HasOne(p => p.Product)
            .WithMany(s => s.Sales)
            .HasForeignKey(s => s.ProductId).OnDelete(DeleteBehavior.Restrict)
            );

            modelBuilder.Entity<Sale>(s =>
           s.HasOne(c => c.Customer)
           .WithMany(s => s.Sales)
           .HasForeignKey(s => s.CustomerId).OnDelete(DeleteBehavior.Restrict)
           );

            modelBuilder.Entity<Sale>(s =>
          s.HasOne(c => c.Store)
          .WithMany(s => s.Sales)
          .HasForeignKey(s => s.StoreId).OnDelete(DeleteBehavior.Restrict)
          );

            modelBuilder.Entity<Sale>()
             .Property(e => e.Date).HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<Product>(s => s.HasKey(s => s.ProductId));

            modelBuilder.Entity<Product>()
                .Property(e => e.Name)
                .HasMaxLength(50)
                .IsRequired(true)
                .IsUnicode(true);

            modelBuilder.Entity<Product>()
               .Property(e => e.Description)
               .HasMaxLength(250)
               .HasDefaultValue("No description")
               .IsUnicode(true);

            modelBuilder.Entity<Customer>(s => s.HasKey(s => s.CustomerId));

            modelBuilder.Entity<Customer>()
              .Property(e => e.Name)
              .HasMaxLength(100)
              .IsRequired(true)
              .IsUnicode(true);

            modelBuilder.Entity<Customer>()
              .Property(e => e.Email)
              .HasMaxLength(80)
              .IsRequired(true)
              .IsUnicode(false);

            modelBuilder.Entity<Store>(s => s.HasKey(s => s.StoreId));

            modelBuilder.Entity<Store>()
              .Property(e => e.Name)
              .HasMaxLength(80)
              .IsRequired(true)
              .IsUnicode(true);
        }
    }
}
