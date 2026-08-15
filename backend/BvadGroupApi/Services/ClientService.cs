using BvadGroupApi.Data;
using BvadGroupApi.Dtos;
using BvadGroupApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BvadGroupApi.Services
{
    public interface IClientService
    {
        Task<List<ClientDto>> GetAllAsync(ClientFilters filters);
        Task<ClientDto?> GetByIdAsync(Guid id);
        Task<ClientDto?> CreateAsync(CreateClientDto dto);
        Task<ClientDto?> UpdateAsync(Guid id, CreateClientDto dto);
        Task<bool> DeleteAsync(Guid id);
    }

    public class ClientService : IClientService
    {
        private readonly AppDbContext _context;
        private readonly IBillingNumberService _numbering;

        public ClientService(AppDbContext context, IBillingNumberService numbering)
        {
            _context = context;
            _numbering = numbering;
        }

        public async Task<List<ClientDto>> GetAllAsync(ClientFilters filters)
        {
            var query = _context.Clients.AsQueryable();

            if (filters.IsActive.HasValue)
                query = query.Where(c => c.IsActive == filters.IsActive);

            if (!string.IsNullOrWhiteSpace(filters.Search))
            {
                var s = filters.Search.ToLower();
                query = query.Where(c =>
                    c.Name.ToLower().Contains(s) ||
                    (c.ContactPerson != null && c.ContactPerson.ToLower().Contains(s)) ||
                    (c.Email != null && c.Email.ToLower().Contains(s)) ||
                    (c.Phone != null && c.Phone.Contains(s)) ||
                    (c.ClientCode != null && c.ClientCode.ToLower().Contains(s)));
            }

            var list = await query.OrderBy(c => c.Name).ToListAsync();
            return list.Select(ToDto).ToList();
        }

        public async Task<ClientDto?> GetByIdAsync(Guid id)
        {
            var c = await _context.Clients.FindAsync(id);
            return c == null ? null : ToDto(c);
        }

        public async Task<ClientDto?> CreateAsync(CreateClientDto dto)
        {
            var client = new Client
            {
                ClientCode = await _numbering.GenerateClientCodeAsync(),
                Type = dto.Type,
                Name = dto.Name,
                ContactPerson = dto.ContactPerson,
                Position = dto.Position,
                LegalForm = dto.LegalForm,
                RegistrationNumber = dto.RegistrationNumber,
                TaxNumber = dto.TaxNumber,
                Capital = dto.Capital,
                Email = dto.Email,
                Phone = dto.Phone,
                SecondaryPhone = dto.SecondaryPhone,
                Website = dto.Website,
                Address = dto.Address,
                City = dto.City,
                Country = dto.Country ?? "Congo",
                PostalCode = dto.PostalCode,
                Notes = dto.Notes,
                IsActive = true
            };

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            return ToDto(client);
        }

        public async Task<ClientDto?> UpdateAsync(Guid id, CreateClientDto dto)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null) return null;

            client.Type = dto.Type;
            client.Name = dto.Name;
            client.ContactPerson = dto.ContactPerson;
            client.Position = dto.Position;
            client.LegalForm = dto.LegalForm;
            client.RegistrationNumber = dto.RegistrationNumber;
            client.TaxNumber = dto.TaxNumber;
            client.Capital = dto.Capital;
            client.Email = dto.Email;
            client.Phone = dto.Phone;
            client.SecondaryPhone = dto.SecondaryPhone;
            client.Website = dto.Website;
            client.Address = dto.Address;
            client.City = dto.City;
            client.Country = dto.Country;
            client.PostalCode = dto.PostalCode;
            client.Notes = dto.Notes;
            client.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return ToDto(client);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null) return false;

            // Vérification : pas de devis ou factures liés
            var hasQuotes = await _context.Quotes.AnyAsync(q => q.ClientId == id);
            var hasInvoices = await _context.Invoices.AnyAsync(i => i.ClientId == id);

            if (hasQuotes || hasInvoices)
            {
                // Soft delete
                client.IsActive = false;
                client.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
            return true;
        }

        private static ClientDto ToDto(Client c) => new(
            c.Id, c.ClientCode, c.Type.ToString(), c.Name, c.DisplayName,
            c.ContactPerson, c.Position,
            c.LegalForm, c.RegistrationNumber, c.TaxNumber, c.Capital,
            c.Email, c.Phone, c.SecondaryPhone, c.Website,
            c.Address, c.City, c.Country, c.PostalCode,
            c.Notes, c.IsActive, c.CreatedAt
        );
    }
}