using BvadGroupApi.Data;
using BvadGroupApi.Dtos;
using BvadGroupApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BvadGroupApi.Services
{
    public interface IOrgChartService
    {
        Task<List<OrgNodeDto>> GetOrgChartAsync(Guid companyId);
    }

    public class OrgChartService : IOrgChartService
    {
        private readonly AppDbContext _context;

        public OrgChartService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<OrgNodeDto>> GetOrgChartAsync(Guid companyId)
        {
            var employees = await _context.Employees
                .Include(e => e.Company)
                .Where(e => e.CompanyId == companyId
                         && e.Status != EmployeeStatus.Terminated)
                .ToListAsync();

            // Construire un dictionnaire pour accès rapide
            var dict = employees.ToDictionary(
                e => e.Id,
                e => new OrgNodeDto
                {
                    Id = e.Id,
                    FullName = e.FullName,
                    Position = e.Position,
                    Department = e.Department,
                    Email = e.Email,
                    PhotoUrl = e.PhotoUrl,
                    PhoneNumber = e.PhoneNumber,
                    CompanyRole = e.CompanyRole.ToString(),
                    IsCommitteeMember = e.IsCommitteeMember,
                    CommitteePosition = e.CommitteePosition.ToString(),
                    CompanyColor = e.Company?.Color ?? "#1e3a8a"
                });

            // Racines = ceux qui n'ont pas de manager (ou dont le manager n'est pas dans la même filiale)
            var roots = new List<OrgNodeDto>();

            foreach (var emp in employees)
            {
                var node = dict[emp.Id];

                if (emp.ManagerId.HasValue && dict.TryGetValue(emp.ManagerId.Value, out var parentNode))
                {
                    parentNode.Children.Add(node);
                }
                else
                {
                    roots.Add(node);
                }
            }

            // Trier : membres du comité d'abord, puis alphabétique
            SortNodes(roots);

            return roots;
        }

        private void SortNodes(List<OrgNodeDto> nodes)
        {
            nodes.Sort((a, b) =>
            {
                // PDG en premier
                var aIsCEO = a.CommitteePosition == "CEO" ? 0 : 1;
                var bIsCEO = b.CommitteePosition == "CEO" ? 0 : 1;
                if (aIsCEO != bIsCEO) return aIsCEO - bIsCEO;

                // Puis membres du comité
                var aCommittee = a.IsCommitteeMember ? 0 : 1;
                var bCommittee = b.IsCommitteeMember ? 0 : 1;
                if (aCommittee != bCommittee) return aCommittee - bCommittee;

                // Puis alphabétique
                return a.FullName.CompareTo(b.FullName);
            });

            foreach (var node in nodes)
            {
                SortNodes(node.Children);
            }
        }
    }
}