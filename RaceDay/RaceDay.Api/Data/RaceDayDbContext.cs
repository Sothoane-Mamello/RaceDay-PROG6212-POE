
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data
{
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Organiser> Organisers => Set<Organiser>();
        public DbSet<Participant> Participants => Set<Participant>();
        public DbSet<Event> Events => Set<Event>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<EventEnrolment> EventEnrolments => Set<EventEnrolment>();
        public DbSet<Result> Results => Set<Result>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User accounts
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.UserId);
                entity.HasIndex(u => u.Email).IsUnique();

                entity.Property(u => u.FullName)
                    .HasMaxLength(120).IsRequired();

                entity.Property(u => u.Email)
                    .HasMaxLength(255).IsRequired();

                entity.Property(u => u.PasswordHash)
                    .HasMaxLength(255).IsRequired();

                entity.Property(u => u.Role)
                    .HasMaxLength(20).IsRequired();

                entity.Property(u => u.ProfileImageUrl)
                    .HasMaxLength(500);

                entity.Property(u => u.CreatedAt)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(u => u.IsActive)
                    .HasDefaultValue(true);

                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_Users_Role",
                    "[Role] IN ('Organiser', 'Participant')"));
            });

            // Organiser profile
            modelBuilder.Entity<Organiser>(entity =>
            {
                entity.HasKey(o => o.OrganiserId);
                entity.Property(o => o.OrganiserId)
                    .ValueGeneratedNever();

                entity.Property(o => o.OrganisationName)
                    .HasMaxLength(150);

                entity.Property(o => o.PhoneNumber)
                    .HasMaxLength(30);

                entity.HasOne(o => o.User)
                    .WithOne(u => u.Organiser)
                    .HasForeignKey<Organiser>(o => o.OrganiserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Participant profile
            modelBuilder.Entity<Participant>(entity =>
            {
                entity.HasKey(p => p.ParticipantId);
                entity.Property(p => p.ParticipantId)
                    .ValueGeneratedNever();

                entity.Property(p => p.PhoneNumber)
                    .HasMaxLength(30);

                entity.Property(p => p.EmergencyContact)
                    .HasMaxLength(120);

                entity.HasOne(p => p.User)
                    .WithOne(u => u.Participant)
                    .HasForeignKey<Participant>(p => p.ParticipantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Events
            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasKey(e => e.EventId);

                entity.Property(e => e.Name)
                    .HasMaxLength(150).IsRequired();

                entity.Property(e => e.Description)
                    .HasMaxLength(1000).IsRequired();

                entity.Property(e => e.Location)
                    .HasMaxLength(250).IsRequired();

                entity.Property(e => e.EventType)
                    .HasMaxLength(10).IsRequired();

                entity.Property(e => e.BannerImageUrl)
                    .HasMaxLength(500);

                entity.Property(e => e.DistanceKm)
                    .HasPrecision(6, 2);

                entity.HasOne(e => e.Organiser)
                    .WithMany(o => o.Events)
                    .HasForeignKey(e => e.OrganiserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.ToTable(t =>
                {
                    t.HasCheckConstraint(
                        "CK_Events_Distance",
                        "[DistanceKm] > 0");

                    t.HasCheckConstraint(
                        "CK_Events_Type",
                        "[EventType] IN ('Run', 'Walk', 'Cycle')");
                });
            });

            // Event categories
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(c => c.CategoryId);

                entity.Property(c => c.Name)
                    .HasMaxLength(100).IsRequired();

                entity.Property(c => c.CategoryType)
                    .HasMaxLength(20).IsRequired();

                entity.Property(c => c.Description)
                    .HasMaxLength(300);

                entity.Property(c => c.DistanceKm)
                    .HasPrecision(6, 2);

                entity.HasIndex(c => new { c.EventId, c.Name })
                    .IsUnique();

                entity.HasAlternateKey(c => new
                {
                    c.CategoryId,
                    c.EventId
                });

                entity.HasOne(c => c.Event)
                    .WithMany(e => e.Categories)
                    .HasForeignKey(c => c.EventId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.ToTable(t =>
                {
                    t.HasCheckConstraint(
                        "CK_Categories_Type",
                        "[CategoryType] IN ('Age', 'Distance')");

                    t.HasCheckConstraint(
                        "CK_Categories_Age",
                        "([MinAge] IS NULL OR [MinAge] >= 0) " +
                        "AND ([MaxAge] IS NULL OR [MaxAge] >= 0) " +
                        "AND ([MinAge] IS NULL OR [MaxAge] IS NULL " +
                        "OR [MaxAge] >= [MinAge])");

                    t.HasCheckConstraint(
                        "CK_Categories_Distance",
                        "[DistanceKm] IS NULL OR [DistanceKm] > 0");

                    t.HasCheckConstraint(
                        "CK_Categories_TypeFields",
                        "([CategoryType] = 'Age' " +
                        "AND [DistanceKm] IS NULL " +
                        "AND ([MinAge] IS NOT NULL OR [MaxAge] IS NOT NULL)) " +
                        "OR ([CategoryType] = 'Distance' " +
                        "AND [DistanceKm] IS NOT NULL " +
                        "AND [MinAge] IS NULL AND [MaxAge] IS NULL)");
                });
            });

            // Event enrolments
            modelBuilder.Entity<EventEnrolment>(entity =>
            {
                entity.HasKey(e => e.EnrolmentId);

                entity.Property(e => e.Status)
                    .HasMaxLength(20).IsRequired();

                entity.HasIndex(e => new
                {
                    e.ParticipantId,
                    e.EventId
                }).IsUnique();

                entity.HasOne(e => e.Participant)
                    .WithMany(p => p.EventEnrolments)
                    .HasForeignKey(e => e.ParticipantId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Event)
                    .WithMany(ev => ev.EventEnrolments)
                    .HasForeignKey(e => e.EventId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Category)
                    .WithMany(c => c.EventEnrolments)
                    .HasForeignKey(e => new
                    {
                        e.CategoryId,
                        e.EventId
                    })
                    .HasPrincipalKey(c => new
                    {
                        c.CategoryId,
                        c.EventId
                    })
                    .OnDelete(DeleteBehavior.Restrict);

                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_EventEnrolments_Status",
                    "[Status] IN ('Pending', 'Confirmed', 'Cancelled')"));
            });

            // Event results
            modelBuilder.Entity<Result>(entity =>
            {
                entity.HasKey(r => r.ResultId);

                entity.HasOne(r => r.EventEnrolment)
                    .WithOne(e => e.Result)
                    .HasForeignKey<Result>(r => r.EnrolmentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.ToTable(t =>
                {
                    t.HasCheckConstraint(
                        "CK_Results_FinishTime",
                        "[FinishTimeSeconds] > 0");

                    t.HasCheckConstraint(
                        "CK_Results_Position",
                        "[FinishingPosition] > 0");
                });
            });
        }
    }
}
