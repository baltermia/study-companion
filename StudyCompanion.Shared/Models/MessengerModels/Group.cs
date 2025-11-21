namespace StudyCompanion.Shared.Models;

public class Group : Chat
{
    public required string? Title { get; init; }
    public int? MessageThreadId { get; set; }
}

