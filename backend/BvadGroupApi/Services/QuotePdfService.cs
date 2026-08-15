using BvadGroupApi.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BvadGroupApi.Services
{
    public interface IQuotePdfService
    {
        byte[] Generate(Quote quote);
    }

    public class QuotePdfService : IQuotePdfService
    {
        private readonly IFileStorageService _storage;
        private readonly ILogger<QuotePdfService> _logger;

        public QuotePdfService(IFileStorageService storage, ILogger<QuotePdfService> logger)
        {
            _storage = storage;
            _logger = logger;
        }

        public byte[] Generate(Quote quote)
        {
            _logger.LogInformation("📝 Génération devis PDF : {Number}", quote.QuoteNumber);

            var company = quote.Company;
            var client = quote.Client;
            var color = company?.Color ?? "#1e3a8a";

            var logoBytes = BillingPdfHelper.LoadImage(_storage, company?.LogoUrl);
            var stampBytes = BillingPdfHelper.LoadImage(_storage, company?.StampUrl);
            var signBytes = BillingPdfHelper.LoadImage(_storage, company?.DirectorSignatureUrl);

            try
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(1.2f, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(9));

                        // ═══ HEADER ═══
                        page.Header().Element(c => BillingPdfHelper.RenderHeader(
                            c, company!, logoBytes,
                            "DEVIS", quote.QuoteNumber, quote.IssueDate, color));

                        // ═══ CONTENU ═══
                        page.Content().PaddingVertical(6).Column(col =>
                        {
                            col.Spacing(8);

                            // Client + Infos devis (2 colonnes)
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Element(c =>
                                    BillingPdfHelper.RenderClientBlock(c, client!, color));

                                row.ConstantItem(10);

                                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(info =>
                                {
                                    info.Item().Text("INFORMATIONS DEVIS")
                                        .Bold().FontSize(8).FontColor(color);

                                    info.Item().PaddingTop(4).Text(text =>
                                    {
                                        text.Span("Date d'émission : ").FontSize(9).FontColor(Colors.Grey.Darken2);
                                        text.Span($"{quote.IssueDate:dd/MM/yyyy}").FontSize(9).Bold();
                                    });
                                    info.Item().Text(text =>
                                    {
                                        text.Span("Valable jusqu'au : ").FontSize(9).FontColor(Colors.Grey.Darken2);
                                        text.Span($"{quote.ValidUntil:dd/MM/yyyy}").FontSize(9).Bold();
                                    });
                                    info.Item().PaddingTop(4).Text(text =>
                                    {
                                        text.Span("Devise : ").FontSize(9).FontColor(Colors.Grey.Darken2);
                                        text.Span(quote.Currency).FontSize(9).Bold();
                                    });
                                    info.Item().Text(text =>
                                    {
                                        text.Span("TVA : ").FontSize(9).FontColor(Colors.Grey.Darken2);
                                        text.Span($"{quote.VatRate}%").FontSize(9).Bold();
                                    });
                                });
                            });

                            // Objet
                            if (!string.IsNullOrEmpty(quote.Subject))
                            {
                                col.Item().PaddingTop(4).Text(text =>
                                {
                                    text.Span("Objet : ").Bold().FontSize(10).FontColor(color);
                                    text.Span(quote.Subject).FontSize(10);
                                });
                            }

                            // ═══ TABLE DES LIGNES ═══
                            col.Item().PaddingTop(4).Table(table =>
                            {
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.ConstantColumn(25);      // #
                                    cols.RelativeColumn(4);        // Description
                                    cols.ConstantColumn(40);       // Qté
                                    cols.ConstantColumn(50);       // Unité
                                    cols.ConstantColumn(70);       // PU
                                    cols.ConstantColumn(40);       // Remise
                                    cols.ConstantColumn(80);       // Total
                                });

                                // Header
                                table.Header(header =>
                                {
                                    string[] titles = { "#", "Description", "Qté", "Unité", "P.U. HT", "Rem.%", "Total HT" };
                                    foreach (var t in titles)
                                    {
                                        header.Cell().Background(color).Padding(4)
                                            .Text(t).Bold().FontSize(8).FontColor(Colors.White);
                                    }
                                });

                                // Lignes
                                var rowIdx = 1;
                                foreach (var line in quote.LineItems.OrderBy(l => l.Order))
                                {
                                    var bg = rowIdx % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;

                                    table.Cell().Background(bg).Padding(4).Text(rowIdx.ToString()).FontSize(8);
                                    table.Cell().Background(bg).Padding(4).Text(line.Description).FontSize(8);
                                    table.Cell().Background(bg).Padding(4).AlignRight().Text(line.Quantity.ToString("N0")).FontSize(8);
                                    table.Cell().Background(bg).Padding(4).Text(line.Unit ?? "").FontSize(8);
                                    table.Cell().Background(bg).Padding(4).AlignRight().Text(BillingPdfHelper.FormatMoney(line.UnitPrice, "")).FontSize(8);
                                    table.Cell().Background(bg).Padding(4).AlignRight().Text($"{line.DiscountPercent}%").FontSize(8);
                                    table.Cell().Background(bg).Padding(4).AlignRight().Text(BillingPdfHelper.FormatMoney(line.LineTotal, "")).FontSize(8).Bold();

                                    rowIdx++;
                                }
                            });

                            // ═══ TOTAUX ═══
                            col.Item().PaddingTop(6).Row(row =>
                            {
                                row.RelativeItem();  // espace
                                row.ConstantItem(220).Column(totals =>
                                {
                                    totals.Item().Row(r =>
                                    {
                                        r.RelativeItem().Text("Sous-total HT").FontSize(9);
                                        r.ConstantItem(100).AlignRight().Text(BillingPdfHelper.FormatMoney(quote.SubtotalHT, quote.Currency)).FontSize(9);
                                    });

                                    if (quote.DiscountPercent > 0)
                                    {
                                        totals.Item().Row(r =>
                                        {
                                            r.RelativeItem().Text($"Remise ({quote.DiscountPercent}%)").FontSize(9).FontColor(Colors.Orange.Darken2);
                                            r.ConstantItem(100).AlignRight().Text($"- {BillingPdfHelper.FormatMoney(quote.DiscountAmount, quote.Currency)}").FontSize(9).FontColor(Colors.Orange.Darken2);
                                        });
                                    }

                                    totals.Item().Row(r =>
                                    {
                                        r.RelativeItem().Text($"TVA ({quote.VatRate}%)").FontSize(9);
                                        r.ConstantItem(100).AlignRight().Text(BillingPdfHelper.FormatMoney(quote.VatAmount, quote.Currency)).FontSize(9);
                                    });

                                    totals.Item().PaddingTop(4).Background(color).Padding(6).Row(r =>
                                    {
                                        r.RelativeItem().Text("TOTAL TTC").Bold().FontSize(11).FontColor(Colors.White);
                                        r.ConstantItem(100).AlignRight().Text(BillingPdfHelper.FormatMoney(quote.TotalTTC, quote.Currency)).Bold().FontSize(11).FontColor(Colors.White);
                                    });
                                });
                            });

                            // ═══ CONDITIONS ═══
                            if (!string.IsNullOrEmpty(quote.PaymentTerms) || !string.IsNullOrEmpty(quote.Notes))
                            {
                                col.Item().PaddingTop(6).Column(cond =>
                                {
                                    if (!string.IsNullOrEmpty(quote.PaymentTerms))
                                    {
                                        cond.Item().Text("Conditions de paiement").Bold().FontSize(9).FontColor(color);
                                        cond.Item().PaddingTop(1).Text(quote.PaymentTerms).FontSize(8);
                                    }

                                    if (!string.IsNullOrEmpty(quote.Notes))
                                    {
                                        cond.Item().PaddingTop(3).Text("Notes").Bold().FontSize(9).FontColor(color);
                                        cond.Item().PaddingTop(1).Text(quote.Notes).FontSize(8);
                                    }
                                });
                            }

                            // ═══ ACCEPTATION + SIGNATURE ═══
                            col.Item().PaddingTop(8).Row(row =>
                            {
                                // Bloc acceptation client
                                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(acc =>
                                {
                                    acc.Item().Text("BON POUR ACCORD").Bold().FontSize(9).FontColor(color);
                                    acc.Item().PaddingTop(2).Text("Le client")
                                        .FontSize(8).FontColor(Colors.Grey.Darken2);
                                    acc.Item().PaddingTop(35).LineHorizontal(1).LineColor(Colors.Grey.Darken1);
                                    acc.Item().Text("Nom, date et signature précédés de « Bon pour accord »")
                                        .FontSize(7).Italic().FontColor(Colors.Grey.Darken1);
                                });

                                row.ConstantItem(15);

                                // Bloc signature émetteur
                                row.RelativeItem().Element(c =>
                                    BillingPdfHelper.RenderSignatureBlock(c, company!, signBytes, stampBytes));
                            });
                        });

                        // ═══ FOOTER ═══
                        page.Footer().AlignCenter().Column(f =>
                        {
                            f.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                            f.Item().PaddingTop(3).Text(text =>
                            {
                                text.Span($"Devis n° {quote.QuoteNumber} · ")
                                    .FontSize(7).FontColor(Colors.Grey.Darken1);
                                text.Span($"Valable jusqu'au {quote.ValidUntil:dd/MM/yyyy}")
                                    .FontSize(7).FontColor(Colors.Grey.Darken1);
                            });
                            if (!string.IsNullOrEmpty(company?.InvoiceFooter))
                            {
                                f.Item().Text(company.InvoiceFooter).FontSize(6).FontColor(Colors.Grey.Darken1);
                            }
                        });
                    });
                });

                return document.GeneratePdf();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur génération devis PDF");
                throw;
            }
        }
    }
}