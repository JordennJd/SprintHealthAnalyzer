using HealthSprintAnalyzer.Contracts.Models;
using HealthSprintAnalyzer.Contracts.Services;
using HealthSprintAnalyzer.Storage.Repositories;
using SprintHealthAnalyzer.Entities;

namespace SprintHealthAnalyzer.Services;

public class SprintAnalyzer : ISprintAnalyzer
{
	private readonly IDatasetRepository _datasetRepository;
	private readonly ISprintRepository _sprintRepository;
	private readonly ITicketRepository _ticketRepository;

	public SprintAnalyzer(IDatasetRepository datasetRepository, ISprintRepository sprintRepository, ITicketRepository ticketRepository)
	{
		(_datasetRepository, _sprintRepository) = (datasetRepository, sprintRepository);
		_ticketRepository = ticketRepository;
	}

	public async Task<IList<SprintAnalyze>> AnalyzeSprintAsync(SprintAnalyzeRequest request)
	{
		var sprintsIds = (await _datasetRepository.GetByFilterAsync(x => x.Id == request.DatasetId))
		.SelectMany(x => x.SprintsIds).Where(x => request.Sprints.Contains(x)).ToList();
		
		var sprints =(await _sprintRepository.GetByFilterAsync(x => sprintsIds.Contains(x.Id)))
		.ToList();
		
		sprints.ForEach(async sprint =>
		{
			sprint.Tickets = (await _ticketRepository.GetByFilterAsync(x => sprint.EntityIds.Contains(x.EntityId))).ToList();
		});
		
		return null;
	}
}
