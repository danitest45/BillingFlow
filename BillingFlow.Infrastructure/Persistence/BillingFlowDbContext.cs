using BillingFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BillingFlow.Infrastructure.Persistence
{
    public class BillingFlowDbContext : DbContext
    {
        public BillingFlowDbContext(DbContextOptions<BillingFlowDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Client> Clients => Set<Client>();
        public DbSet<InvoiceRecord> InvoiceRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(x => x.Email)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.HasIndex(x => x.Email)
                    .IsUnique();
            });

            modelBuilder.Entity<Client>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(x => x.Email)
                    .HasMaxLength(200);

                entity.Property(x => x.Phone)
                    .HasMaxLength(20);

                entity.Property(x => x.MonthlyAmount)
                    .HasPrecision(18, 2);

            });

            modelBuilder.Entity<InvoiceRecord>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Amount)
                    .HasPrecision(18, 2);

                entity.Property(x => x.Status)
                    .HasConversion<int>();

                entity.HasIndex(x => new { x.ClientId, x.ReferenceYear, x.ReferenceMonth })
                    .IsUnique();
            });

            modelBuilder.Entity<InvoiceRecord>()
                .HasOne(i => i.Client)
                .WithMany(c => c.InvoiceRecords)
                .HasForeignKey(i => i.ClientId);

        }
    }
}
