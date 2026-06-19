namespace Core.Entities
{
    public class AuditEquipmentSerialFcds
    {
        public int Id { get; set; }

        public int TraceabilityId { get; set; }

        public string EquipmentSerial { get; set; } = string.Empty;

        public virtual TraceabilityElementFcds Traceability { get; set; } = null!;
    }
}