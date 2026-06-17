namespace Core.Entities
{
    public class TraceabilityElementFcds
    {
        public int Id { get; set; }

        public int AuditId { get; set; }

        public string OperatorsPayroll { get; set; } = string.Empty;

        public int CategoryId { get; set; }

        public int? TypeMeasuringEquipmentId { get; set; }

        public string? ShopOrder { get; set; }

        public string? BatchPipe { get; set; }

        public int? PipeDiameterId { get; set; }

        public int? PipeWallId { get; set; }

        public virtual AuditDataFcds Audit { get; set; } = null!;

        public virtual ICollection<MachineCode> MachineCodes { get; set; } = new List<MachineCode>();

        public virtual ICollection<AuditEquipmentSerialFcds> EquipmentSerials { get; set; } = new List<AuditEquipmentSerialFcds>();
    }
}