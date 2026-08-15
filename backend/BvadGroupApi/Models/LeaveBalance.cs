namespace BvadGroupApi.Models
{
    /// <summary>
    /// Solde de congés d'un employé pour un type et une année.
    /// </summary>
    public class LeaveBalance
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        public Guid LeaveTypeId { get; set; }
        public LeaveType LeaveType { get; set; } = null!;

        public int Year { get; set; }

        /// <summary>Jours alloués (calculés au 1er janvier ou cumulés mensuellement)</summary>
        public decimal AllocatedDays { get; set; }

        /// <summary>Jours déjà consommés</summary>
        public decimal UsedDays { get; set; }

        /// <summary>Jours reportés de l'année précédente</summary>
        public decimal CarriedOverDays { get; set; }

        /// <summary>Ajustements manuels par RH (bonus/malus)</summary>
        public decimal Adjustment { get; set; }

        /// <summary>Solde restant (calculé)</summary>
        public decimal RemainingDays => AllocatedDays + CarriedOverDays + Adjustment - UsedDays;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}