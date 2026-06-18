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

                entity.HasOne(e => e.Audit)
                    .WithMany(a => a.ProcessControls)
                    .HasForeignKey(e => e.AuditId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(e => e.LastHourOfRelease).HasColumnType("time");
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
        }
    }
}