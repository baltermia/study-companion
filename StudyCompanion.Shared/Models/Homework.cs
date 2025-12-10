using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudyCompanion.Shared.Models;

public class Homework
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public User User { get; set; }

    public required string Note { get; set; }
    public required DateOnly Due { get; set; }
}
