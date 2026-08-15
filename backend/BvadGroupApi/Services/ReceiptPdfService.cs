using BvadGroupApi.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BvadGroupApi.Services
{
    public interface IReceiptPdfService
    {
        byte[] Generate(Payment payment);
    }

    public class ReceiptPdfService : IReceiptPdfService
    {
        private readonly IFileStorageService _storage;
        private readonly ILogger<ReceiptPdfService> _logger;

        public ReceiptPdfService(IFileStorageService storage, ILogger<ReceiptPdfService> logger)
        {
            _storage = storage;
            _logger = logger;
        }

        public byte[] Generate(Payment payment)
        {
            _logger.LogInformation("🧾 Génération reçu PDF : {Number}", payment.PaymentNumber);

            var invoice = payment.Invoice;
            var company = invoice?.Company;
            var client = invoice?.Client;
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
                        page.Margin(1.5f, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        // ═══ HEADER ═══
                        page.Header().Element(c => BillingPdfHelper.RenderHeader(
                            c, company!, logoBytes,
                            "REÇU DE PAIEMENT", payment.PaymentNumber ?? "—", payment.PaymentDate, color));

                        // ═══ CONTENU ═══
                        page.Content().PaddingVertical(20).Column(col =>
                        {
                            col.Spacing(15);

                            // Titre grand
                            col.Item().AlignCenter().Text("REÇU DE PAIEMENT")
                                .Bold().FontSize(18).FontColor(color);

                            col.Item().AlignCenter().Text($"N° {payment.PaymentNumber}")
                                .FontSize(11).FontColor(Colors.Grey.Darken2);

                            // Client
                            col.Item().PaddingTop(10).Element(c =>
                                BillingPdfHelper.RenderClientBlock(c, client!, color));

                            // Corps du reçu
                            col.Item().PaddingTop(10).Border(2).BorderColor(color).Padding(20).Column(body =>
                            {
                                body.Item().AlignCenter().Text($"Nous accusons réception de la somme de :")
                                    .FontSize(11).FontColor(Colors.Grey.Darken3);

                                body.Item().PaddingTop(8).AlignCenter().Background(color).Padding(12).Text(text =>
                                {
                                    text.Span(BillingPdfHelper.FormatMoney(payment.Amount, payment.Currency))
                                        .Bold().FontSize(24).FontColor(Colors.White);
                                });

                                body.Item().PaddingTop(15).Text(text =>
                                {
                                    text.Span("Correspondant au règlement ").FontSize(10);
                                    text.Span(GetMethodLabel(payment.Method, payment.MobileMoneyOperator)).Bold().FontSize(10);
                                    text.Span($" de la facture ").FontSize(10);
                                    text.Span(invoice?.InvoiceNumber ?? "—").Bold().FontSize(10);
                                    text.Span($" en date du {invoice?.IssueDate:dd/MM/yyyy}.").FontSize(10);
                                });

                                if (!string.IsNullOrEmpty(payment.Reference))
                                {
                                    body.Item().PaddingTop(5).Text(text =>
                                    {
                                        text.Span("Référence : ").FontSize(9).FontColor(Colors.Grey.Darken2);
                                        text.Span(payment.Reference).FontSize(9).Bold();
                                    });
                                }

                                body.Item().PaddingTop(10).Row(r =>
                                {
                                    r.RelativeItem().Text(text =>
                                    {
                                        text.Span("Montant facture : ").FontSize(9).FontColor(Colors.Grey.Darken2);
                                        text.Span(BillingPdfHelper.FormatMoney(invoice?.TotalTTC ?? 0, payment.Currency)).FontSize(9).Bold();
                                    });
                                    r.RelativeItem().AlignRight().Text(text =>
                                    {
                                        text.Span("Solde restant : ").FontSize(9).FontColor(Colors.Grey.Darken2);
                                        text.Span(BillingPdfHelper.FormatMoney(invoice?.AmountDue ?? 0, payment.Currency))
                                            .FontSize(9).Bold()
                                            .FontColor((invoice?.AmountDue ?? 0) == 0 ? Colors.Green.Darken2 : Colors.Orange.Darken2);
                                    });
                                });

                                if (!string.IsNullOrEmpty(payment.Notes))
                                {
                                    body.Item().PaddingTop(8).Text(text =>
                                    {
                                        text.Span("Notes : ").FontSize(9).FontColor(Colors.Grey.Darken2);
                                        text.Span(payment.Notes).FontSize(9).Italic();
                                    });
                                }
                            });

                            // Date + lieu
                            col.Item().PaddingTop(10).AlignRight().Text(text =>
                            {
                                text.Span($"Fait à {company?.City ?? "Brazzaville"}, le {payment.PaymentDate:dd/MM/yyyy}")
                                    .FontSize(10).Italic().FontColor(Colors.Grey.Darken2);
                            });

                            // Signature
                            col.Item().PaddingTop(15).Row(row =>
                            {
                                row.RelativeItem();
                                row.ConstantItem(240).Element(c =>
                                    BillingPdfHelper.RenderSignatureBlock(c, company!, signBytes, stampBytes));
                            });
                        });

                        // Footer
                        page.Footer().AlignCenter().Column(f =>
                        {
                            f.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                            f.Item().PaddingTop(3).Text(text =>
                            {
                                text.Span($"Reçu n° {payment.PaymentNumber} · ").FontSize(7).FontColor(Colors.Grey.Darken1);
                                text.Span($"Généré le {DateTime.UtcNow:dd/MM/yyyy HH:mm}").FontSize(7).FontColor(Colors.Grey.Darken1);
                            });
                        });
                    });
                });

                return document.GeneratePdf();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur génération reçu PDF");
                throw;
            }
        }

        private static string GetMethodLabel(PaymentMethod method, MobileMoneyOperator? op) => method switch
        {
            PaymentMethod.Cash => "en espèces",
            PaymentMethod.BankTransfer => "par virement bancaire",
            PaymentMethod.MobileMoney => $"par Mobile Money ({op?.ToString() ?? "—"})",
            PaymentMethod.Check => "par chèque",
            PaymentMethod.Card => "par carte bancaire",
            _ => "par autre moyen"
        };
    }
}