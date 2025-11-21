using NodaTime;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudyCompanion.Shared.Models;

public enum Language
{
    English = 0,
    German = 1,
}

public class Settings
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public Language Language { get; set; } = Language.English;

    public required DateTimeZone TimeZone { get; set; }
}
