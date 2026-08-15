using BvadGroupApi.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BvadGroupApi.Services
{
    /// <summary>
    /// Helpers partagés pour les PDFs de facturation (header, footer, formatage).
    /// </summary>
    public static class BillingPdfHelper
    {
        public static byte[]? LoadImage(IFileStorageService storage, string? relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return null;
            try
            {
                var fullPath = storage.GetFullPath(relativePath);
                if (!File.Exists(fullPath)) return null;
                var bytes = File.ReadAllBytes(fullPath);
                return bytes.Length > 100 ? bytes : null;
            }
            catch { return null; }
        }

        /// <summary>Format monétaire avec espaces + devise</summary>
        public static string FormatMoney(decimal amount, string currency)
        {
            var formatted = amount.ToString("N0", new System.Globalization.CultureInfo("fr-FR"));
            return $"{formatted} {currency}";
        }

        /// <summary>Rend le header d'un document (logo + infos filiale)</summary>
        public static void RenderHeader(
            IContainer container,
            Company company,
            byte[]? logoBytes,
            string documentTitle,
            string documentNumber,
            DateTime documentDate,
            string companyColor)
        {
            container.Column(col =>
            {
                col.Item().Row(row =>
                {
                    // Logo à gauche
                    row.ConstantItem(80).Column(logoCol =>
                    {
                        if (logoBytes != null)
                        {
                            logoCol.Item().Height(65).Image(logoBytes).FitArea();
                        }
                        else
                        {
                            logoCol.Item().Height(65).AlignCenter().AlignMiddle()
                                .Text(company.Logo ?? "🏢").FontSize(35);
                        }
                    });

                    // Infos filiale au centre
                    row.RelativeItem().PaddingLeft(12).Column(info =>
                    {
                        info.Item().Text(company.Name ?? "BVAD GROUP")
                            .Bold().FontSize(15).FontColor(companyColor);

                        if (!string.IsNullOrEmpty(company.LegalName))
                            info.Item().Text(company.LegalName)
                                .FontSize(8).FontColor(Colors.Grey.Darken2);

                        if (!string.IsNullOrEmpty(company.Slogan))
                            info.Item().PaddingTop(1).Text(company.Slogan)
                                .Italic().FontSize(7).FontColor(Colors.Grey.Darken1);

                        info.Item().PaddingTop(3).Text(text =>
                        {
                            if (!string.IsNullOrEmpty(company.Address))
                                text.Span($"{company.Address} · ").FontSize(7).FontColor(Colors.Grey.Darken2);
                            if (!string.IsNullOrEmpty(company.City))
                                text.Span($"{company.City}, {company.Country}").FontSize(7).FontColor(Colors.Grey.Darken2);
                        });

                        info.Item().Text(text =>
                        {
                            if (!string.IsNullOrEmpty(company.Phone))
                                text.Span($"📞 {company.Phone}  ").FontSize(7).FontColor(Colors.Grey.Darken2);
                            if (!string.IsNullOrEmpty(company.Email))
                                text.Span($"✉ {company.Email}").FontSize(7).FontColor(Colors.Grey.Darken2);
                        });

                        info.Item().Text(text =>
                        {
                            if (!string.IsNullOrEmpty(company.RegistrationNumber))
                                text.Span($"RCCM : {company.RegistrationNumber}  ").FontSize(7).FontColor(Colors.Grey.Darken1);
                            if (!string.IsNullOrEmpty(company.TaxNumber))
                                text.Span($"NIU : {company.TaxNumber}").FontSize(7).FontColor(Colors.Grey.Darken1);
                        });
                    });

                    // Info document à droite
                    row.ConstantItem(140).AlignRight().Column(right =>
                    {
                        right.Item().Text(documentTitle)
                            .Bold().FontSize(14).FontColor(companyColor);
                        right.Item().Text($"N° {documentNumber}")
                            .Bold().FontSize(10).FontColor(Colors.Grey.Darken3);
                        right.Item().Text($"Date : {documentDate:dd/MM/yyyy}")
                            .FontSize(8).FontColor(Colors.Grey.Darken2);
                    });
                });

                col.Item().PaddingVertical(5).LineHorizontal(2).LineColor(companyColor);
            });
        }

        /// <summary>Rend le bloc CLIENT</summary>
        public static void RenderClientBlock(IContainer container, Client client, string companyColor)
        {
            container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(col =>
            {
                col.Item().Text("CLIENT").Bold().FontSize(8).FontColor(companyColor);
                col.Item().PaddingTop(2).Text(client.DisplayName)
                    .Bold().FontSize(11).FontColor(Colors.Grey.Darken4);

                if (!string.IsNullOrEmpty(client.ContactPerson))
                    col.Item().Text($"À l'attention de : {client.ContactPerson}").FontSize(8);

                if (!string.IsNullOrEmpty(client.Address))
                    col.Item().Text(client.Address).FontSize(8);

                if (!string.IsNullOrEmpty(client.City))
                    col.Item().Text($"{client.City}, {client.Country}").FontSize(8);

                if (!string.IsNullOrEmpty(client.Phone))
                    col.Item().Text($"📞 {client.Phone}").FontSize(8);

                if (!string.IsNullOrEmpty(client.Email))
                    col.Item().Text($"✉ {client.Email}").FontSize(8);

                if (!string.IsNullOrEmpty(client.RegistrationNumber))
                    col.Item().PaddingTop(2).Text($"RCCM : {client.RegistrationNumber}")
                        .FontSize(7).FontColor(Colors.Grey.Darken1);

                if (!string.IsNullOrEmpty(client.TaxNumber))
                    col.Item().Text($"NIU : {client.TaxNumber}")
                        .FontSize(7).FontColor(Colors.Grey.Darken1);
            });
        }

        /// <summary>Rend le footer avec signature + cachet</summary>
        public static void RenderSignatureBlock(
            IContainer container,
            Company company,
            byte[]? directorSignBytes,
            byte[]? stampBytes)
        {
            container.Column(col =>
            {
                col.Item().Text("Signature et cachet de l'émetteur")
                    .Bold().FontSize(8).FontColor(Colors.Grey.Darken3);

                col.Item().PaddingTop(3).Height(70).Row(row =>
                {
                    // Signature dirigeant
                    row.RelativeItem().Column(inner =>
                    {
                        if (directorSignBytes != null)
                            inner.Item().MaxHeight(65).Image(directorSignBytes).FitHeight();
                    });

                    // Cachet
                    row.ConstantItem(70).Column(stampCol =>
                    {
                        if (stampBytes != null)
                            stampCol.Item().MaxHeight(70).Image(stampBytes).FitArea();
                    });
                });

                col.Item().LineHorizontal(1).LineColor(Colors.Grey.Darken1);

                if (!string.IsNullOrEmpty(company.DirectorName))
                    col.Item().Text(company.DirectorName)
                        .FontSize(8).Bold().FontColor(Colors.Grey.Darken3);
                if (!string.IsNullOrEmpty(company.DirectorTitle))
                    col.Item().Text(company.DirectorTitle)
                        .FontSize(7).Italic().FontColor(Colors.Grey.Darken1);
            });
        }
    }
}