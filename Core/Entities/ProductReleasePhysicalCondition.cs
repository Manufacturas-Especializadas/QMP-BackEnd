namespace Core.Entities
{
    public class ProductReleasePhysicalCondition
    {
        public int Id { get; set; }

        public int AuditId { get; set; }

        public byte Brands { get; set; }

        public byte Blows { get; set; }

        public byte Pollution { get; set; }

        public byte Ovality { get; set; }

        public byte Burr { get; set; }

        public byte Warped { get; set; }

        public byte ExcessOil { get; set; }

        public virtual AuditDataFcds Audit { get; set; } = null!;
    }
}