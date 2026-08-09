using BvadGroupApi.Models;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BvadGroupApi.Services
{
    public interface IBadgePdfService
    {
        byte[] GenerateBadge(Employee employee);
    }

    public class BadgePdfService : IBadgePdfService
    {
        private readonly IFileStorageService _storage;
        private readonly ILogger<BadgePdfService> _logger;

        public BadgePdfService(IFileStorageService storage, ILogger<BadgePdfService> logger)
        {
            _storage = storage;
            _logger = logger;
        }

        public byte[] GenerateBadge(Employee employee)
        {
            _logger.LogInformation("🎫 Génération badge pour {Name}", employee.FullName);

            // ═══ QR Code ═══
            var qrData = $"BVAD|{employee.EmployeeNumber}|{employee.FullName}|{employee.Company?.Code}";
            byte[] qrBytes;
            try
            {
                qrBytes = GenerateQrCode(qrData);
                _logger.LogInformation("✅ QR code généré ({Size} bytes)", qrBytes.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur QR code");
                qrBytes = new byte[0];
            }

            // ═══ Photo identité (avec protection) ═══
            byte[]? photoBytes = null;
            try
            {
                if (!string.IsNullOrEmpty(employee.IdentityPhotoUrl))
                {
                    var photoPath = _storage.GetFullPath(employee.IdentityPhotoUrl);
                    _logger.LogInformation("🔍 Recherche photo : {Path}", photoPath);

                    if (File.Exists(photoPath))
                    {
                        photoBytes = File.ReadAllBytes(photoPath);
                        _logger.LogInformation("✅ Photo chargée ({Size} KB)", photoBytes.Length / 1024);

                        if (photoBytes.Length < 100)
                        {
                            _logger.LogWarning("⚠ Photo trop petite, ignorée");
                            photoBytes = null;
                        }
                    }
                    else
                    {
                        _logger.LogWarning("⚠ Fichier photo introuvable");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur chargement photo");
                photoBytes = null;
            }

            var companyColor = employee.Company?.Color ?? "#1e3a8a";
            var validUntil = employee.BadgeValidUntil ?? DateTime.UtcNow.AddYears(1);

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

                        page.Content().Column(col =>
                        {
                            col.Spacing(20);

                            col.Item().AlignCenter().Text("🎫 CARTE DE SERVICE — BVAD GROUP")
                                .Bold().FontSize(14).FontColor(Colors.Grey.Darken3);

                            col.Item().Border(1).BorderColor(companyColor).Background(Colors.White)
                                .Padding(15).Column(recto =>
                                {
                                    // Header
                                    recto.Item().Background(companyColor).Padding(10).Row(row =>
                                    {
                                        row.RelativeItem().Text(text =>
                                        {
                                            text.Line(employee.Company?.Name ?? "BVAD GROUP")
                                                .Bold().FontSize(14).FontColor(Colors.White);
                                            text.Line("Bâtir · Valoriser").FontSize(8).FontColor(Colors.White);
                                        });
                                        row.ConstantItem(60).AlignRight().Text("🏢").FontSize(30);
                                    });

                                    // Corps
                                    recto.Item().PaddingTop(15).Row(row =>
                                    {
                                        // Photo
                                        row.ConstantItem(90).Column(photoCol =>
                                        {
                                            if (photoBytes != null && photoBytes.Length > 0)
                                            {
                                                try
                                                {
                                                    photoCol.Item().Height(110).Image(photoBytes).FitArea();
                                                }
                                                catch
                                                {
                                                    // Fallback initiales si image invalide
                                                    photoCol.Item().Height(110).Background(Colors.Grey.Lighten3)
                                                        .AlignCenter().AlignMiddle()
                                                        .Text($"{GetInitials(employee.FullName)}")
                                                        .FontSize(30).Bold().FontColor(Colors.Grey.Darken2);
                                                }
                                            }
                                            else
                                            {
                                                photoCol.Item().Height(110).Background(Colors.Grey.Lighten3)
                                                    .AlignCenter().AlignMiddle()
                                                    .Text($"{GetInitials(employee.FullName)}")
                                                    .FontSize(30).Bold().FontColor(Colors.Grey.Darken2);
                                            }
                                        });

                                        row.ConstantItem(15);

                                        // Infos
                                        row.RelativeItem().Column(info =>
                                        {
                                            info.Item().Text(employee.FullName)
                                                .Bold().FontSize(15).FontColor(Colors.Grey.Darken4);
                                            info.Item().PaddingTop(4).Text(employee.Position)
                                                .FontSize(11).FontColor(companyColor);
                                            if (!string.IsNullOrEmpty(employee.Department))
                                            {
                                                info.Item().Text($"Département : {employee.Department}")
                                                    .FontSize(9).FontColor(Colors.Grey.Darken2);
                                            }

                                            info.Item().PaddingTop(15).Text(text =>
                                            {
                                                text.Span("Matricule : ").FontSize(9).FontColor(Colors.Grey.Darken1);
                                                text.Span(employee.EmployeeNumber ?? "—").FontSize(10).Bold();
                                            });
                                            info.Item().Text(text =>
                                            {
                                                text.Span("Depuis : ").FontSize(9).FontColor(Colors.Grey.Darken1);
                                                text.Span($"{employee.HireDate:MM/yyyy}").FontSize(10);
                                            });
                                            info.Item().Text(text =>
                                            {
                                                text.Span("Valide jusqu'au : ").FontSize(9).FontColor(Colors.Grey.Darken1);
                                                text.Span($"{validUntil:dd/MM/yyyy}").FontSize(10).Bold();
                                            });
                                        });

                                        // QR
                                        row.ConstantItem(15);
                                        row.ConstantItem(80).AlignRight().Column(qrCol =>
                                        {
                                            if (qrBytes.Length > 0)
                                            {
                                                qrCol.Item().Height(80).Image(qrBytes).FitArea();
                                            }
                                            qrCol.Item().AlignCenter().Text("Scanner")
                                                .FontSize(7).FontColor(Colors.Grey.Darken2);
                                        });
                                    });
                                });

                            col.Item().PaddingTop(10).Text(text =>
                            {
                                text.Span("Cette carte est strictement personnelle. ")
                                    .FontSize(8).FontColor(Colors.Grey.Darken2);
                                text.Span("En cas de perte, contacter immédiatement la DRH.")
                                    .FontSize(8).FontColor(Colors.Red.Darken1);
                            });

                            col.Item().PaddingTop(30).AlignCenter().Text(text =>
                            {
                                text.Span($"Émis le {DateTime.UtcNow:dd/MM/yyyy}")
                                    .FontSize(9).FontColor(Colors.Grey.Darken1);
                            });
                        });
                    });
                });

                var pdfBytes = document.GeneratePdf();
                _logger.LogInformation("✅ Badge PDF généré ({Size} KB)", pdfBytes.Length / 1024);
                return pdfBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur génération PDF");
                throw;
            }
        }

        private byte[] GenerateQrCode(string data)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            return qrCode.GetGraphic(20);
        }

        private string GetInitials(string fullName)
        {
            var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
                return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
            return fullName.Substring(0, Math.Min(2, fullName.Length)).ToUpper();
        }
    }
}