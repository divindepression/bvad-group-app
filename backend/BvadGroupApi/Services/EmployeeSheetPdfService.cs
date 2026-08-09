using BvadGroupApi.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BvadGroupApi.Services
{
    public interface IEmployeeSheetPdfService
    {
        byte[] GenerateSheet(Employee employee);
    }

    public class EmployeeSheetPdfService : IEmployeeSheetPdfService
    {
        private readonly IFileStorageService _storage;
        private readonly ILogger<EmployeeSheetPdfService> _logger;

        public EmployeeSheetPdfService(IFileStorageService storage, ILogger<EmployeeSheetPdfService> logger)
        {
            _storage = storage;
            _logger = logger;
        }

        public byte[] GenerateSheet(Employee employee)
        {
            _logger.LogInformation("📄 Génération fiche employé pour {Name}", employee.FullName);

            var company = employee.Company;
            var companyColor = company?.Color ?? "#1e3a8a";

            var logoBytes = LoadImage(company?.LogoUrl);
            var stampBytes = LoadImage(company?.StampUrl);
            var directorSignBytes = LoadImage(company?.DirectorSignatureUrl);
            var employeeSignBytes = LoadImage(employee.SignatureUrl);
            var photoBytes = LoadImage(employee.IdentityPhotoUrl);

            try
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(1.0f, Unit.Centimetre);  // 🔽 Marges réduites
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(8));  // 🔽 Font plus petite

                        // ═══════════════════════════════════════
                        // HEADER compact
                        // ═══════════════════════════════════════
                        page.Header().Column(col =>
                        {
                            col.Item().Row(row =>
                            {
                                row.ConstantItem(60).Column(logoCol =>
                                {
                                    if (logoBytes != null)
                                        logoCol.Item().Height(45).Image(logoBytes).FitArea();
                                    else
                                        logoCol.Item().Height(45).AlignCenter().AlignMiddle()
                                            .Text(company?.Logo ?? "🏢").FontSize(28);
                                });

                                row.RelativeItem().PaddingLeft(8).Column(info =>
                                {
                                    info.Item().Text(company?.Name ?? "BVAD GROUP")
                                        .Bold().FontSize(14).FontColor(companyColor);
                                    if (!string.IsNullOrEmpty(company?.LegalName))
                                        info.Item().Text(company.LegalName).FontSize(7).FontColor(Colors.Grey.Darken2);
                                    info.Item().Text(text =>
                                    {
                                        if (!string.IsNullOrEmpty(company?.Address))
                                            text.Span($"{company.Address} · ").FontSize(6).FontColor(Colors.Grey.Darken2);
                                        if (!string.IsNullOrEmpty(company?.Phone))
                                            text.Span($"{company.Phone}").FontSize(6).FontColor(Colors.Grey.Darken2);
                                    });
                                    info.Item().Text(text =>
                                    {
                                        if (!string.IsNullOrEmpty(company?.RegistrationNumber))
                                            text.Span($"RCCM : {company.RegistrationNumber}  ").FontSize(6).FontColor(Colors.Grey.Darken1);
                                        if (!string.IsNullOrEmpty(company?.TaxNumber))
                                            text.Span($"NIU : {company.TaxNumber}").FontSize(6).FontColor(Colors.Grey.Darken1);
                                    });
                                });

                                row.ConstantItem(100).AlignRight().Column(right =>
                                {
                                    right.Item().Text("FICHE EMPLOYÉ")
                                        .Bold().FontSize(10).FontColor(companyColor);
                                    right.Item().Text($"N° {employee.EmployeeNumber ?? "—"}")
                                        .FontSize(8).FontColor(Colors.Grey.Darken3);
                                    right.Item().Text($"Émise le {DateTime.UtcNow:dd/MM/yyyy}")
                                        .FontSize(6).FontColor(Colors.Grey.Darken1);
                                });
                            });

                            col.Item().PaddingVertical(3).LineHorizontal(1.5f).LineColor(companyColor);
                        });

                        // ═══════════════════════════════════════
                        // CONTENU compact
                        // ═══════════════════════════════════════
                        page.Content().PaddingVertical(4).Column(col =>
                        {
                            col.Spacing(5);  // 🔽 Espacement réduit

                            // ═══ Photo + Nom + badges ═══
                            col.Item().Row(row =>
                            {
                                row.ConstantItem(75).Column(photoCol =>
                                {
                                    if (photoBytes != null)
                                    {
                                        photoCol.Item().Height(95).Border(1.5f).BorderColor(companyColor)
                                            .Image(photoBytes).FitArea();
                                    }
                                    else
                                    {
                                        photoCol.Item().Height(95).Border(1.5f).BorderColor(companyColor)
                                            .Background(Colors.Grey.Lighten3)
                                            .AlignCenter().AlignMiddle()
                                            .Text(GetInitials(employee.FullName))
                                            .FontSize(28).Bold().FontColor(Colors.Grey.Darken2);
                                    }
                                });

                                row.ConstantItem(10);

                                row.RelativeItem().Column(info =>
                                {
                                    info.Item().Text(employee.FullName)
                                        .Bold().FontSize(15).FontColor(Colors.Grey.Darken4);
                                    info.Item().Text(employee.Position)
                                        .FontSize(11).FontColor(companyColor);
                                    if (!string.IsNullOrEmpty(employee.Department))
                                        info.Item().Text($"Département : {employee.Department}")
                                            .FontSize(8).FontColor(Colors.Grey.Darken2);

                                    info.Item().PaddingTop(4).Text(text =>
                                    {
                                        text.Span("Statut : ").FontSize(7).FontColor(Colors.Grey.Darken1);
                                        text.Span(TranslateStatus(employee.Status) + "   ").Bold().FontSize(8);
                                        text.Span("Contrat : ").FontSize(7).FontColor(Colors.Grey.Darken1);
                                        text.Span(employee.ContractType.ToString() + "   ").Bold().FontSize(8);
                                        text.Span("Embauche : ").FontSize(7).FontColor(Colors.Grey.Darken1);
                                        text.Span(employee.HireDate.ToString("dd/MM/yyyy")).Bold().FontSize(8);
                                    });

                                    if (employee.IsCommitteeMember)
                                    {
                                        info.Item().PaddingTop(2).Text("🏛 Membre du comité de direction")
                                            .Bold().FontSize(8).FontColor(companyColor);
                                    }
                                });
                            });

                            // ═══ Sections en 2 colonnes ═══
                            col.Item().Row(row =>
                            {
                                // Colonne gauche
                                row.RelativeItem().Column(left =>
                                {
                                    left.Spacing(5);
                                    AddSection(left, "🆔 IDENTITÉ", companyColor, table =>
                                    {
                                        AddRow(table, "Genre", TranslateGender(employee.Gender));
                                        if (employee.BirthDate.HasValue)
                                            AddRow(table, "Naissance", $"{employee.BirthDate:dd/MM/yyyy} ({employee.Age} ans)");
                                        AddRow(table, "Lieu", employee.BirthPlace ?? "—");
                                        AddRow(table, "Nationalité", employee.Nationality ?? "—");
                                        AddRow(table, "Situation", employee.MaritalStatus ?? "—");
                                        AddRow(table, "N° CNI", employee.NationalIdNumber ?? "—");
                                        AddRow(table, "N° CNPS", employee.SocialSecurityNumber ?? "—");
                                    });

                                    AddSection(left, "💼 EMPLOI", companyColor, table =>
                                    {
                                        AddRow(table, "Matricule", employee.EmployeeNumber ?? "—");
                                        AddRow(table, "Poste", employee.Position);
                                        AddRow(table, "Département", employee.Department ?? "—");
                                        AddRow(table, "Type contrat", employee.ContractType.ToString());
                                        AddRow(table, "Embauche", employee.HireDate.ToString("dd/MM/yyyy"));
                                        if (employee.EndDate.HasValue)
                                            AddRow(table, "Fin", employee.EndDate?.ToString("dd/MM/yyyy") ?? "—");
                                        if (employee.Salary.HasValue)
                                            AddRow(table, "Salaire brut", $"{employee.Salary:N0} FCFA");
                                    });
                                });

                                row.ConstantItem(8);

                                // Colonne droite
                                row.RelativeItem().Column(right =>
                                {
                                    right.Spacing(5);
                                    AddSection(right, "📞 CONTACT", companyColor, table =>
                                    {
                                        AddRow(table, "Email pro", employee.Email);
                                        AddRow(table, "Email perso", employee.PersonalEmail ?? "—");
                                        AddRow(table, "Téléphone", employee.PhoneNumber ?? "—");
                                        AddRow(table, "Tél. 2", employee.SecondaryPhone ?? "—");
                                        AddRow(table, "Adresse", employee.Address ?? "—");
                                        AddRow(table, "Ville / Pays", $"{employee.City ?? "—"}, {employee.Country ?? "—"}");
                                    });

                                    if (!string.IsNullOrEmpty(employee.EmergencyContactName))
                                    {
                                        AddSection(right, "🚨 URGENCE", "#dc2626", table =>
                                        {
                                            AddRow(table, "Nom", employee.EmergencyContactName);
                                            AddRow(table, "Lien", employee.EmergencyContactRelation ?? "—");
                                            AddRow(table, "Téléphone", employee.EmergencyContactPhone ?? "—");
                                        });
                                    }

                                    if (!string.IsNullOrEmpty(employee.BankName) || !string.IsNullOrEmpty(employee.MobileMoneyNumber))
                                    {
                                        AddSection(right, "🏦 BANCAIRE", companyColor, table =>
                                        {
                                            if (!string.IsNullOrEmpty(employee.BankName))
                                            {
                                                AddRow(table, "Banque", employee.BankName);
                                                AddRow(table, "N° compte", employee.BankAccountNumber ?? "—");
                                            }
                                            if (!string.IsNullOrEmpty(employee.MobileMoneyNumber))
                                            {
                                                AddRow(table, "Mobile Money", $"{employee.MobileMoneyOperator} — {employee.MobileMoneyNumber}");
                                            }
                                        });
                                    }
                                });
                            });

                            // ═══ SIGNATURES + CACHET (compact) ═══
                            col.Item().PaddingTop(6).Row(row =>
                            {
                                // Signature employé
                                row.RelativeItem().Column(sign =>
                                {
                                    sign.Item().Text("Signature de l'employé").Bold().FontSize(8);
                                    sign.Item().PaddingTop(2).Height(55).Column(inner =>
                                    {
                                        if (employeeSignBytes != null)
                                            inner.Item().MaxHeight(50).Image(employeeSignBytes).FitHeight();
                                    });
                                    sign.Item().LineHorizontal(1).LineColor(Colors.Grey.Darken1);
                                    sign.Item().Text(employee.FullName).FontSize(7).FontColor(Colors.Grey.Darken2);
                                });

                                row.ConstantItem(15);

                                // Signature dirigeant + cachet
                                row.RelativeItem().Column(sign =>
                                {
                                    sign.Item().Text("Pour l'entreprise").Bold().FontSize(8);
                                    sign.Item().PaddingTop(2).Height(55).Row(sigRow =>
                                    {
                                        sigRow.RelativeItem().Column(inner =>
                                        {
                                            if (directorSignBytes != null)
                                                inner.Item().MaxHeight(50).Image(directorSignBytes).FitHeight();
                                        });
                                        sigRow.ConstantItem(55).Column(stampCol =>
                                        {
                                            if (stampBytes != null)
                                                stampCol.Item().MaxHeight(55).Image(stampBytes).FitArea();
                                        });
                                    });
                                    sign.Item().LineHorizontal(1).LineColor(Colors.Grey.Darken1);
                                    sign.Item().Text(company?.DirectorName ?? "").FontSize(7).FontColor(Colors.Grey.Darken2);
                                    sign.Item().Text(company?.DirectorTitle ?? "").FontSize(6).Italic().FontColor(Colors.Grey.Darken1);
                                });
                            });
                        });

                        // ═══ FOOTER ═══
                        page.Footer().AlignCenter().Text(text =>
                        {
                            text.Span("Fiche générée par ").FontSize(6).FontColor(Colors.Grey.Darken1);
                            text.Span("BVAD GROUP").FontSize(6).Bold().FontColor(Colors.Grey.Darken1);
                            text.Span($" · Document confidentiel · {DateTime.UtcNow:dd/MM/yyyy HH:mm}").FontSize(6).FontColor(Colors.Grey.Darken1);
                        });
                    });
                });

                var pdfBytes = document.GeneratePdf();
                _logger.LogInformation("✅ Fiche PDF générée ({Size} KB)", pdfBytes.Length / 1024);
                return pdfBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur génération fiche PDF");
                throw;
            }
        }

        // ═══ Helpers ═══
        private void AddSection(ColumnDescriptor col, string title, string color, Action<TableDescriptor> content)
        {
            col.Item().Column(section =>
            {
                section.Item().PaddingBottom(2).Text(title)
                    .Bold().FontSize(9).FontColor(color);
                section.Item().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.ConstantColumn(75);
                        cols.RelativeColumn();
                    });
                    content(table);
                });
            });
        }

        private void AddRow(TableDescriptor table, string label, string value)
        {
            table.Cell().PaddingVertical(1).Text(label).FontSize(7).FontColor(Colors.Grey.Darken2);
            table.Cell().PaddingVertical(1).Text(value).FontSize(7).FontColor(Colors.Grey.Darken4);
        }

        private byte[]? LoadImage(string? relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return null;
            try
            {
                var fullPath = _storage.GetFullPath(relativePath);
                if (!File.Exists(fullPath)) return null;
                var bytes = File.ReadAllBytes(fullPath);
                return bytes.Length > 100 ? bytes : null;
            }
            catch { return null; }
        }

        private string GetInitials(string fullName)
        {
            var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2) return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
            return fullName.Length >= 2 ? fullName[..2].ToUpper() : fullName.ToUpper();
        }

        private string TranslateStatus(EmployeeStatus s) => s switch
        {
            EmployeeStatus.Active => "Actif",
            EmployeeStatus.OnLeave => "En congé",
            EmployeeStatus.Suspended => "Suspendu",
            EmployeeStatus.Terminated => "Parti",
            EmployeeStatus.Probation => "Période d'essai",
            _ => s.ToString()
        };

        private string TranslateGender(Gender g) => g switch
        {
            Gender.Male => "Masculin",
            Gender.Female => "Féminin",
            _ => "Autre"
        };
    }
}