using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudyCompanion.Shared.Models;

public enum Role
{
    User = 0,
    Mod = 1,
    Admin = 2,
}

public class Player
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public required User User { get; set; }

    public Player? ReferredBy { get; set; }

    public required Settings Settings { get; set; }

    public Role Role { get; set; } = Role.User;

    public decimal PercentageWithdrawn { get; set; }
}

