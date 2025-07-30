using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Models;

namespace TimeTracker.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<TaskItem> TaskItems { get; set; }
        public DbSet<TimeEntry> TimeEntries { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<ProjectUser> ProjectUsers { get; set; }
        public DbSet<UserTeam> UserTeams { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Client configurations
            modelBuilder.Entity<Client>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // Project configurations
            modelBuilder.Entity<Project>(entity =>
            {
                entity.HasOne(p => p.Client)
                    .WithMany(c => c.Projects)
                    .HasForeignKey(p => p.ClientId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.CreatedByUser)
                    .WithMany(u => u.Projects)
                    .HasForeignKey(p => p.CreatedByUserId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.Property(p => p.HourlyRate)
                    .HasPrecision(10, 2);
            });

            // TaskItem configurations
            modelBuilder.Entity<TaskItem>(entity =>
            {
                entity.HasOne(t => t.Project)
                    .WithMany(p => p.Tasks)
                    .HasForeignKey(t => t.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(t => t.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(t => t.CreatedByUserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // TimeEntry configurations
            modelBuilder.Entity<TimeEntry>(entity =>
            {
                entity.HasOne(te => te.Project)
                    .WithMany(p => p.TimeEntries)
                    .HasForeignKey(te => te.ProjectId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(te => te.TaskItem)
                    .WithMany(t => t.TimeEntries)
                    .HasForeignKey(te => te.TaskItemId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(te => te.User)
                    .WithMany(u => u.TimeEntries)
                    .HasForeignKey(te => te.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(te => te.HourlyRate)
                    .HasPrecision(10, 2);

                entity.Ignore(te => te.Duration);
                entity.Ignore(te => te.DurationHours);
                entity.Ignore(te => te.TotalAmount);
                entity.Ignore(te => te.IsRunning);
            });

            // Team configurations
            modelBuilder.Entity<Team>(entity =>
            {
                entity.HasOne(t => t.Manager)
                    .WithMany()
                    .HasForeignKey(t => t.ManagerId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(t => t.Name).IsUnique();
            });

            // ProjectUser configurations (many-to-many)
            modelBuilder.Entity<ProjectUser>(entity =>
            {
                entity.HasKey(pu => new { pu.ProjectId, pu.UserId });

                entity.HasOne(pu => pu.Project)
                    .WithMany(p => p.ProjectUsers)
                    .HasForeignKey(pu => pu.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pu => pu.User)
                    .WithMany()
                    .HasForeignKey(pu => pu.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // UserTeam configurations (many-to-many)
            modelBuilder.Entity<UserTeam>(entity =>
            {
                entity.HasKey(ut => new { ut.UserId, ut.TeamId });

                entity.HasOne(ut => ut.User)
                    .WithMany(u => u.UserTeams)
                    .HasForeignKey(ut => ut.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ut => ut.Team)
                    .WithMany(t => t.UserTeams)
                    .HasForeignKey(ut => ut.TeamId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}