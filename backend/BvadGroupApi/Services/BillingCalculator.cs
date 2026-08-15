namespace BvadGroupApi.Services
{
    /// <summary>
    /// Helper pour calculer les totaux HT/TVA/TTC.
    /// </summary>
    public static class BillingCalculator
    {
        public record Totals(decimal SubtotalHT, decimal DiscountAmount, decimal AfterDiscount, decimal VatAmount, decimal TotalTTC);

        public static Totals Calculate(
            IEnumerable<(decimal Quantity, decimal UnitPrice, decimal DiscountPercent)> lines,
            decimal globalDiscountPercent,
            decimal vatRate)
        {
            // Sous-total HT (somme des lignes avec remise par ligne)
            decimal subtotalHT = 0;
            foreach (var line in lines)
            {
                var lineTotal = line.Quantity * line.UnitPrice * (1 - line.DiscountPercent / 100m);
                subtotalHT += lineTotal;
            }

            // Remise globale
            var discountAmount = subtotalHT * (globalDiscountPercent / 100m);
            var afterDiscount = subtotalHT - discountAmount;

            // TVA
            var vatAmount = afterDiscount * (vatRate / 100m);

            // TTC
            var totalTTC = afterDiscount + vatAmount;

            return new Totals(
                Math.Round(subtotalHT, 2),
                Math.Round(discountAmount, 2),
                Math.Round(afterDiscount, 2),
                Math.Round(vatAmount, 2),
                Math.Round(totalTTC, 2)
            );
        }
    }
}