namespace Core.Entities
{
    public class AuditDataACD
    {
        public int Id { get; set; }

        public DateTime AuditDate { get; set; }

        public int UserId { get; set; }

        public int ShiftId { get; set; }

        public int? RejectionId { get; set; }

        public virtual User User { get; set; } = null!;

        public virtual Shift Shift { get; set; } = null!;

        public virtual Rejection? Rejection { get; set; }

        public virtual ICollection<Line> Lines { get; set; } = new HashSet<Line>();

        public virtual ICollection<AuditFindingACD> Findings { get; set; } = new HashSet<AuditFindingACD>();
    }
}