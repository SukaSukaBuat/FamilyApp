using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace FamilyApp.Common.Databases.FamilyDb
{
    public class FamilyDbContext : IdentityDbContext<TblUser, IdentityRole<Guid>, Guid>
    {
        public FamilyDbContext(DbContextOptions<FamilyDbContext> options)
      : base(options)
        {
        }

        protected virtual DbSet<TblBase> TblBase { get; set; }
        protected virtual DbSet<TblBaseSoftDelete> TblBaseSoftDelete { get; set; }
        protected virtual DbSet<TblPerson> TblPerson { get; set; }
        protected virtual DbSet<TblMarried> TblMarried { get; set; }
        public virtual DbSet<TblAuditTrail> TblAuditTrail { get; set; }
        public virtual DbSet<TblUser> TblUser { get; set; }
        public virtual DbSet<TblLoginSession> TblLoginSession { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresEnum<Gender>();
            modelBuilder.HasPostgresEnum<OuthProvider>();

            modelBuilder.Entity<TblBaseSoftDelete>().HasQueryFilter(x => !x.IsDeleted);

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TblUser>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
            });

            modelBuilder.Entity<TblPerson>(entity =>
            {
                entity.HasOne(d => d.User).WithMany().HasForeignKey(d => d.UserId).OnDelete(DeleteBehavior.NoAction);
                entity.Property(e => e.Name).HasMaxLength(200);
                entity.Property(e => e.Nickname).HasMaxLength(20);
                entity.Property(e => e.IcNumber).HasMaxLength(12);
                entity.Property(e => e.PhoneNumber).HasMaxLength(12);
            });

            modelBuilder.Entity<TblBase>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.UseTpcMappingStrategy();
            });
            modelBuilder.Entity<TblBaseSoftDelete>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasIndex(e => e.IsDeleted);
                entity.UseTpcMappingStrategy();
            });

            modelBuilder.Entity<TblMarried>(entity =>
            {
                entity.HasOne(d => d.Husband).WithMany(p => p.AsHusband).HasForeignKey(d => d.HusbandId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(d => d.Wife).WithMany(p => p.AsWife).HasForeignKey(d => d.WifeId).OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<TblLoginSession>(entity =>
            {
                entity.HasOne(d => d.TblUser).WithMany(p => p.TblLoginSessions).HasForeignKey(d => d.UserId).OnDelete(DeleteBehavior.NoAction);
            });
        }
     }
}
