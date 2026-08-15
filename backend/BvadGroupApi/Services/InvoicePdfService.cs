using BvadGroupApi.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BvadGroupApi.Services
{
    public interface IInvoicePdfService
    {
        byte[] Generate(Invoice invoice);
    }

    public class InvoicePdfService : IInvoicePdfService
    {
        private readonly IFileStorageService _storage;
        private readonly ILogger<InvoicePdfService> _logger;

        public InvoicePdfService(IFileStorageService storage, ILogger<InvoicePdfService> logger)
        {
            _storage = storage;
            _logger = logger;
        }

        public byte[] Generate(Invoice invoice)
        {
            _logger.LogInformation("🧾 Génération facture PDF : {Number}", invoice.InvoiceNumber);

            var company = invoice.Company;
            var client = invoice.Client;
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
                            "FACTURE", invoice.InvoiceNumber, invoice.IssueDate, color));

                        // ═══ CONTENU ═══
                        page.Content().PaddingVertical(6).Column(col =>
                        {
                            col.Spacing(8);

                            // Badge statut si payée / partiellement / annulée
                            if (invoice.Status == InvoiceStatus.Paid)
                            {
                                col.Item().AlignRight().Background(Colors.Green.Darken2).Padding(4)
                                    .Text("✓ PAYÉE").Bold().FontSize(10).FontColor(Colors.White);
                            }
                            else if (invoice.Status == InvoiceStatus.PartiallyPaid)
                            {
                                col.Item().AlignRight().Background(Colors.Orange.Darken2).Padding(4)
                                    .Text($"PARTIELLEMENT PAYÉE").Bold().FontSize(10).FontColor(Colors.White);
                            }
                            else if (invoice.Status == InvoiceStatus.Cancelled)
                            {
                                col.Item().AlignRight().Background(Colors.Red.Darken2).Padding(4)
                                    .Text("✗ ANNULÉE").Bold().FontSize(10).FontColor(Colors.White);
                            }
                            else if (invoice.IsOverdue)
                            {
                                col.Item().AlignRight().Background(Colors.Red.Darken2).Padding(4)
                                    .Text($"⚠ EN RETARD ({invoice.DaysOverdue} jours)").Bold().FontSize(10).FontColor(Colors.White);
                            }

                            // Client + Infos facture
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Element(c =>
                                    BillingPdfHelper.RenderClientBlock(c, client!, color));

                                row.ConstantItem(10);

                                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(info =>
                                {
                                    info.Item().Text("INFORMATIONS FACTURE")
                                        .Bold().FontSize(8).FontColor(color);

                                    info.Item().PaddingTop(4).Text(text =>
                                    {
                                        text.Span("Date d'émission : ").FontSize(9).FontColor(Colors.Grey.Darken2);
                                        text.Span($"{invoice.IssueDate:dd/MM/yyyy}").FontSize(9).Bold();
                                    });
                                    info.Item().Text(text =>
                                    {
                                        text.Span("Date d'échéance : ").FontSize(9).FontColor(Colors.Grey.Darken2);
                                        text.Span($"{invoice.DueDate:dd/MM/yyyy}").FontSize(9).Bold()
                                            .FontColor(invoice.IsOverdue ? Colors.Red.Darken2 : Colors.Grey.Darken4);
                                    });
                                    info.Item().PaddingTop(4).Text(text =>
                                    {
                                        text.Span("Devise : ").FontSize(9).FontColor(Colors.Grey.Darken2);
                                        text.Span(invoice.Currency).FontSize(9).Bold();
                                    });
                                    info.Item().Text(text =>
                                    {
                                        text.Span("TVA : ").FontSize(9).FontColor(Colors.Grey.Darken2);
                                        text.Span($"{invoice.VatRate}%").FontSize(9).Bold();
                                    });
                                });
                            });

                            // Objet
                            if (!string.IsNullOrEmpty(invoice.Subject))
                            {
                                col.Item().PaddingTop(2).Text(text =>
                                {
                                    text.Span("Objet : ").Bold().FontSize(10).FontColor(color);
                                    text.Span(invoice.Subject).FontSize(10);
                                });
                            }

                            // ═══ TABLE ═══
                            col.Item().PaddingTop(4).Table(table =>
                            {
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.ConstantColumn(25);
                                    cols.RelativeColumn(4);
                                    cols.ConstantColumn(40);
                                    cols.ConstantColumn(50);
                                    cols.ConstantColumn(70);
                                    cols.ConstantColumn(40);
                                    cols.ConstantColumn(80);
                                });

                                table.Header(header =>
                                {
                                    string[] titles = { "#", "Description", "Qté", "Unité", "P.U. HT", "Rem.%", "Total HT" };
                                    foreach (var t in titles)
                                    {
                                        header.Cell().Background(color).Padding(4)
                                            .Text(t).Bold().FontSize(8).FontColor(Colors.White);
                                    }
                                });

                                var rowIdx = 1;
                                foreach (var line in invoice.LineItems.OrderBy(l => l.Order))
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
                                row.RelativeItem();
                                row.ConstantItem(240).Column(totals =>
                                {
                                    totals.Item().Row(r =>
                                    {
                                        r.RelativeItem().Text("Sous-total HT").FontSize(9);
                                        r.ConstantItem(110).AlignRight().Text(BillingPdfHelper.FormatMoney(invoice.SubtotalHT, invoice.Currency)).FontSize(9);
                                    });

                                    if (invoice.DiscountPercent > 0)
                                    {
                                        totals.Item().Row(r =>
                                        {
                                            r.RelativeItem().Text($"Remise ({invoice.DiscountPercent}%)").FontSize(9).FontColor(Colors.Orange.Darken2);
                                            r.ConstantItem(110).AlignRight().Text($"- {BillingPdfHelper.FormatMoney(invoice.DiscountAmount, invoice.Currency)}").FontSize(9).FontColor(Colors.Orange.Darken2);
                                        });
                                    }

                                    totals.Item().Row(r =>
                                    {
                                        r.RelativeItem().Text($"TVA ({invoice.VatRate}%)").FontSize(9);
                                        r.ConstantItem(110).AlignRight().Text(BillingPdfHelper.FormatMoney(invoice.VatAmount, invoice.Currency)).FontSize(9);
                                    });

                                    totals.Item().PaddingTop(4).Background(color).Padding(6).Row(r =>
                                    {
                                        r.RelativeItem().Text("TOTAL TTC").Bold().FontSize(11).FontColor(Colors.White);
                                        r.ConstantItem(110).AlignRight().Text(BillingPdfHelper.FormatMoney(invoice.TotalTTC, invoice.Currency)).Bold().FontSize(11).FontColor(Colors.White);
                                    });

                                    // Paiements + Solde dû
                                    if (invoice.AmountPaid > 0)
                                    {
                                        totals.Item().PaddingTop(3).Row(r =>
                                        {
                                            r.RelativeItem().Text("Payé").FontSize(9).FontColor(Colors.Green.Darken2);
                                            r.ConstantItem(110).AlignRight().Text($"- {BillingPdfHelper.FormatMoney(invoice.AmountPaid, invoice.Currency)}").FontSize(9).FontColor(Colors.Green.Darken2);
                                        });

                                        totals.Item().PaddingTop(2).Background(Colors.Grey.Darken3).Padding(6).Row(r =>
                                        {
                                            r.RelativeItem().Text("SOLDE DÛ").Bold().FontSize(11).FontColor(Colors.White);
                                            r.ConstantItem(110).AlignRight().Text(BillingPdfHelper.FormatMoney(invoice.AmountDue, invoice.Currency))
                                                .Bold().FontSize(11).FontColor(invoice.AmountDue == 0 ? Colors.Green.Lighten2 : Colors.White);
                                        });
                                    }
                                });
                            });

                            // ═══ CONDITIONS + MODE PAIEMENT ═══
                            col.Item().PaddingTop(6).Row(row =>
                            {
                                row.RelativeItem().Column(left =>
                                {
                                    if (!string.IsNullOrEmpty(invoice.PaymentTerms))
                                    {
                                        left.Item().Text("Conditions de paiement").Bold().FontSize(9).FontColor(color);
                                        left.Item().PaddingTop(1).Text(invoice.PaymentTerms).FontSize(8);
                                    }

                                    if (!string.IsNullOrEmpty(invoice.Notes))
                                    {
                                        left.Item().PaddingTop(3).Text("Notes").Bold().FontSize(9).FontColor(color);
                                        left.Item().PaddingTop(1).Text(invoice.Notes).FontSize(8);
                                    }
                                });

                                row.ConstantItem(15);

                                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(pay =>
                                {
                                    pay.Item().Text("MODES DE PAIEMENT").Bold().FontSize(8).FontColor(color);

                                    if (!string.IsNullOrEmpty(company?.BankName))
                                    {
                                        pay.Item().PaddingTop(2).Text($"🏦 {company.BankName}").FontSize(8);
                                        if (!string.IsNullOrEmpty(company.BankAccountNumber))
                                            pay.Item().Text($"Compte : {company.BankAccountNumber}").FontSize(7).FontColor(Colors.Grey.Darken2);
                                    }

                                    if (!string.IsNullOrEmpty(company?.MobileMoneyNumber))
                                    {
                                        pay.Item().PaddingTop(2).Text($"📱 Mobile Money : {company.MobileMoneyNumber}").FontSize(8);
                                    }
                                });
                            });

                            // ═══ SIGNATURE ═══
                            col.Item().PaddingTop(8).Row(row =>
                            {
                                row.RelativeItem();
                                row.ConstantItem(220).Element(c =>
                                    BillingPdfHelper.RenderSignatureBlock(c, company!, signBytes, stampBytes));
                            });
                        });

                        // ═══ FOOTER MENTIONS LÉGALES ═══
                        page.Footer().Column(f =>
                        {
                            f.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                            f.Item().PaddingTop(3).AlignCenter().Text(text =>
                            {
                                text.Span($"Facture n° {invoice.InvoiceNumber} · ").FontSize(7).FontColor(Colors.Grey.Darken1);
                                text.Span($"Émise le {invoice.IssueDate:dd/MM/yyyy}").FontSize(7).FontColor(Colors.Grey.Darken1);
                            });
                            f.Item().AlignCenter().Text("TVA sur les encaissements · Pas d'escompte pour paiement anticipé · Pénalités de retard au taux légal")
                                .FontSize(6).Italic().FontColor(Colors.Grey.Darken1);

                            if (!string.IsNullOrEmpty(company?.InvoiceFooter))
                            {
                                f.Item().AlignCenter().Text(company.InvoiceFooter).FontSize(6).FontColor(Colors.Grey.Darken1);
                            }
                        });
                    });
                });

                return document.GeneratePdf();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur génération facture PDF");
                throw;
            }
        }
    }
}