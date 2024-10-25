
using Microsoft.EntityFrameworkCore;
using TaskBlaster.TaskManagement.DAL.Entities;

namespace TaskBlaster.TaskManagement.DAL.Data;

public class TaskBlasterDbContext : DbContext
{
    public TaskBlasterDbContext(DbContextOptions<TaskBlasterDbContext> options) : base(options) { }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Entities.Task> Tasks { get; set; }
    public DbSet<Priority> Priorities { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<TaskTag> TaskTags { get; set; }
    public DbSet<Status> Statuses { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<TaskNotification> TaskNotifications { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<TaskTag>()
            .HasKey(aa => new { aa.TaskId, aa.TagId });

        modelBuilder.Entity<TaskTag>()
        .HasOne(tt => tt.Tag)
        .WithMany(t => t.TaskTags)
        .HasForeignKey(tt => tt.TagId)
        .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TaskTag>()
        .HasOne(tt => tt.Task)
        .WithMany(t => t.TaskTags)
        .HasForeignKey(tt => tt.TaskId)
        .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Comment>()
        .HasOne(c => c.Task)
        .WithMany(t => t.Comments)
        .HasForeignKey(t => t.TaskId)
        .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TaskNotification>()
        .HasOne(tn => tn.Task)
        .WithOne(t => t.TaskNotification)
        .HasForeignKey<TaskNotification>(tn => tn.TaskId)
        .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Entities.Task>()
        .HasOne(t => t.Priority)
        .WithMany(p => p.Tasks)
        .HasForeignKey(t => t.PriorityId)
        .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Entities.Task>()
        .HasOne(t => t.Status)
        .WithMany(s => s.Tasks)
        .HasForeignKey(t => t.StatusId)
        .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Entities.Task>()
        .HasOne(t => t.CreatedBy)
        .WithMany(s => s.CreatedTasks)
        .HasForeignKey(t => t.CreatedById)
        .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Entities.Task>()
        .HasOne(t => t.AssignedTo)
        .WithMany(s => s.AssignedTasks)
        .HasForeignKey(t => t.AssignedToId)
        .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Entities.Task>()
            .Property(t => t.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<Comment>()
            .Property(c => c.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<TaskNotification>()
            .Property(tn => tn.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<Priority>()
            .Property(p => p.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<Status>()
            .Property(s => s.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<User>()
            .Property(u => u.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<Tag>()
            .Property(t => t.Id)
            .ValueGeneratedOnAdd();

    }
}