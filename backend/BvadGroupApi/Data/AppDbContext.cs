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
        }
    }
}