using BvadGroupApi.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BvadGroupApi.Services
{
    public interface IContractPdfService
    {
        byte[] GenerateContractPdf(Contract contract);
    }

    public class ContractPdfService : IContractPdfService
    {
        static ContractPdfService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] GenerateContractPdf(Contract contract)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    // Header
                    page.Header().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text(text =>
                            {
                                text.Line(contract.Company?.Name ?? "BVAD GROUP")
                                    .Bold().FontSize(20).FontColor(contract.Company?.Color ?? "#1e3a8a");
                                text.Line("Bâtir · Valoriser · Accompagner · Développer")
                                    .FontSize(9).FontColor(Colors.Grey.Darken2);
                            });
                            row.ConstantItem(150).AlignRight().Text(text =>
                            {
                                text.Line($"N° {contract.ContractNumber}").Bold();
                                text.Line($"{contract.StartDate:dd/MM/yyyy}").FontSize(9);
                            });
                        });
                        col.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    });

                    // Body
                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        col.Spacing(15);

                        // Titre
                        col.Item().AlignCenter().Text($"CONTRAT DE TRAVAIL — {contract.ContractType}")
                            .Bold().FontSize(16);

                        // Parties
                        col.Item().Text("ENTRE LES SOUSSIGNÉS :").Bold();

                        col.Item().PaddingLeft(15).Column(inner =>
                        {
                            inner.Item().Text(text =>
                            {
                                text.Span("L'entreprise ").FontSize(10);
                                text.Span(contract.Company?.Name ?? "").Bold();
                                text.Span(", ci-après désignée « l'Employeur »,");
                            });
                            inner.Item().PaddingTop(10).Text("D'UNE PART,").Bold();
                        });

                        col.Item().Text("ET").Bold().AlignCenter();

                        col.Item().PaddingLeft(15).Column(inner =>
                        {
                            inner.Item().Text(text =>
                            {
                                text.Span("Monsieur / Madame ");
                                text.Span(contract.Employee?.FullName ?? "").Bold();
                                text.Span($", né(e) le {contract.Employee?.BirthDate:dd/MM/yyyy}");
                            });
                            inner.Item().Text($"Demeurant à : {contract.Employee?.City ?? "—"}, {contract.Employee?.Country ?? "—"}");
                            inner.Item().Text($"Ci-après désigné(e) « l'Employé(e) »");
                            inner.Item().PaddingTop(10).Text("D'AUTRE PART,").Bold();
                        });

                        col.Item().PaddingTop(10).Text("IL A ÉTÉ CONVENU CE QUI SUIT :").Bold().FontSize(11);

                        // Articles
                        AddArticle(col, "Article 1 — Objet",
                            $"L'Employeur engage l'Employé(e) au poste de {contract.Position}" +
                            (contract.Department != null ? $" au sein du département {contract.Department}." : "."));

                        AddArticle(col, "Article 2 — Prise d'effet",
                            $"Le présent contrat prend effet à compter du {contract.StartDate:dd/MM/yyyy}" +
                            (contract.EndDate.HasValue ? $" et prend fin le {contract.EndDate:dd/MM/yyyy}." : " pour une durée indéterminée.") +
                            (contract.TrialPeriodMonths.HasValue ? $"\nUne période d'essai de {contract.TrialPeriodMonths} mois est prévue." : ""));

                        AddArticle(col, "Article 3 — Rémunération",
                            $"L'Employé(e) percevra un salaire brut mensuel de " +
                            $"{contract.Salary:N0} {contract.Currency}, payable à terme échu.");

                        AddArticle(col, "Article 4 — Durée du travail",
                            $"La durée hebdomadaire de travail est fixée à {contract.WeeklyHours ?? 40} heures.");

                        if (!string.IsNullOrWhiteSpace(contract.SpecialClauses))
                        {
                            AddArticle(col, "Article 5 — Clauses particulières", contract.SpecialClauses);
                        }

                        // Signatures
                        col.Item().PaddingTop(40).Row(row =>
                        {
                            row.RelativeItem().Column(sign =>
                            {
                                sign.Item().Text("Pour l'Employeur").Bold();
                                sign.Item().PaddingTop(50).LineHorizontal(1);
                                sign.Item().Text("Signature").FontSize(9).FontColor(Colors.Grey.Darken2);
                            });
                            row.ConstantItem(30);
                            row.RelativeItem().Column(sign =>
                            {
                                sign.Item().Text("L'Employé(e)").Bold();
                                sign.Item().PaddingTop(50).LineHorizontal(1);
                                sign.Item().Text("Signature (précédée de « Lu et approuvé »)").FontSize(9).FontColor(Colors.Grey.Darken2);
                            });
                        });
                    });

                    // Footer
                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Fait à ").FontSize(9).FontColor(Colors.Grey.Darken2);
                        text.Span(contract.Employee?.City ?? "Douala").FontSize(9).FontColor(Colors.Grey.Darken2);
                        text.Span($", le {DateTime.UtcNow:dd/MM/yyyy}").FontSize(9).FontColor(Colors.Grey.Darken2);
                    });
                });
            });

            return document.GeneratePdf();
        }

        private void AddArticle(ColumnDescriptor col, string title, string body)
        {
            col.Item().Column(inner =>
            {
                inner.Item().Text(title).Bold().FontSize(11);
                inner.Item().PaddingTop(3).Text(body).LineHeight(1.4f);
            });
        }
    }
}