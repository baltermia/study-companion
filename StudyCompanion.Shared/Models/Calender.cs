using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudyCompanion.Shared.Models;

public class Calender
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public required string Link { get; set; }
    public required DateTime LastRefresh { get; set; }
    public required string Data  { get; set; }
}