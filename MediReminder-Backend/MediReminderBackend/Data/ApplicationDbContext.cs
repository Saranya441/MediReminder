using MediReminderBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace MediReminderBackend.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<UserProfile> UserProfiles { get; set; } = null!;
    public DbSet<UserSettings> UserSettings { get; set; } = null!;
    public DbSet<Medicine> Medicines { get; set; } = null!;
    public DbSet<MedicineSchedule> MedicineSchedules { get; set; } = null!;
    public DbSet<MedicineHistory> MedicineHistories { get; set; } = null!;
    public DbSet<Prescription> Prescriptions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. User Entity Configurations
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);

            entity.HasIndex(u => u.Email)
                  .IsUnique()
                  .HasFilter("[Email] IS NOT NULL");

            entity.HasIndex(u => u.MobileNumber)
                  .IsUnique()
                  .HasFilter("[MobileNumber] IS NOT NULL");
        });

        // 2. UserProfile (1:1 with User, Cascade Delete)
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.HasOne(p => p.User)
                  .WithOne(u => u.Profile)
                  .HasForeignKey<UserProfile>(p => p.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 3. UserSettings (1:1 with User, Cascade Delete)
        modelBuilder.Entity<UserSettings>(entity =>
        {
            entity.HasKey(s => s.Id);

            entity.HasOne(s => s.User)
                  .WithOne(u => u.Settings)
                  .HasForeignKey<UserSettings>(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 4. Medicine (1:N with User, Cascade Delete)
        modelBuilder.Entity<Medicine>(entity =>
        {
            entity.HasKey(m => m.Id);

            entity.HasOne(m => m.User)
                  .WithMany(u => u.Medicines)
                  .HasForeignKey(m => m.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 5. MedicineSchedule (1:N with Medicine, Cascade Delete)
        modelBuilder.Entity<MedicineSchedule>(entity =>
        {
            entity.HasKey(ms => ms.Id);

            entity.Property(ms => ms.ReminderTime)
                  .HasColumnType("time(0)");

            entity.HasOne(ms => ms.Medicine)
                  .WithMany(m => m.Schedules)
                  .HasForeignKey(ms => ms.MedicineId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 6. MedicineHistory (1:N with User, Restrict on Medicine to protect historical adherence)
        modelBuilder.Entity<MedicineHistory>(entity =>
        {
            entity.HasKey(mh => mh.Id);

            entity.HasIndex(mh => new { mh.UserId, mh.MedicineId, mh.ScheduledDateTime })
                  .IsUnique();

            entity.HasOne(mh => mh.User)
                  .WithMany(u => u.MedicineHistories)
                  .HasForeignKey(mh => mh.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(mh => mh.Medicine)
                  .WithMany(m => m.Histories)
                  .HasForeignKey(mh => mh.MedicineId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // 7. Prescription (1:N with User, Cascade Delete)
        modelBuilder.Entity<Prescription>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.HasOne(p => p.User)
                  .WithMany(u => u.Prescriptions)
                  .HasForeignKey(p => p.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
