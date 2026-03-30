using Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Line> Lines => Set<Line>();

        public DbSet<Client> Clients => Set<Client>();

        public DbSet<Material> Materials => Set<Material>();

        public DbSet<TypeScrap> TypeScraps => Set<TypeScrap>();

        public DbSet<Shift> Shifts => Set<Shift>();

        public DbSet<Defect> Defects => Set<Defect>();

        public DbSet<Process> Processes => Set<Process>();

        public DbSet<MachineCode> MachineCodes => Set<MachineCode>();

        public DbSet<Scrap> Scraps => Set<Scrap>();

        public DbSet<User> Users => Set<User>();

        public DbSet<DefectRejection> DefectsRejections => Set<DefectRejection>();

        public DbSet<ContainmentAction> ContainmentActions => Set<ContainmentAction>();

        public DbSet<Condition> Conditions => Set<Condition>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserRole>().ToTable("UserRoles");

            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            modelBuilder.Entity<Scrap>(entity =>
            {
                entity.ToTable("Scrap");

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");

                entity.HasOne(d => d.Line)
                    .WithMany(p => p.Scraps)
                    .HasForeignKey(d => d.LineId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(d => d.Defect)
                    .WithMany()
                    .HasForeignKey(d => d.DefectId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Role>().ToTable("Roles");
            modelBuilder.Entity<Line>().ToTable("Lines");
            modelBuilder.Entity<Client>().ToTable("Clients");
            modelBuilder.Entity<TypeScrap>().ToTable("TypeScrap");
            modelBuilder.Entity<MachineCode>().ToTable("MachineCodes");
            modelBuilder.Entity<Material>().ToTable("Material");
            modelBuilder.Entity<Shift>().ToTable("Shifts");
            modelBuilder.Entity<Process>().ToTable("Process");
            modelBuilder.Entity<Defect>().ToTable("Defects");

            modelBuilder.Entity<DefectRejection>(entity =>
            {
                entity.ToTable("DefectsRejections");
                entity.Property(e => e.DefectName).HasMaxLength(70);
            });

            modelBuilder.Entity<Condition>(entity =>
            {
                entity.ToTable("Conditions");
                entity.Property(e => e.ConditionName).HasMaxLength(70);

                entity.HasOne(c => c.DefectRejection)
                    .WithMany(d => d.Conditions)
                    .HasForeignKey(c => c.DefectId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ContainmentAction>(entity =>
            {
                entity.ToTable("ContainmentAction");
                entity.Property(e => e.ContainmentActionName).HasMaxLength(30);
            });
        }
    }
}