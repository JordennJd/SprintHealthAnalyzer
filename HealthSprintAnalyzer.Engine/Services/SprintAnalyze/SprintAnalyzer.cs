using HealthSprintAnalyzer.Contracts.Models;
using HealthSprintAnalyzer.Contracts.Services;
using HealthSprintAnalyzer.Storage.Repositories;
using SprintHealthAnalyzer.Entities;
using static HealthSprintAnalyzer.Engine.Services.SprintAnalyze.Constants.Constants;
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
		var sprints = await GetData(request);
		
		return null;
	}
	
	private SprintAnalyze AnalyzeSprint(Sprint sprint, DateTime date)
	{
		var SprintAnalyze = new SprintAnalyze(sprint.SprintName,new List<Metrics>());
		
		var day = 1;
		
		for (DateTime currentDate = sprint.SprintStartDate; currentDate <= sprint.SprintEndDate; currentDate = currentDate.AddDays(1))
		{
			SprintAnalyze.Metrics.Add(new Metrics(
			
				day,
				GetSumOfCreatedTicketsOnDate(sprint, date),
				GetSumOfInWorkTicketsOnDate(sprint, date),
				GetSumOfDoneTicketsOnDate(sprint, date)
				//...
			));
			day++;
		}
		
		return SprintAnalyze;
	}
	
	private int GetSumOfCreatedTicketsOnDate(Sprint sprint, DateTime date)
	{
		return sprint.Tickets
		.Where(x => x.IsCreatedOnDate(date, sprint.SprintEndDate)).DaysSum();
	}
	
	private int GetSumOfInWorkTicketsOnDate(Sprint sprint, DateTime date)
	{
		return sprint.Tickets
		.Where(x => !x.IsInProgressOnDate(date, sprint.SprintEndDate)).DaysSum();
	}
	
	private int GetSumOfDoneTicketsOnDate(Sprint sprint, DateTime date)
	{
		return sprint.Tickets
		.Where(x => !x.IsDoneOnDate(date, sprint.SprintEndDate)).DaysSum();
	}
	
	private async Task<List<Sprint>> GetData(SprintAnalyzeRequest request)
	{
		var sprintsIds = (await _datasetRepository.GetByFilterAsync(x => x.Id == request.DatasetId))
		.SelectMany(x => x.SprintsIds).Where(x => request.Sprints.Contains(x)).ToList();
		
		var sprints =(await _sprintRepository.GetByFilterAsync(x => sprintsIds.Contains(x.Id)))
		.ToList();
		
		sprints.ForEach(async sprint =>
		{
			sprint.Tickets = (await _ticketRepository.GetByFilterAsync(x => sprint.EntityIds.Contains(x.EntityId))).ToList();
		});
		
		return sprints;
	}
}
