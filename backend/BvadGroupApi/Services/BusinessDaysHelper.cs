namespace BvadGroupApi.Services
{
    public static class BusinessDaysHelper
    {
        /// <summary>
        /// Compte le nombre de jours ouvrés entre 2 dates (exclut samedi et dimanche).
        /// </summary>
        public static int CountBusinessDays(DateTime start, DateTime end)
        {
            if (end < start) return 0;

            int count = 0;
            var current = start.Date;
            var endDate = end.Date;

            while (current <= endDate)
            {
                if (current.DayOfWeek != DayOfWeek.Saturday
                    && current.DayOfWeek != DayOfWeek.Sunday)
                {
                    count++;
                }
                current = current.AddDays(1);
            }

            return count;
        }
    }
}