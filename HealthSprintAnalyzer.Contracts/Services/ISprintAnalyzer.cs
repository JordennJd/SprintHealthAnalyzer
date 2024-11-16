using HealthSprintAnalyzer.Contracts.Models;

namespace HealthSprintAnalyzer.Contracts.Services;

public interface ISprintAnalyzer
{
    Task<IList<SprintAnalyze>> AnalyzeSprintAsync(SprintAnalyzeRequest request);
}
