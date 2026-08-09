using Microsoft.EntityFrameworkCore;
using BvadGroupApi.Models;

namespace BvadGroupApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // 🏢 Tables
        public DbSet<Company> Companies { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserCompany> UserCompanies { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Contract> Contracts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ========================================
            // 🏢 Configuration Company
            // ========================================
            modelBuilder.Entity<Company>(entity =>
            {
                entity.HasIndex(c => c.Code).IsUnique();
                entity.Property(c => c.Code).HasMaxLength(50).IsRequired();
                entity.Property(c => c.Name).HasMaxLength(200).IsRequired();
                entity.Property(c => c.Color).HasMaxLength(20).IsRequired();
                entity.Property(c => c.Logo).HasMaxLength(500);
                entity.Property(c => c.Description).HasMaxLength(1000);
            });

            // ========================================
            // 👤 Configuration User
            // ========================================
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Username).IsUnique();
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Username).HasMaxLength(100).IsRequired();
                entity.Property(u => u.Email).HasMaxLength(200).IsRequired();
                entity.Property(u => u.PasswordHash).IsRequired();
                entity.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
                entity.Property(u => u.LastName).HasMaxLength(100).IsRequired();
                entity.Property(u => u.PhoneNumber).HasMaxLength(30);
                entity.Property(u => u.PhotoUrl).HasMaxLength(500);
            });

            // ========================================
            // 🔗 Configuration UserCompany (many-to-many)
            // ========================================
            modelBuilder.Entity<UserCompany>(entity =>
            {
                // Un user ne peut être qu'une fois dans la même filiale
                entity.HasIndex(uc => new { uc.UserId, uc.CompanyId }).IsUnique();

                entity.HasOne(uc => uc.User)
                      .WithMany(u => u.UserCompanies)
                      .HasForeignKey(uc => uc.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(uc => uc.Company)
                      .WithMany(c => c.UserCompanies)
                      .HasForeignKey(uc => uc.CompanyId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ========================================
            // 👨‍💼 Configuration Employee
            // ========================================
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.MiddleName).HasMaxLength(100);
                entity.Property(e => e.Email).HasMaxLength(200).IsRequired();
                entity.Property(e => e.PhoneNumber).HasMaxLength(30);
                entity.Property(e => e.Position).HasMaxLength(150).IsRequired();
                entity.Property(e => e.Department).HasMaxLength(100);
                entity.Property(e => e.Address).HasMaxLength(300);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.Country).HasMaxLength(100);
                entity.Property(e => e.Nationality).HasMaxLength(100);
                entity.Property(e => e.BirthPlace).HasMaxLength(200);
                entity.Property(e => e.PhotoUrl).HasMaxLength(500);
                entity.Property(e => e.Notes).HasMaxLength(2000);
                entity.Property(e => e.Salary).HasPrecision(18, 2);
                entity.Property(e => e.CommitteePositionCustom).HasMaxLength(150);

                // Company
                entity.HasOne(e => e.Company)
                      .WithMany()
                      .HasForeignKey(e => e.CompanyId)
                      .OnDelete(DeleteBehavior.Restrict);

                // User (compte auto-créé)
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.SetNull);

                // Manager (hiérarchie)
                entity.HasOne(e => e.Manager)
                      .WithMany()
                      .HasForeignKey(e => e.ManagerId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // ========================================
            // 📄 Configuration Contract
            // ========================================
            modelBuilder.Entity<Contract>(entity =>
            {
                entity.HasIndex(c => c.ContractNumber).IsUnique();
                entity.Property(c => c.ContractNumber).HasMaxLength(50).IsRequired();
                entity.Property(c => c.Position).HasMaxLength(150).IsRequired();
                entity.Property(c => c.Department).HasMaxLength(100);
                entity.Property(c => c.Currency).HasMaxLength(10).IsRequired();
                entity.Property(c => c.DocumentUrl).HasMaxLength(500);
                entity.Property(c => c.DocumentFileName).HasMaxLength(300);
                entity.Property(c => c.SpecialClauses).HasMaxLength(4000);
                entity.Property(c => c.Notes).HasMaxLength(2000);
                entity.Property(c => c.Salary).HasPrecision(18, 2);

                // Employee
                entity.HasOne(c => c.Employee)
                      .WithMany()
                      .HasForeignKey(c => c.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Company
                entity.HasOne(c => c.Company)
                      .WithMany()
                      .HasForeignKey(c => c.CompanyId)
                      .OnDelete(DeleteBehavior.Restrict);

                // CreatedBy
                entity.HasOne(c => c.CreatedBy)
                      .WithMany()
                      .HasForeignKey(c => c.CreatedById)
                      .OnDelete(DeleteBehavior.SetNull);
            });

        }
    }
}