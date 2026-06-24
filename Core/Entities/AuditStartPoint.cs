namespace Core.Entities
{
    public class AuditStartPoint
    {
        public int Id { get; set; }

        public string ProcessName { get; set; }

        public virtual ICollection<AuditFindingACD> Findings { get; set; } = new HashSet<AuditFindingACD>();
    }
}