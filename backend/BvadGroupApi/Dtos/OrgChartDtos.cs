namespace BvadGroupApi.Dtos
{
    /// <summary>Noeud de l'organigramme (récursif)</summary>
    public class OrgNodeDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string? Email { get; set; }
        public string? PhotoUrl { get; set; }
        public string? PhoneNumber { get; set; }
        public string CompanyRole { get; set; } = string.Empty;
        public bool IsCommitteeMember { get; set; }
        public string? CommitteePosition { get; set; }
        public string CompanyColor { get; set; } = "#1e3a8a";

        /// <summary>Employés qui reportent directement à celui-ci</summary>
        public List<OrgNodeDto> Children { get; set; } = new();

        public int TotalSubordinates => Children.Count + Children.Sum(c => c.TotalSubordinates);
    }
}