namespace HealthSprintAnalyzer.Contracts.Models;

public record DatasetListView(
	string Id,
	DateTime LoadTime,
	TimeSpan ParsingTime,
	DateTime From,
	DateTime To,
	List<string> Teams = null,
	List<long> SprintsIds = null,
	List<SprintView> Sprints = null
);
