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
        options.UseSqlServer("Server=localhost\\dev;Database=Testing;Trusted_Connection=true;TrustServerCertificate=True;MultipleActiveResultSets=true"));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var seeder = new DatabaseSeeder(db);
    seeder.Seed();

    var taskFilter = db.TaskFilters.First();
    var searchTerm = "i need a good react developer";
    var formattedSearchTerm = FormatFullTextSearchTerm(searchTerm);
    var containsResults = db.People
        .Where(p => EF.Functions.Contains(p.Bio, formattedSearchTerm))
        .ToList();
    Console.WriteLine($"CONTAINS '{formattedSearchTerm}': {string.Join(", ", containsResults.Select(p => p.Name))}");

    var freeTextResults = db.People
        .Where(p => EF.Functions.FreeText(p.Bio, searchTerm))
        .ToList();
    Console.WriteLine($"FREETEXT '{searchTerm}': {string.Join(", ", freeTextResults.Select(p => p.Name))}");

    var dueDateRange = taskFilter.DueDate?.Value.GetDateRange();

    var filtered = db.TicketTasks.Where(t =>
        dueDateRange != null &&
        t.DueDate.ToDateTime(new TimeOnly()) >= dueDateRange.Value.Start &&
        t.DueDate.ToDateTime(new TimeOnly()) <= dueDateRange.Value.End
    ).ToList();
}

await app.RunAsync();

static string FormatFullTextSearchTerm(string searchTerm)
{
    var stopWords = new HashSet<string> { "a", "an", "and", "are", "as", "at", "be", "but", "by", "for", "if", "in", "into", "is", "it", "no", "not", "of", "on", "or", "such", "that", "the", "their", "then", "there", "these", "they", "this", "to", "was", "will", "with", "i", "you", "he", "she", "we", "they", "me", "him", "her", "us", "them" };

    var words = searchTerm.ToLower().Split(new[] { ' ', '\t' }, System.StringSplitOptions.RemoveEmptyEntries)
        .Where(w => !stopWords.Contains(w))
        .ToList();

    return words.Count > 0 ? string.Join(" & ", words) : searchTerm;
}