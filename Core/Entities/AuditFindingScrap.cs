namespace Core.Entities
{
    public class AuditFindingScrap
    {
        public int Id { get; set; }

        public int AuditId { get; set; }

        public int TypeScrapId { get; set; }

        public decimal EstimatedWeight { get; set; }

        public byte MaterialCorrectlyIdentified { get; set; }

        public byte MaterialCorrectlySegregated { get; set; }

        public string? UnreportedReason { get; set; }

        public string? ImageEvidence { get; set; }

        public string? SupervisorSignature { get; set; }

        public virtual AuditDataScrap Audit { get; set; } = null!;

        public virtual TypeScrap TypeScrap { get; set; } = null!;

    }
}