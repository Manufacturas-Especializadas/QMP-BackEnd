namespace Core.Entities
{
    public class FcdsProcess
    {
        public int Id { get; set; }

        public string ProcessName { get; set; } = string.Empty;

        public virtual ICollection<AuditDataFcds> Audits { get; set; } = new List<AuditDataFcds>();
    }
}