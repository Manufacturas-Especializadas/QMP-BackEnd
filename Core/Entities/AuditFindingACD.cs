namespace Core.Entities
{
    public class AuditFindingACD
    {
        public int Id { get; set; }

        public int AuditId { get; set; }

        public int StartPointId { get; set; }

        public int EndPointId { get; set; }

        public string PartNumber { get; set; } = null!;

        public int NumberOfPieces { get; set; }

        public string SampleSize { get; set; } = null!;

        public int PackerPayroll { get; set; }

        public bool? ContainerIdMatch { get; set; }

        public byte FrontView { get; set; }

        public byte SideView { get; set; }

        public byte TopView { get; set; }

        public byte IsometricView { get; set; }

        public string? ShopOrder { get; set; }

        public byte WeldingDefects { get; set; }

        public byte PpBom { get; set; }

        public string? ImagesEvidence { get; set; }

        public bool? CompleteProcess { get; set; }

        public bool IsProductConforming { get; set; }

        public virtual AuditDataACD Audit { get; set; } = null!;

        public virtual AuditStartPoint StartPoint { get; set; } = null!;

        public virtual AuditEndPoint EndPoint { get; set; } = null!;
    }
}