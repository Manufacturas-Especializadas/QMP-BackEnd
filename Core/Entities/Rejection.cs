
namespace Core.Entities
{
    public class Rejection
    {
        public int Id { get; set; }

        public int? Folio { get; set; }

        public string Inspector { get; set; } = string.Empty;

        public string? PartNumber { get; set; }

        public int NumberOfPieces { get; set; }

        public int? OperatorPayroll { get; set; }

        public string? Description { get; set; }

        public string? Image { get; set; }

        public string? InformedSignature { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        public int? DefectId { get; set; }

        public int? ConditionId { get; set; }

        public int? LineId { get; set; }

        public int? ClientId { get; set; }

        public int? UserId { get; set; }

        public int? ContainmentActionId { get; set; }

        public virtual DefectRejection? Defect { get; set; }

        public virtual Condition? Condition { get; set; }

        public virtual Line? Line { get; set; }

        public virtual Client? Client { get; set; }

        public virtual ContainmentAction? ContainmentAction { get; set; }

        public virtual User User { get; set; }
    }
}