using Server.Models;

namespace Server.Services;

public sealed class ProjectService
{
    private static readonly HashSet<string> Technologies = new(StringComparer.OrdinalIgnoreCase)
    {
        "csharp", "javascript", "python"
    };

    private static readonly HashSet<string> Statuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "active", "archived"
    };

    private readonly List<Project> projects = new();
    private int nextId = 1;
    private readonly object sync = new();

    public IReadOnlyList<Project> GetAll(string? technology = null)
    {
        lock (sync)
        {
            IEnumerable<Project> result = projects;
            if (!string.IsNullOrWhiteSpace(technology))
            {
                result = result.Where(p => p.Technology.Equals(technology.Trim(), StringComparison.OrdinalIgnoreCase));
            }
            return result.Select(Clone).ToList();
        }
    }

    public Project? GetById(int id)
    {
        lock (sync)
        {
            var project = projects.FirstOrDefault(p => p.Id == id);
            return project is null ? null : Clone(project);
        }
    }

    public Project Create(string title, string technology)
    {
        ValidateTitle(title);
        ValidateTechnology(technology);

        lock (sync)
        {
            var project = new Project
            {
                Id = nextId++,
                Title = title.Trim(),
                Technology = technology.Trim().ToLowerInvariant(),
                Status = "active"
            };
            projects.Add(project);
            return Clone(project);
        }
    }

    public bool Update(int id, string title, string technology, string status)
    {
        ValidateTitle(title);
        ValidateTechnology(technology);
        ValidateStatus(status);

        lock (sync)
        {
            var project = projects.FirstOrDefault(p => p.Id == id);
            if (project is null)
                return false;

            project.Title = title.Trim();
            project.Technology = technology.Trim().ToLowerInvariant();
            project.Status = status.Trim().ToLowerInvariant();
            return true;
        }
    }

    public IReadOnlyDictionary<string, int> GetTechnologyCounts()
    {
        lock (sync)
        {
            return new Dictionary<string, int>
            {
                ["csharp"] = projects.Count(p => p.Technology == "csharp"),
                ["javascript"] = projects.Count(p => p.Technology == "javascript"),
                ["python"] = projects.Count(p => p.Technology == "python")
            };
        }
    }

    public static void ValidateTitle(string title)
    {
        if (title is null || title.Trim().Length is < 5 or > 100)
            throw new ArgumentException("Название должно содержать от 5 до 100 символов.");
    }

    public static void ValidateTechnology(string technology)
    {
        if (string.IsNullOrWhiteSpace(technology) || !Technologies.Contains(technology.Trim()))
            throw new ArgumentException("Технология должна быть csharp, javascript или python.");
    }

    public static void ValidateStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status) || !Statuses.Contains(status.Trim()))
            throw new ArgumentException("Статус должен быть active или archived.");
    }

    private static Project Clone(Project project) => new()
    {
        Id = project.Id,
        Title = project.Title,
        Technology = project.Technology,
        Status = project.Status
    };
}
