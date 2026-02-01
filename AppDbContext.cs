using EfCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

public class AppDbContext : DbContext
{
    public DbSet<Person> People { get; set; }
    public DbSet<TicketTask> TicketTasks { get; set; }
    public DbSet<TaskFilter> TaskFilters { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<RelativeDateRange>()
            .HaveConversion<RelativeDateRangeConverter>();
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskFilter>(b =>
        {
            b.OwnsOne(tf => tf.TagIds).ToJson();
            b.OwnsOne(tf => tf.TaskStatusIds).ToJson();
            b.OwnsOne(tf => tf.DueDate).ToJson();
        });
    }

    public class RelativeDateRangeConverter : ValueConverter<RelativeDateRange, string>
    {
        public RelativeDateRangeConverter() : base(
                v => v.ToString(),
                v => new RelativeDateRange(v))
        {
        }
    }
}
