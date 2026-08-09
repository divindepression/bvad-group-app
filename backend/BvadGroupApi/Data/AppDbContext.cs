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

                // Relation Employee → Company
                entity.HasOne(e => e.Company)
                      .WithMany()
                      .HasForeignKey(e => e.CompanyId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Relation Employee → User (optionnelle)
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

        }
    }
}