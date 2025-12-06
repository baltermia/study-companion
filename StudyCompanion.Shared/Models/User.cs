using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudyCompanion.Shared.Models;

public enum Role
{
    User = 0,
    Mod = 1,
    Admin = 2,
}

public class User
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public required TelegramUser TelegramUser { get; set; }

    public required Settings Settings { get; set; }

    public List<Homework> Homework { get; set; } = [];

    public Role Role { get; set; } = Role.User;

}

