namespace Core.Entities
{
    public class AuditDimensionalSpecFcds
    {
        public int Id { get; set; }

        public int AuditId { get; set; }

        public string SpecName { get; set; } = string.Empty;

        public string ExpectedValue { get; set; } = string.Empty;

        public string RealValue { get; set; } = string.Empty;

        public virtual AuditDataFcds Audit { get; set; } = null!;
    }
}