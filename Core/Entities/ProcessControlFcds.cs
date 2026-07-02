namespace Core.Entities
{
    public class ProcessControlFcds
    {
        public int Id { get; set; }

        public int AuditId { get; set; }

        public byte MttoValidation { get; set; }

        public byte Realese1stPiece { get; set; }

        public byte Spc { get; set; }

        public byte MaterialCorrectlyIdentified { get; set; }

        public byte IdentifiedMeasuringEquipment { get; set; }

        public byte CalibratedMeasuringEquipment { get; set; }

        public byte ItProcess { get; set; }

        public string? TypeOil { get; set; }

        public byte MeasuringEquipmentAdequate { get; set; }

        public byte MeasuringEquipmentOperatorMatch { get; set; }

        public TimeSpan LastHourOfRelease { get; set; }

        public virtual AuditDataFcds Audit { get; set; } = null!;
    }
}