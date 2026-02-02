using EfCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Linq;

var builder = Host.CreateDefaultBuilder(args);

// Add EF Core
builder.ConfigureServices(services =>
{
    services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=Testing;Trusted_Connection=true;MultipleActiveResultSets=true"));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    db.Database.EnsureDeleted();
    db.Database.EnsureCreated();

    var taskFilter = new TaskFilter
    {
        Name = "Sample Filter",
        DueDate = new FilterCriterion<RelativeDateRange>
        {
            Operator = ComparisonOperator.In,
            Value = new RelativeDateRange("T+10")
        },
        TagIds = new FilterCriterion<List<int>>
        {
            Operator = ComparisonOperator.NotEqual,
            Value = [1, 2]
        }
    };

    db.TaskFilters.Add(taskFilter);

    var tasks = new List<TicketTask>
    {
        new TicketTask { Title = "Task 1", DueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-14)), TagIds = new List<int> { 3 } },
        new TicketTask { Title = "Task 2", DueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-24)), TagIds = new List<int> { 1, 2 } },
        new TicketTask { Title = "Task 3", DueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1)), TagIds = new List<int> { 4 } },
        new TicketTask { Title = "Task 3", DueDate = DateOnly.FromDateTime(DateTime.Now), TagIds = new List<int> { 4 } }
    };
    db.TicketTasks.AddRange(tasks);
    db.SaveChanges();

    var dueDateRange = taskFilter.DueDate?.Value.GetDateRange();

    var filtered = db.TicketTasks.Where(t =>
        dueDateRange != null &&
        t.DueDate.ToDateTime(new TimeOnly()) >= dueDateRange.Value.Start &&
        t.DueDate.ToDateTime(new TimeOnly()) <= dueDateRange.Value.End
    ).ToList();
}

await app.RunAsync();

