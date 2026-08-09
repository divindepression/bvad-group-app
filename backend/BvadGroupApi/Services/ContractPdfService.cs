using BvadGroupApi.Data;
using BvadGroupApi.Models;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BvadGroupApi.Services
{
    public interface IContractPdfService
    {
        Task<byte[]> GenerateContractPdfAsync(Contract contract);
    }

    public class ContractPdfService : IContractPdfService
    {
        private readonly IFileStorageService _storage;
        private readonly AppDbContext _context;
        private readonly ILogger<ContractPdfService> _logger;

        public ContractPdfService(
            IFileStorageService storage,
            AppDbContext context,
            ILogger<ContractPdfService> logger)
        {
            _storage = storage;
            _context = context;
            _logger = logger;
        }

        public async Task<byte[]> GenerateContractPdfAsync(Contract contract)
        {
            _logger.LogInformation("📄 Génération contrat PDF pour {Name}", contract.Employee?.FullName);

            // ═══ Charger l'employé complet (pour signature) ═══
            var employee = await _context.Employees
                .Include(e => e.Company)
                .FirstOrDefaultAsync(e => e.Id == contract.EmployeeId);

            var company = contract.Company ?? employee?.Company;

            // ═══ Charger les images ═══
            var logoBytes = LoadImage(company?.LogoUrl);
            var stampBytes = LoadImage(company?.StampUrl);
            var directorSignBytes = LoadImage(company?.DirectorSignatureUrl);
            var employeeSignBytes = LoadImage(employee?.SignatureUrl);

            var companyColor = company?.Color ?? "#1e3a8a";

            try
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(1.8f, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        // ═══════════════════════════════════════
                        // HEADER — Logo + Infos filiale
                        // ═══════════════════════════════════════
                        page.Header().Column(col =>
                        {
                            col.Item().Row(row =>
                            {
                                // Logo à gauche
                                row.ConstantItem(90).Column(logoCol =>
                                {
                                    if (logoBytes != null)
                                    {
                                        logoCol.Item().Height(70).Image(logoBytes).FitArea();
                                    }
                                    else
                                    {
                                        logoCol.Item().Height(70).AlignCenter().AlignMiddle()
                                            .Text(company?.Logo ?? "🏢").FontSize(40);
                                    }
                                });

                                // Infos entreprise au centre
                                row.RelativeItem().PaddingLeft(15).Column(info =>
                                {
                                    info.Item().Text(company?.Name ?? "BVAD GROUP")
                                        .Bold().FontSize(18).FontColor(companyColor);

                                    if (!string.IsNullOrEmpty(company?.LegalName))
                                        info.Item().Text(company.LegalName)
                                            .FontSize(9).FontColor(Colors.Grey.Darken2);

                                    if (!string.IsNullOrEmpty(company?.Slogan))
                                        info.Item().PaddingTop(2).Text(company.Slogan)
                                            .Italic().FontSize(8).FontColor(Colors.Grey.Darken1);

                                    info.Item().PaddingTop(5).Text(text =>
                                    {
                                        if (!string.IsNullOrEmpty(company?.Address))
                                            text.Span($"{company.Address}").FontSize(8).FontColor(Colors.Grey.Darken2);
                                        if (!string.IsNullOrEmpty(company?.City))
                                            text.Span($" · {company.City}").FontSize(8).FontColor(Colors.Grey.Darken2);
                                    });

                                    info.Item().Text(text =>
                                    {
                                        if (!string.IsNullOrEmpty(company?.Phone))
                                            text.Span($"📞 {company.Phone}").FontSize(8).FontColor(Colors.Grey.Darken2);
                                        if (!string.IsNullOrEmpty(company?.Email))
                                            text.Span($"  ✉ {company.Email}").FontSize(8).FontColor(Colors.Grey.Darken2);
                                    });
                                });

                                // Info contrat à droite
                                row.ConstantItem(140).AlignRight().Column(right =>
                                {
                                    right.Item().Text($"N° {contract.ContractNumber}")
                                        .Bold().FontSize(11).FontColor(companyColor);
                                    right.Item().Text($"{contract.StartDate:dd/MM/yyyy}")
                                        .FontSize(9).FontColor(Colors.Grey.Darken2);

                                    if (!string.IsNullOrEmpty(company?.RegistrationNumber))
                                        right.Item().PaddingTop(5).Text($"RCCM : {company.RegistrationNumber}")
                                            .FontSize(7).FontColor(Colors.Grey.Darken1);

                                    if (!string.IsNullOrEmpty(company?.TaxNumber))
                                        right.Item().Text($"NIU : {company.TaxNumber}")
                                            .FontSize(7).FontColor(Colors.Grey.Darken1);
                                });
                            });

                            col.Item().PaddingVertical(8).LineHorizontal(2).LineColor(companyColor);
                        });

                        // ═══════════════════════════════════════
                        // CONTENU
                        // ═══════════════════════════════════════
                        page.Content().PaddingVertical(8).Column(col =>
                        {
                            col.Spacing(8);

                            // Titre
                            col.Item().AlignCenter().Text($"CONTRAT DE TRAVAIL — {contract.ContractType}")
                                .Bold().FontSize(16).FontColor(Colors.Grey.Darken4);

                            // Parties
                            col.Item().PaddingTop(5).Text("ENTRE LES SOUSSIGNÉS :").Bold().FontSize(11);

                            col.Item().PaddingLeft(15).Column(inner =>
                            {
                                inner.Item().Text(text =>
                                {
                                    text.Span("L'entreprise ").FontSize(10);
                                    text.Span(company?.LegalName ?? company?.Name ?? "").Bold().FontSize(10);
                                    if (!string.IsNullOrEmpty(company?.Address))
                                        text.Span($", dont le siège est situé {company.Address}, {company.City}").FontSize(10);
                                    text.Span(", représentée par ").FontSize(10);
                                    text.Span(company?.DirectorName ?? "son représentant légal").Bold().FontSize(10);
                                    text.Span($", en qualité de {company?.DirectorTitle ?? "Dirigeant"}").FontSize(10);
                                    text.Span(", ci-après désignée « l'Employeur »,").FontSize(10);
                                });
                                inner.Item().PaddingTop(6).Text("D'UNE PART,").Bold().FontSize(10);
                            });

                            col.Item().Text("ET").Bold().AlignCenter().FontSize(11);

                            col.Item().PaddingLeft(15).Column(inner =>
                            {
                                inner.Item().Text(text =>
                                {
                                    text.Span($"{(employee?.Gender == Gender.Female ? "Madame" : "Monsieur")} ");
                                    text.Span(employee?.FullName ?? "").Bold();
                                    if (employee?.BirthDate.HasValue == true)
                                        text.Span($", né(e) le {employee.BirthDate:dd/MM/yyyy}");
                                    if (!string.IsNullOrEmpty(employee?.BirthPlace))
                                        text.Span($" à {employee.BirthPlace}");
                                });

                                if (!string.IsNullOrEmpty(employee?.NationalIdNumber))
                                    inner.Item().Text($"CNI n° {employee.NationalIdNumber}").FontSize(9);

                                inner.Item().Text($"Demeurant : {employee?.Address ?? "—"}, {employee?.City ?? ""}, {employee?.Country ?? ""}");
                                inner.Item().Text("Ci-après désigné(e) « l'Employé(e) »");
                                inner.Item().PaddingTop(6).Text("D'AUTRE PART,").Bold().FontSize(10);
                            });

                            col.Item().PaddingTop(8).Text("IL A ÉTÉ CONVENU CE QUI SUIT :").Bold().FontSize(11);

                            // Articles
                            AddArticle(col, "Article 1 — Objet",
                                $"L'Employeur engage l'Employé(e) au poste de {contract.Position}" +
                                (contract.Department != null ? $" au sein du département {contract.Department}." : "."));

                            AddArticle(col, "Article 2 — Prise d'effet et durée",
                                $"Le présent contrat prend effet à compter du {contract.StartDate:dd/MM/yyyy}" +
                                (contract.EndDate.HasValue
                                    ? $" et prend fin le {contract.EndDate:dd/MM/yyyy}."
                                    : " pour une durée indéterminée.") +
                                (contract.TrialPeriodMonths.HasValue
                                    ? $"\nUne période d'essai de {contract.TrialPeriodMonths} mois est prévue à compter de la date d'embauche."
                                    : ""));

                            AddArticle(col, "Article 3 — Rémunération",
                                $"L'Employé(e) percevra un salaire brut mensuel de " +
                                $"{contract.Salary:N0} {contract.Currency}, payable à terme échu, " +
                                $"selon les modalités en vigueur au sein de l'entreprise.");

                            AddArticle(col, "Article 4 — Durée du travail",
                                $"La durée hebdomadaire de travail est fixée à {contract.WeeklyHours ?? 40} heures, " +
                                "conformément à la législation en vigueur.");

                            if (!string.IsNullOrWhiteSpace(contract.SpecialClauses))
                            {
                                AddArticle(col, "Article 5 — Clauses particulières", contract.SpecialClauses);
                            }

                            AddArticle(col, "Article 6 — Loi applicable",
                                "Le présent contrat est soumis aux lois et règlements en vigueur dans le pays de rattachement de l'Employeur.");

                            // ═══════════════════════════════════════
                            // SIGNATURES (avec cachet + signatures scannées)
                            // ═══════════════════════════════════════
                            col.Item().PaddingTop(10).Row(row =>
                            {
                                // ═══ EMPLOYEUR ═══
                                row.RelativeItem().Column(sign =>
                                {
                                    sign.Item().Text("Pour l'Employeur").Bold().FontSize(11);
                                    sign.Item().PaddingTop(2).Text(company?.DirectorName ?? "")
                                        .FontSize(9).FontColor(Colors.Grey.Darken2);
                                    sign.Item().Text(company?.DirectorTitle ?? "")
                                        .FontSize(8).Italic().FontColor(Colors.Grey.Darken1);

                                    // Zone signature + cachet superposés
                                    sign.Item().PaddingTop(8).Height(100).Row(sigRow =>
                                    {
                                        sigRow.RelativeItem().Column(inner =>
                                        {
                                            // Signature dirigeant
                                            if (directorSignBytes != null)
                                            {
                                                inner.Item().MaxHeight(65).Image(directorSignBytes).FitHeight();
                                            }
                                            else
                                            {
                                                inner.Item().Height(60);
                                            }
                                        });

                                        // Cachet officiel à droite
                                        sigRow.ConstantItem(90).Column(stampCol =>
                                        {
                                            if (stampBytes != null)
                                            {
                                                stampCol.Item().MaxHeight(70).Image(stampBytes).FitArea();
                                            }
                                        });
                                    });

                                    sign.Item().LineHorizontal(1).LineColor(Colors.Grey.Darken1);
                                    sign.Item().Text("Signature et cachet").FontSize(8).FontColor(Colors.Grey.Darken2);
                                });

                                row.ConstantItem(20);

                                // ═══ EMPLOYÉ ═══
                                row.RelativeItem().Column(sign =>
                                {
                                    sign.Item().Text("L'Employé(e)").Bold().FontSize(11);
                                    sign.Item().PaddingTop(2).Text(employee?.FullName ?? "")
                                        .FontSize(9).FontColor(Colors.Grey.Darken2);
                                    sign.Item().Text(contract.Position)
                                        .FontSize(8).Italic().FontColor(Colors.Grey.Darken1);

                                    sign.Item().PaddingTop(8).Height(100).Column(inner =>
                                    {
                                        // Signature employé (si présente)
                                        if (employeeSignBytes != null)
                                        {
                                            inner.Item().MaxHeight(70).Image(employeeSignBytes).FitHeight();
                                        }
                                        else
                                        {
                                            inner.Item().Height(80).AlignCenter().AlignMiddle()
                                                .Text("[ Signature à apposer ]")
                                                .FontSize(9).FontColor(Colors.Grey.Lighten1).Italic();
                                        }
                                    });

                                    sign.Item().LineHorizontal(1).LineColor(Colors.Grey.Darken1);
                                    sign.Item().Text("Signature précédée de « Lu et approuvé »")
                                        .FontSize(8).FontColor(Colors.Grey.Darken2);
                                });
                            });
                        });

                        // ═══════════════════════════════════════
                        // FOOTER
                        // ═══════════════════════════════════════
                        page.Footer().AlignCenter().Column(footer =>
                        {
                            footer.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                            footer.Item().PaddingTop(4).Text(text =>
                            {
                                text.Span($"Fait à ").FontSize(9).FontColor(Colors.Grey.Darken2);
                                text.Span(company?.City ?? "Douala").FontSize(9).FontColor(Colors.Grey.Darken2);
                                text.Span($", le {DateTime.UtcNow:dd/MM/yyyy}").FontSize(9).FontColor(Colors.Grey.Darken2);
                            });
                            footer.Item().Text(text =>
                            {
                                text.Span("Document généré par ").FontSize(7).FontColor(Colors.Grey.Darken1);
                                text.Span("BVAD GROUP").FontSize(7).Bold().FontColor(Colors.Grey.Darken1);
                                text.Span($" · {DateTime.UtcNow:dd/MM/yyyy HH:mm}").FontSize(7).FontColor(Colors.Grey.Darken1);
                            });
                        });
                    });
                });

                var pdfBytes = document.GeneratePdf();
                _logger.LogInformation("✅ Contrat PDF généré ({Size} KB)", pdfBytes.Length / 1024);
                return pdfBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur génération contrat PDF");
                throw;
            }
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
            catch
            {
                return null;
            }
        }

        private void AddArticle(ColumnDescriptor col, string title, string body)
        {
            col.Item().Column(inner =>
            {
                inner.Item().Text(title).Bold().FontSize(9.5f).FontColor(Colors.Grey.Darken4);
                inner.Item().PaddingTop(1).Text(body).LineHeight(1.25f).FontSize(9);
            });
        }
    }
}