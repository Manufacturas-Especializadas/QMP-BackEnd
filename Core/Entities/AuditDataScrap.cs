namespace Core.Entities
{
    public class AuditDataScrap
    {
        public int Id { get; set; }

        public DateTime AuditDate { get; set; }

        public int UserId { get; set; }

        public int ShiftId { get; set; }

        public int LeaderPayroll { get; set; }

        public virtual User User { get; set; }

        public virtual Shift Shift { get; set; }

        public virtual ICollection<Line> Lines { get; set; } = new HashSet<Line>();

        public virtual ICollection<AuditFindingScrap> Findings { get; set; } = new HashSet<AuditFindingScrap>();
    }
}