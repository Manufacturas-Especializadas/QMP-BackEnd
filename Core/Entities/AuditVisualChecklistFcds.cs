namespace Core.Entities
{
    public class AuditVisualChecklistFcds
    {
        public int Id { get; set; }

        public int AuditId { get; set; }

        public string CheckpointName { get; set; } = string.Empty;

        public byte ResultValue { get; set; }

        public virtual AuditDataFcds Audit { get; set; } = null!;
    }
}