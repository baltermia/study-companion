using StudyCompanion.Shared.Models;

namespace StudyCompanion.Shared.Contracts;

public interface IAiService
{
    public Task<string> GetUserSummary(User user);
}