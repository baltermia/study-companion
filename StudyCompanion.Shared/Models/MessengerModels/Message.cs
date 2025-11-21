namespace StudyCompanion.Shared.Models;

public class Message : Identity
{
    public required Chat Chat { get; init; }
    public required string? Text { get; init; }
}

