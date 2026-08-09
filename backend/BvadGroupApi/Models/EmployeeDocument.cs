namespace BvadGroupApi.Models
{
    public enum DocumentType
    {
        Other = 0,
        CV = 1,                     // Curriculum Vitae
        NationalIdFront = 2,        // CNI recto
        NationalIdBack = 3,         // CNI verso
        Passport = 4,               // Passeport
        Diploma = 5,                // Diplôme
        Certificate = 6,            // Certificat
        DrivingLicense = 7,         // Permis de conduire
        MedicalCertificate = 8,     // Certificat médical
        WorkPermit = 9,             // Permis de travail
        Reference = 10,             // Lettre de recommandation
        Contract = 11,              // Contrat signé
        Payslip = 12,               // Fiche de paie
        BirthCertificate = 13,      // Acte de naissance
        MarriageCertificate = 14    // Acte de mariage
    }

    /// <summary>
    /// Document appartenant à un employé (CV, CNI, diplômes, etc.)
    /// </summary>
    public class EmployeeDocument
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        public DocumentType Type { get; set; } = DocumentType.Other;
        public string Title { get; set; } = string.Empty;              // Titre libre (ex: "Master en Info")
        public string? Description { get; set; }

        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;            // Chemin relatif
        public string? ContentType { get; set; }                       // application/pdf, image/jpeg...
        public long FileSize { get; set; }

        public DateTime? IssueDate { get; set; }                       // Date de délivrance
        public DateTime? ExpiryDate { get; set; }                      // Date d'expiration
        public bool IsVerified { get; set; }                           // Vérifié par RH ?

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid? UploadedById { get; set; }

        // 🧮 Calculé
        public bool IsExpired => ExpiryDate.HasValue && ExpiryDate < DateTime.UtcNow;
        public bool IsExpiringSoon => ExpiryDate.HasValue
            && ExpiryDate > DateTime.UtcNow
            && (ExpiryDate.Value - DateTime.UtcNow).TotalDays <= 60;
    }
}