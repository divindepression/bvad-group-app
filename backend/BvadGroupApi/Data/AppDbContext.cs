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
        public DbSet<EmployeeDocument> EmployeeDocuments { get; set; }
        public DbSet<LeaveType> LeaveTypes { get; set; }
        public DbSet<LeaveBalance> LeaveBalances { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<QuoteLineItem> QuoteLineItems { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceLineItem> InvoiceLineItems { get; set; }
        public DbSet<Payment> Payments { get; set; }

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
                entity.Property(c => c.LegalName).HasMaxLength(300);
                entity.Property(c => c.Color).HasMaxLength(20).IsRequired();
                entity.Property(c => c.Logo).HasMaxLength(500);
                entity.Property(c => c.LogoUrl).HasMaxLength(500);
                entity.Property(c => c.StampUrl).HasMaxLength(500);
                entity.Property(c => c.DirectorSignatureUrl).HasMaxLength(500);
                entity.Property(c => c.Description).HasMaxLength(1000);
                entity.Property(c => c.Slogan).HasMaxLength(300);
                entity.Property(c => c.RegistrationNumber).HasMaxLength(50);
                entity.Property(c => c.TaxNumber).HasMaxLength(50);
                entity.Property(c => c.Address).HasMaxLength(500);
                entity.Property(c => c.City).HasMaxLength(100);
                entity.Property(c => c.Country).HasMaxLength(100);
                entity.Property(c => c.Phone).HasMaxLength(30);
                entity.Property(c => c.Email).HasMaxLength(200);
                entity.Property(c => c.Website).HasMaxLength(300);
                entity.Property(c => c.DirectorName).HasMaxLength(200);
                entity.Property(c => c.DirectorTitle).HasMaxLength(150);
                entity.Property(c => c.DefaultVatRate).HasPrecision(5, 2);
                entity.Property(c => c.DefaultCurrency).HasMaxLength(10);
                entity.Property(c => c.BankName).HasMaxLength(200);
                entity.Property(c => c.BankAccountNumber).HasMaxLength(100);
                entity.Property(c => c.MobileMoneyNumber).HasMaxLength(30);
                entity.Property(c => c.InvoiceFooter).HasMaxLength(2000);
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

                // Nouveaux champs à configurer
                entity.Property(e => e.EmployeeNumber).HasMaxLength(50);
                entity.Property(e => e.PersonalEmail).HasMaxLength(200);
                entity.Property(e => e.SecondaryPhone).HasMaxLength(30);
                entity.Property(e => e.PostalCode).HasMaxLength(20);
                entity.Property(e => e.MaritalStatus).HasMaxLength(50);
                entity.Property(e => e.NationalIdNumber).HasMaxLength(50);
                entity.Property(e => e.PassportNumber).HasMaxLength(50);
                entity.Property(e => e.SocialSecurityNumber).HasMaxLength(50);
                entity.Property(e => e.EmergencyContactName).HasMaxLength(200);
                entity.Property(e => e.EmergencyContactPhone).HasMaxLength(30);
                entity.Property(e => e.EmergencyContactRelation).HasMaxLength(50);
                entity.Property(e => e.BankName).HasMaxLength(200);
                entity.Property(e => e.BankAccountNumber).HasMaxLength(50);
                entity.Property(e => e.BankIban).HasMaxLength(50);
                entity.Property(e => e.BankSwift).HasMaxLength(20);
                entity.Property(e => e.PaymentMethod).HasMaxLength(50);
                entity.Property(e => e.MobileMoneyOperator).HasMaxLength(50);
                entity.Property(e => e.MobileMoneyNumber).HasMaxLength(30);
                entity.Property(e => e.IdentityPhotoUrl).HasMaxLength(500);
                entity.Property(e => e.SignatureUrl).HasMaxLength(500);

                entity.HasIndex(e => e.EmployeeNumber).IsUnique();

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

            // ========================================
            // 📎 Configuration EmployeeDocument
            // ========================================
            modelBuilder.Entity<EmployeeDocument>(entity =>
            {
                entity.Property(d => d.Title).HasMaxLength(200).IsRequired();
                entity.Property(d => d.Description).HasMaxLength(1000);
                entity.Property(d => d.FileName).HasMaxLength(300).IsRequired();
                entity.Property(d => d.FileUrl).HasMaxLength(500).IsRequired();
                entity.Property(d => d.ContentType).HasMaxLength(100);

                entity.HasOne(d => d.Employee)
                      .WithMany()
                      .HasForeignKey(d => d.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });


            // ========================================
            // 🏖 Configuration LeaveType
            // ========================================
            modelBuilder.Entity<LeaveType>(entity =>
            {
                entity.HasIndex(l => l.Code).IsUnique();
                entity.Property(l => l.Code).HasMaxLength(20).IsRequired();
                entity.Property(l => l.Name).HasMaxLength(200).IsRequired();
                entity.Property(l => l.Description).HasMaxLength(1000);
                entity.Property(l => l.Icon).HasMaxLength(20);
                entity.Property(l => l.Color).HasMaxLength(20).IsRequired();
                entity.Property(l => l.DaysAccruedPerMonth).HasPrecision(5, 2);

                entity.HasOne(l => l.Company)
                      .WithMany()
                      .HasForeignKey(l => l.CompanyId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // ========================================
            // 📊 Configuration LeaveBalance
            // ========================================
            modelBuilder.Entity<LeaveBalance>(entity =>
            {
                entity.HasIndex(b => new { b.EmployeeId, b.LeaveTypeId, b.Year }).IsUnique();
                entity.Property(b => b.AllocatedDays).HasPrecision(6, 2);
                entity.Property(b => b.UsedDays).HasPrecision(6, 2);
                entity.Property(b => b.CarriedOverDays).HasPrecision(6, 2);
                entity.Property(b => b.Adjustment).HasPrecision(6, 2);

                entity.HasOne(b => b.Employee)
                      .WithMany()
                      .HasForeignKey(b => b.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(b => b.LeaveType)
                      .WithMany()
                      .HasForeignKey(b => b.LeaveTypeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ========================================
            // 📝 Configuration LeaveRequest
            // ========================================
            modelBuilder.Entity<LeaveRequest>(entity =>
            {
                entity.Property(r => r.Reason).HasMaxLength(2000);
                entity.Property(r => r.ProofDocumentUrl).HasMaxLength(500);
                entity.Property(r => r.ProofDocumentName).HasMaxLength(300);
                entity.Property(r => r.ApprovalComment).HasMaxLength(2000);
                entity.Property(r => r.DaysCount).HasPrecision(6, 2);

                entity.HasOne(r => r.Employee)
                      .WithMany()
                      .HasForeignKey(r => r.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.Company)
                      .WithMany()
                      .HasForeignKey(r => r.CompanyId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.LeaveType)
                      .WithMany()
                      .HasForeignKey(r => r.LeaveTypeId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.ApprovedByUser)
                      .WithMany()
                      .HasForeignKey(r => r.ApprovedByUserId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // ========================================
            // 🔔 Configuration Notification
            // ========================================
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.Property(n => n.Title).HasMaxLength(300).IsRequired();
                entity.Property(n => n.Message).HasMaxLength(1000).IsRequired();
                entity.Property(n => n.Icon).HasMaxLength(20);
                entity.Property(n => n.Color).HasMaxLength(20);
                entity.Property(n => n.ActionUrl).HasMaxLength(500);
                entity.Property(n => n.RelatedEntityType).HasMaxLength(100);

                entity.HasIndex(n => n.UserId);
                entity.HasIndex(n => new { n.UserId, n.IsRead });

                entity.HasOne(n => n.User)
                      .WithMany()
                      .HasForeignKey(n => n.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ========================================
            // 👤 Configuration Client
            // ========================================
            modelBuilder.Entity<Client>(entity =>
            {
                entity.HasIndex(c => c.ClientCode).IsUnique();
                entity.Property(c => c.ClientCode).HasMaxLength(50);
                entity.Property(c => c.Name).HasMaxLength(300).IsRequired();
                entity.Property(c => c.ContactPerson).HasMaxLength(200);
                entity.Property(c => c.Position).HasMaxLength(150);
                entity.Property(c => c.LegalForm).HasMaxLength(50);
                entity.Property(c => c.RegistrationNumber).HasMaxLength(50);
                entity.Property(c => c.TaxNumber).HasMaxLength(50);
                entity.Property(c => c.Capital).HasPrecision(18, 2);
                entity.Property(c => c.Email).HasMaxLength(200);
                entity.Property(c => c.Phone).HasMaxLength(30);
                entity.Property(c => c.SecondaryPhone).HasMaxLength(30);
                entity.Property(c => c.Website).HasMaxLength(300);
                entity.Property(c => c.Address).HasMaxLength(500);
                entity.Property(c => c.City).HasMaxLength(100);
                entity.Property(c => c.Country).HasMaxLength(100);
                entity.Property(c => c.PostalCode).HasMaxLength(20);
                entity.Property(c => c.Notes).HasMaxLength(2000);
            });

            // ========================================
            // 📝 Configuration Quote
            // ========================================
            modelBuilder.Entity<Quote>(entity =>
            {
                entity.HasIndex(q => q.QuoteNumber).IsUnique();
                entity.Property(q => q.QuoteNumber).HasMaxLength(100).IsRequired();
                entity.Property(q => q.Currency).HasMaxLength(10);
                entity.Property(q => q.VatRate).HasPrecision(5, 2);
                entity.Property(q => q.Subject).HasMaxLength(500);
                entity.Property(q => q.Notes).HasMaxLength(2000);
                entity.Property(q => q.PaymentTerms).HasMaxLength(1000);
                entity.Property(q => q.SubtotalHT).HasPrecision(18, 2);
                entity.Property(q => q.VatAmount).HasPrecision(18, 2);
                entity.Property(q => q.TotalTTC).HasPrecision(18, 2);
                entity.Property(q => q.DiscountPercent).HasPrecision(5, 2);
                entity.Property(q => q.DiscountAmount).HasPrecision(18, 2);
                entity.Property(q => q.RejectionReason).HasMaxLength(1000);

                entity.HasOne(q => q.Company).WithMany().HasForeignKey(q => q.CompanyId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(q => q.Client).WithMany().HasForeignKey(q => q.ClientId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(q => q.CreatedByUser).WithMany().HasForeignKey(q => q.CreatedByUserId).OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<QuoteLineItem>(entity =>
            {
                entity.Property(l => l.Description).HasMaxLength(500).IsRequired();
                entity.Property(l => l.Unit).HasMaxLength(30);
                entity.Property(l => l.Quantity).HasPrecision(10, 2);
                entity.Property(l => l.UnitPrice).HasPrecision(18, 2);
                entity.Property(l => l.DiscountPercent).HasPrecision(5, 2);

                entity.HasOne(l => l.Quote).WithMany(q => q.LineItems).HasForeignKey(l => l.QuoteId).OnDelete(DeleteBehavior.Cascade);
            });

            // ========================================
            // 🧾 Configuration Invoice
            // ========================================
            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.HasIndex(i => i.InvoiceNumber).IsUnique();
                entity.Property(i => i.InvoiceNumber).HasMaxLength(100).IsRequired();
                entity.Property(i => i.Currency).HasMaxLength(10);
                entity.Property(i => i.VatRate).HasPrecision(5, 2);
                entity.Property(i => i.Subject).HasMaxLength(500);
                entity.Property(i => i.Notes).HasMaxLength(2000);
                entity.Property(i => i.PaymentTerms).HasMaxLength(1000);
                entity.Property(i => i.SubtotalHT).HasPrecision(18, 2);
                entity.Property(i => i.VatAmount).HasPrecision(18, 2);
                entity.Property(i => i.TotalTTC).HasPrecision(18, 2);
                entity.Property(i => i.DiscountPercent).HasPrecision(5, 2);
                entity.Property(i => i.DiscountAmount).HasPrecision(18, 2);
                entity.Property(i => i.AmountPaid).HasPrecision(18, 2);

                entity.HasOne(i => i.Company).WithMany().HasForeignKey(i => i.CompanyId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(i => i.Client).WithMany().HasForeignKey(i => i.ClientId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(i => i.FromQuote).WithMany().HasForeignKey(i => i.FromQuoteId).OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(i => i.CreatedByUser).WithMany().HasForeignKey(i => i.CreatedByUserId).OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<InvoiceLineItem>(entity =>
            {
                entity.Property(l => l.Description).HasMaxLength(500).IsRequired();
                entity.Property(l => l.Unit).HasMaxLength(30);
                entity.Property(l => l.Quantity).HasPrecision(10, 2);
                entity.Property(l => l.UnitPrice).HasPrecision(18, 2);
                entity.Property(l => l.DiscountPercent).HasPrecision(5, 2);

                entity.HasOne(l => l.Invoice).WithMany(i => i.LineItems).HasForeignKey(l => l.InvoiceId).OnDelete(DeleteBehavior.Cascade);
            });

            // ========================================
            // 💳 Configuration Payment
            // ========================================
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.Property(p => p.PaymentNumber).HasMaxLength(100);
                entity.Property(p => p.Amount).HasPrecision(18, 2);
                entity.Property(p => p.Currency).HasMaxLength(10);
                entity.Property(p => p.Reference).HasMaxLength(200);
                entity.Property(p => p.Notes).HasMaxLength(1000);

                entity.HasOne(p => p.Invoice).WithMany(i => i.Payments).HasForeignKey(p => p.InvoiceId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(p => p.RecordedByUser).WithMany().HasForeignKey(p => p.RecordedByUserId).OnDelete(DeleteBehavior.SetNull);
            });

        }
    }
}