using EfCore.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

public class DatabaseSeeder
{
    private readonly AppDbContext _db;

    public DatabaseSeeder(AppDbContext db)
    {
        _db = db;
    }

    public void Seed()
    {
        _db.Database.EnsureDeleted();
        _db.Database.EnsureCreated();

        SeedTaskFilters();
        SeedTicketTasks();
        SetupFullTextSearch();
        SeedPeople();
    }

    private void SeedTaskFilters()
    {
        var taskFilter = new TaskFilter
        {
            Name = "Sample Filter",
            DueDate = new FilterCriterion<RelativeDateRange>
            {
                Operator = FilterOperator.In,
                Value = new RelativeDateRange("T+10")
            },
            TagIds = new FilterCriterion<List<int>>
            {
                Operator = FilterOperator.NotEqual,
                Value = [1, 2]
            }
        };

        _db.TaskFilters.Add(taskFilter);
        _db.SaveChanges();
    }

    private void SeedTicketTasks()
    {
        var tasks = new List<TicketTask>
        {
            new TicketTask { Title = "Task 1", DueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-14)), TagIds = new List<int> { 3 } },
            new TicketTask { Title = "Task 2", DueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-24)), TagIds = new List<int> { 1, 2 } },
            new TicketTask { Title = "Task 3", DueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1)), TagIds = new List<int> { 4 } },
            new TicketTask { Title = "Task 3", DueDate = DateOnly.FromDateTime(DateTime.Now), TagIds = new List<int> { 4 } }
        };

        _db.TicketTasks.AddRange(tasks);
        _db.SaveChanges();
    }

    private void SetupFullTextSearch()
    {
        _db.Database.ExecuteSqlRaw("""
            IF NOT EXISTS (SELECT 1 FROM sys.fulltext_catalogs WHERE name = 'FTCatalog')
                CREATE FULLTEXT CATALOG FTCatalog AS DEFAULT;
            
            IF NOT EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('People'))
                CREATE FULLTEXT INDEX ON People(Name, Bio) KEY INDEX PK_People ON FTCatalog;
            """);
    }

    private void SeedPeople()
    {
        var firstNames = new[] { "John", "Jane", "Bob", "Alice", "Charlie", "Diana", "Eve", "Frank", "Grace", "Henry", "Iris", "Jack", "Karen", "Leo", "Megan", "Nathan", "Olivia", "Paul", "Quinn", "Rachel" };
        var lastNames = new[] { "Smith", "Doe", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson", "Thomas", "Taylor", "Moore", "Jackson" };
        var jobTitles = new[] { "Software Engineer", "Data Scientist", "Developer", "Designer", "Manager", "Analyst", "Architect", "DevOps Engineer", "QA Engineer", "Product Manager", "Consultant", "Specialist", "Coordinator", "Administrator", "Director" };
        var technologies = new[] { ".NET", "React", "Python", "Java", "JavaScript", "TypeScript", "Angular", "Vue.js", "AWS", "Azure", "Google Cloud", "Docker", "Kubernetes", "SQL", "NoSQL" };
        var skills = new[] { "specializing", "expertise in", "proficient in", "skilled in", "experienced with", "working with", "focused on" };
        var industries = new[] { "FinTech", "Healthcare", "E-commerce", "Education", "Manufacturing", "Logistics", "Entertainment", "Retail", "Telecommunications", "Energy" };
        var methodologies = new[] { "Agile", "Scrum", "Waterfall", "DevOps", "Test-Driven Development", "Microservices", "Cloud-Native", "Event-Driven", "CI/CD", "Containerized" };
        var seniority = new[] { "junior", "mid-level", "senior", "principal", "staff" };
        var bioTemplates = new[] { 
            "{jt} with {seniority} expertise, {skill} {tech} in {industry} environments", 
            "{seniority} {jt} passionate about {tech}, bringing {skill} to {industry} solutions",
            "{jt} experienced in {methodology} approaches, {skill} {tech} across {industry}",
            "{seniority} professional in {industry}, {skill} {tech} and {methodology} practices",
            "{jt} specializing in {industry} transformation using {tech} and {methodology}"
        };

        var people = firstNames
            .SelectMany((fn, i1) => lastNames.Select((ln, i2) => new { fn, ln, index = i1 * lastNames.Length + i2 }))
            .SelectMany(x => jobTitles.Select((jt, i3) => new { x.fn, x.ln, jt, x.index, i3 }))
            .SelectMany(x => technologies.Select((tech, i4) => new { x.fn, x.ln, x.jt, tech, x.index, i4 }))
            .SelectMany(x => skills.Select((skill, i5) => new { x.fn, x.ln, x.jt, x.tech, skill, x.index, i5 }))
            .SelectMany(x => industries.Select((ind, i6) => new { x.fn, x.ln, x.jt, x.tech, x.skill, ind, x.index, i6 }))
            .SelectMany(x => methodologies.Select((method, i7) => new { x.fn, x.ln, x.jt, x.tech, x.skill, x.ind, method, x.index, i6 = x.i6, i7 }))
            .SelectMany(x => seniority.Select((sen, i8) => new { x.fn, x.ln, x.jt, x.tech, x.skill, x.ind, x.method, sen, x.index, i6 = x.i6, i8 }))
            .SelectMany(x => bioTemplates.Select((template, i9) => new Person
            {
                Name = $"{x.fn} {x.ln}_{x.index * 1000 + x.i6 * 100 + x.i8 + i9}",
                Bio = template
                    .Replace("{jt}", x.jt)
                    .Replace("{tech}", x.tech)
                    .Replace("{skill}", x.skill)
                    .Replace("{industry}", x.ind)
                    .Replace("{methodology}", x.method)
                    .Replace("{seniority}", x.sen)
            }))
            .Take(100000)
            .ToList();

        _db.People.AddRange(people);
        _db.SaveChanges();
    }
}
