using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();

        public DbSet<Rejection> Rejections => Set<Rejection>();

        public DbSet<DefectRejection> DefectsRejections => Set<DefectRejection>();
        public DbSet<Condition> Conditions => Set<Condition>();
        public DbSet<ContainmentAction> ContainmentActions => Set<ContainmentAction>();

        public DbSet<Line> Lines => Set<Line>();
        public DbSet<Client> Clients => Set<Client>();

        public DbSet<Material> Materials => Set<Material>();
        public DbSet<TypeScrap> TypeScraps => Set<TypeScrap>();
        public DbSet<Shift> Shifts => Set<Shift>();
        public DbSet<Process> Processes => Set<Process>();
        public DbSet<MachineCode> MachineCodes => Set<MachineCode>();
        public DbSet<Scrap> Scraps => Set<Scrap>();
        public DbSet<ScrapDetail> ScrapDetails => Set<ScrapDetail>();
        public DbSet<Defect> Defects => Set<Defect>();

        public DbSet<CategoryOperator> CategoryOperator => Set<CategoryOperator>();
        public DbSet<TypeMeasuringEquipment> TypeMeasuringEquipment => Set<TypeMeasuringEquipment>();
        public DbSet<PipeDiameters> PipeDiameters => Set<PipeDiameters>();
        public DbSet<WallsOfDiameters> WallsOfDiameters => Set<WallsOfDiameters>();

        public DbSet<FcdsProcess> FcdsProcesses => Set<FcdsProcess>();
        public DbSet<AuditDataFcds> AuditDataFcds => Set<AuditDataFcds>();
        public DbSet<TraceabilityElementFcds> TraceabilityElementsFcds => Set<TraceabilityElementFcds>();
        public DbSet<AuditEquipmentSerialFcds> AuditEquipmentSerialsFcds => Set<AuditEquipmentSerialFcds>();
        public DbSet<ProcessControlFcds> ProcessControlsFcds => Set<ProcessControlFcds>();
        public DbSet<ProductReleasePhysicalCondition> ProductReleasePhysicalConditions => Set<ProductReleasePhysicalCondition>();
        public DbSet<AuditDimensionalSpecFcds> AuditDimensionalSpecsFcds => Set<AuditDimensionalSpecFcds>();
        public DbSet<AuditVisualChecklistFcds> AuditVisualChecklistFCDS => Set<AuditVisualChecklistFcds>();

        public virtual DbSet<AuditDataScrap> AuditDataScraps => Set<AuditDataScrap>();
        public virtual DbSet<AuditFindingScrap> AuditFindingsScrap => Set<AuditFindingScrap>();

        public DbSet<AuditStartPoint> AuditStartPoints { get; set; } = null!;
        public DbSet<AuditEndPoint> AuditEndPoints { get; set; } = null!;
        public DbSet<AuditDataACD> AuditDataACDs { get; set; } = null!;
        public DbSet<AuditFindingACD> AuditFindingsACDs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.ToTable("UserRoles");

                entity.HasKey(ur => new { ur.UserId, ur.RoleId });

                entity.HasOne(ur => ur.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(ur => ur.UserId);

                entity.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId);
            });

            modelBuilder.Entity<Role>().ToTable("Roles");
            modelBuilder.Entity<User>().ToTable("Users");

            modelBuilder.Entity<Line>().ToTable("Lines");
            modelBuilder.Entity<Client>().ToTable("Clients");
            modelBuilder.Entity<Material>().ToTable("Material");
            modelBuilder.Entity<TypeScrap>().ToTable("TypeScrap");
            modelBuilder.Entity<Shift>().ToTable("Shifts");
            modelBuilder.Entity<Process>().ToTable("Process");
            modelBuilder.Entity<MachineCode>().ToTable("MachineCodes");
            modelBuilder.Entity<Defect>().ToTable("Defects");

            modelBuilder.Entity<CategoryOperator>().ToTable("CategoryOperator");
            modelBuilder.Entity<TypeMeasuringEquipment>().ToTable("TypeMeasuringEquipment");
            modelBuilder.Entity<PipeDiameters>().ToTable("PipeDiameters");

            modelBuilder.Entity<DefectRejection>(entity =>
            {
                entity.ToTable("DefectsRejections");

                entity.Property(e => e.DefectName)
                    .HasMaxLength(70);
            });

            modelBuilder.Entity<Condition>(entity =>
            {
                entity.ToTable("Conditions");

                entity.Property(e => e.ConditionName)
                    .HasMaxLength(70);

                entity.HasOne(c => c.DefectRejection)
                    .WithMany(d => d.Conditions)
                    .HasForeignKey(c => c.DefectId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ContainmentAction>(entity =>
            {
                entity.ToTable("ContainmentAction");

                entity.Property(e => e.ContainmentActionName)
                    .HasMaxLength(30);
            });

            modelBuilder.Entity<Rejection>(entity =>
            {
                entity.ToTable("Rejections");

                entity.Property(r => r.CreatedAt)
                    .HasDefaultValueSql("GETDATE()");

                entity.HasOne<DefectRejection>(r => r.Defect)
                    .WithMany()
                    .HasForeignKey(r => r.DefectId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.Condition)
                    .WithMany()
                    .HasForeignKey(r => r.ConditionId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(r => r.Line)
                    .WithMany()
                    .HasForeignKey(r => r.LineId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.Client)
                    .WithMany()
                    .HasForeignKey(r => r.ClientId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.ContainmentAction)
                    .WithMany()
                    .HasForeignKey(r => r.ContainmentActionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.User)
                    .WithMany()
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });


            modelBuilder.Entity<Scrap>(entity =>
            {
                entity.ToTable("Scrap");

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETDATE()");

                entity.HasOne(d => d.Line)
                    .WithMany(p => p.Scraps)
                    .HasForeignKey(d => d.LineId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<ScrapDetail>(entity =>
            {
                entity.ToTable("ScrapDetail");

                entity.HasOne(d => d.Scrap)
                    .WithMany(p => p.ScrapDetails)
                    .HasForeignKey(d => d.ScrapId)
                    .OnDelete(DeleteBehavior.Cascade); // Si borras la cabecera, se borran los detalles

                entity.HasOne(d => d.Defect)
                    .WithMany()
                    .HasForeignKey(d => d.DefectId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<FcdsProcess>().ToTable("FCDSProcesses");

            modelBuilder.Entity<AuditDataFcds>(entity =>
            {
                entity.ToTable("AuditDataFCDS");

                entity.Property(e => e.AuditDate).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IsProductConforming).HasDefaultValue(true);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Shift)
                    .WithMany()
                    .HasForeignKey(e => e.ShiftId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.FcdsProcess)
                    .WithMany(p => p.Audits)
                    .HasForeignKey(e => e.FcdsProcessId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Rejection)
                    .WithMany()
                    .HasForeignKey(e => e.RejectionId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(a => a.Lines)
                    .WithMany()
                    .UsingEntity<Dictionary<string, object>>(
                        "AuditLinesFCDS",
                        l => l.HasOne<Line>().WithMany().HasForeignKey("LineId").OnDelete(DeleteBehavior.Cascade),
                        a => a.HasOne<AuditDataFcds>().WithMany().HasForeignKey("AuditId").OnDelete(DeleteBehavior.Cascade)
                    );
            });

            modelBuilder.Entity<TraceabilityElementFcds>(entity =>
            {
                entity.ToTable("TraceabilityElementsFCDS");

                entity.HasOne(e => e.Audit)
                    .WithMany(a => a.TraceabilityElements)
                    .HasForeignKey(e => e.AuditId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(t => t.MachineCodes)
                    .WithMany()
                    .UsingEntity<Dictionary<string, object>>(
                        "AuditMachinesFCDS",
                        m => m.HasOne<MachineCode>().WithMany().HasForeignKey("MachineCodeId").OnDelete(DeleteBehavior.Cascade),
                        t => t.HasOne<TraceabilityElementFcds>().WithMany().HasForeignKey("TraceabilityId").OnDelete(DeleteBehavior.Cascade)
                    );
            });

            modelBuilder.Entity<AuditEquipmentSerialFcds>(entity =>
            {
                entity.ToTable("AuditEquipmentSerialsFCDS");

                entity.HasOne(e => e.Traceability)
                    .WithMany(t => t.EquipmentSerials)
                    .HasForeignKey(e => e.TraceabilityId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ProcessControlFcds>(entity =>
            {
                entity.ToTable("ProcessControlsFCDS");

                entity.Property(e => e.MttoValidation).HasColumnName("mttoValidation");
                entity.Property(e => e.Realese1stPiece).HasColumnName("realese1stPiece");
                entity.Property(e => e.Spc).HasColumnName("spc");
                entity.Property(e => e.MaterialCorrectlyIdentified).HasColumnName("materialCorrectlyIdentified");

                entity.Property(e => e.IdentifiedMeasuringEquipment).HasColumnName("identifiedMeasuringEquipment");
                entity.Property(e => e.CalibratedMeasuringEquipment).HasColumnName("calibratedMeasuringEquipment");

                entity.Property(e => e.MeasuringEquipmentAdequate).HasColumnName("measuringEquipmentAdequate");
                entity.Property(e => e.MeasuringEquipmentOperatorMatch).HasColumnName("measuringEquipmentOperatorMatch");

                entity.Property(e => e.ItProcess).HasColumnName("itProcess");
                entity.Property(e => e.TypeOil).HasColumnName("typeOil");

                entity.Property(e => e.LastHourOfRelease)
                      .HasColumnName("lastHourOfRelease")
                      .HasColumnType("time");

                entity.HasOne(e => e.Audit)
                    .WithMany(a => a.ProcessControls)
                    .HasForeignKey(e => e.AuditId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ProductReleasePhysicalCondition>(entity =>
            {
                entity.ToTable("ProductReleasePhysicalCondition");

                entity.HasOne(e => e.Audit)
                    .WithMany(a => a.PhysicalConditions)
                    .HasForeignKey(e => e.AuditId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<AuditDimensionalSpecFcds>(entity =>
            {
                entity.ToTable("AuditDimensionalSpecsFCDS");

                entity.HasOne(e => e.Audit)
                    .WithMany(a => a.DimensionalSpecs)
                    .HasForeignKey(e => e.AuditId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<AuditVisualChecklistFcds>(entity =>
            {
                entity.ToTable("AuditVisualChecklistFCDS");

                entity.HasOne(e => e.Audit)
                    .WithMany(a => a.VisualChecklists)
                    .HasForeignKey(e => e.AuditId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<AuditDataScrap>(entity =>
            {
                entity.ToTable("AuditDataScrap");

                entity.Property(e => e.AuditDate).HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Shift)
                    .WithMany()
                    .HasForeignKey(e => e.ShiftId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(a => a.Lines)
                    .WithMany()
                    .UsingEntity<Dictionary<string, object>>(
                        "AudtLinesScrap",
                        l => l.HasOne<Line>().WithMany().HasForeignKey("LineId").OnDelete(DeleteBehavior.Cascade),
                        a => a.HasOne<AuditDataScrap>().WithMany().HasForeignKey("AuditId").OnDelete(DeleteBehavior.Cascade)
                    );
            });

            modelBuilder.Entity<AuditStartPoint>(entity =>
            {
                entity.ToTable("AuditStartPoint");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ProcessName).HasColumnName("processName").HasMaxLength(50).IsRequired();
            });

            modelBuilder.Entity<AuditEndPoint>(entity =>
            {
                entity.ToTable("AuditEndPoint");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ProcessName).HasColumnName("processName").HasMaxLength(50).IsRequired();
            });

            modelBuilder.Entity<AuditDataACD>(entity =>
            {
                entity.ToTable("AuditDataACD");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AuditDate).HasColumnName("auditDate").HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UserId).HasColumnName("userId");
                entity.Property(e => e.ShiftId).HasColumnName("shiftId");
                entity.Property(e => e.RejectionId).HasColumnName("rejectionId").IsRequired(false);

                entity.HasOne(d => d.Rejection)
                    .WithMany()
                    .HasForeignKey(d => d.RejectionId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(d => d.Lines)
                    .WithMany()
                    .UsingEntity<Dictionary<string, object>>(
                        "AuditLinesACD",
                        l => l.HasOne<Line>().WithMany().HasForeignKey("LineId").OnDelete(DeleteBehavior.Cascade),
                        a => a.HasOne<AuditDataACD>().WithMany().HasForeignKey("AuditId").OnDelete(DeleteBehavior.Cascade),
                        je =>
                        {
                            je.HasKey("AuditId", "LineId");
                            je.ToTable("AuditLinesACD");
                        });
            });

            modelBuilder.Entity<AuditFindingACD>(entity =>
            {
                entity.ToTable("AuditFindingsACD");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AuditId).HasColumnName("auditId");
                entity.Property(e => e.StartPointId).HasColumnName("startPointId");
                entity.Property(e => e.EndPointId).HasColumnName("endPointId");

                entity.Property(e => e.PartNumber).HasColumnName("partNumber").HasMaxLength(100).IsRequired();
                entity.Property(e => e.NumberOfPieces).HasColumnName("numberOfPieces");
                entity.Property(e => e.SampleSize).HasColumnName("sampleSize").HasMaxLength(50).IsRequired();
                entity.Property(e => e.PackerPayroll).HasColumnName("packerPayroll");

                entity.Property(e => e.ShopOrder)
                  .HasColumnName("shopOrder")
                  .HasMaxLength(100);

                entity.Property(e => e.WeldingDefects)
                      .HasColumnName("weldingDefects");

                entity.Property(e => e.PpBom)
                      .HasColumnName("ppBom");

                entity.Property(e => e.ImagesEvidence)
                      .HasColumnName("imagesEvidence");

                entity.Property(e => e.ContainerIdMatch).HasColumnName("containerIdMatch");
                entity.Property(e => e.FrontView).HasColumnName("frontView");
                entity.Property(e => e.SideView).HasColumnName("sideView");
                entity.Property(e => e.TopView).HasColumnName("topView");
                entity.Property(e => e.IsometricView).HasColumnName("isometricView");
                entity.Property(e => e.CompleteProcess).HasColumnName("completeProcess");
                entity.Property(e => e.IsProductConforming).HasColumnName("isProductConforming").HasDefaultValue(true);

                entity.HasOne(d => d.Audit)
                    .WithMany(p => p.Findings)
                    .HasForeignKey(d => d.AuditId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.StartPoint)
                    .WithMany(p => p.Findings)
                    .HasForeignKey(d => d.StartPointId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.EndPoint)
                    .WithMany(p => p.Findings)
                    .HasForeignKey(d => d.EndPointId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });
        }
    }
}