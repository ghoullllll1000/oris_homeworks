namespace Server.Models;

public sealed class Project
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Technology { get; set; } = string.Empty;
    public string Status { get; set; } = "active";
}
