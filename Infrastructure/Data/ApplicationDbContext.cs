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
        }
    }
}