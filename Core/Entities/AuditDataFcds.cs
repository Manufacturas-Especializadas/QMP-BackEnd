namespace Core.Entities
{
    public class AuditDataFcds
    {
        public int Id { get; set; }

        public DateTime? AuditDate { get; set; }

        public int UserId { get; set; }

        public int ShiftId { get; set; }

        public int FcdsProcessId { get; set; }

        public string PartNumber { get; set; } = string.Empty;

        public bool IsProductConforming { get; set; } = true;

        public int? RejectionId { get; set; }

        public virtual User User { get; set; } = null!;

        public virtual Shift Shift { get; set; } = null!;

        public virtual FcdsProcess FcdsProcess { get; set; } = null!;

        public virtual Rejection? Rejection { get; set; }

        public virtual ICollection<Line> Lines { get; set; } = new List<Line>();

        public virtual ICollection<TraceabilityElementFcds> TraceabilityElements { get; set; } = new List<TraceabilityElementFcds>();

        public virtual ICollection<ProcessControlFcds> ProcessControls { get; set; } = new List<ProcessControlFcds>();

        public virtual ICollection<ProductReleasePhysicalCondition> PhysicalConditions { get; set; } = new List<ProductReleasePhysicalCondition>();

        public virtual ICollection<AuditDimensionalSpecFcds> DimensionalSpecs { get; set; } = new List<AuditDimensionalSpecFcds>();

        public virtual ICollection<AuditVisualChecklistFcds> VisualChecklists { get; set; } = new List<AuditVisualChecklistFcds>();
    }
}