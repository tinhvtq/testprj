using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ReservationSystem.Models;

namespace ReservationSystem.Models
{
    public class DbConnectionClass : DbContext
    {
        public DbConnectionClass(DbContextOptions<DbConnectionClass> options)
            : base(options)
        {

        }
        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Machine> Machines { get; set; }
        public DbSet<CanUseMachine> CanUseMachines { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<MachineAdmin> MachineAdmins { get; set; }
        public DbSet<Infomation> Infomations { get; set; }
        public DbSet<CustomField> CustomFields { get; set; }
        public DbSet<ManualFile> ManualFiles { get; set; }
        public DbSet<MaintenanceRecord> MaintenanceRecords { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<CustomFieldValue> CustomFieldValues { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CanUseMachine>().HasKey(sc => new { sc.UserId, sc.MachineId });
            modelBuilder.Entity<CanUseMachine>().HasOne<User>(sc => sc.User).WithMany(s => s.CanUseMachines).HasForeignKey(sc => sc.UserId);
            modelBuilder.Entity<CanUseMachine>().HasOne<Machine>(sc => sc.Machine).WithMany(s => s.CanUseMachines).HasForeignKey(sc => sc.MachineId);

            modelBuilder.Entity<MachineAdmin>().HasKey(sc => new { sc.UserId, sc.MachineId });
            modelBuilder.Entity<MachineAdmin>().HasOne<User>(sc => sc.User).WithMany(s => s.MachineAdmins).HasForeignKey(sc => sc.UserId);
            modelBuilder.Entity<MachineAdmin>().HasOne<Machine>(sc => sc.Machine).WithMany(s => s.MachineAdmins).HasForeignKey(sc => sc.MachineId);

            modelBuilder.Entity<User>().HasOne(u => u.Group).WithMany(g => g.Users).HasForeignKey(u => u.GroupId);
            modelBuilder.Entity<Machine>().HasOne(m => m.Category).WithMany(c => c.Machines).HasForeignKey(m => m.CategoryId);

            modelBuilder.Entity<Group>().HasMany<Group>(g => g.Groups).WithOne().HasForeignKey(p => p.ParentGroupId);
            modelBuilder.Entity<CustomField>().HasOne(c => c.Machine).WithMany(m => m.CustomFields).HasForeignKey(c => c.MachineId);
            modelBuilder.Entity<CustomFieldValue>().HasOne(c => c.Reservation).WithMany(m => m.CustomFieldValues).HasForeignKey(c => c.ReserveId);
            modelBuilder.Entity<CustomFieldValue>().HasOne(c => c.CustomField).WithMany(m => m.CustomFieldValues).HasForeignKey(c => c.FieldId);
        }

    }
}
