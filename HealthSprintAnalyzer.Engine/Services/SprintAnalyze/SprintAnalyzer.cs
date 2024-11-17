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
		
		var answer = new List<SprintAnalyze>();
		
		if (!sprints.Any()) throw new ArgumentException("No sprints found");
		sprints.ForEach(sprint => 
		{
			answer.Add(AnalyzeSprint(sprint, request.To));
		});
		return answer;
	}
	
	private SprintAnalyze AnalyzeSprint(Sprint sprint, DateTime date)
	{
		var SprintAnalyze = new SprintAnalyze(sprint.SprintName, new List<Metrics>());
		
		var day = 1;
		
		for (DateTime currentDate = sprint.SprintStartDate; currentDate <= sprint.SprintEndDate; currentDate = currentDate.AddDays(1))
		{
			SprintAnalyze.Metrics.Add(GetMetrics(sprint, currentDate, day));
			day++;
		}
		
		return SprintAnalyze;
	}
	
	private int GetSumOfCreatedTicketsOnDate(Sprint sprint, DateTime date)
	{
		return sprint.Tickets
		.Where(x => x.IsCreatedOnDate(date, sprint.SprintEndDate) && x.IsInSprintOnDate(sprint.SprintName, date, sprint.SprintEndDate)).HoursSum();
	}
	
	private int GetSumOfInWorkTicketsOnDate(Sprint sprint, DateTime date)
	{
		return sprint.Tickets
		.Where(x => x.IsInProgressOnDate(date, sprint.SprintEndDate) && x.IsInSprintOnDate(sprint.SprintName, date, sprint.SprintEndDate)).HoursSum();
	}
	
	private int GetSumOfDoneTicketsOnDate(Sprint sprint, DateTime date)
	{
		return sprint.Tickets
		.Where(x => x.IsDoneOnDate(date, sprint.SprintEndDate) && x.IsInSprintOnDate(sprint.SprintName, date, sprint.SprintEndDate)).HoursSum();
	}
	
	private int GetSumOfRemovedTicketsOnDate(Sprint sprint, DateTime date)
	{
		return sprint.Tickets
		.Where(x => x.IsRemovedOnDate(date, sprint.SprintEndDate) && x.IsInSprintOnDate(sprint.SprintName, date, sprint.SprintEndDate)).HoursSum();
	}
	
	private IEnumerable<Ticket> GetLeavedFromSprintTicketsOnDate(Sprint sprint, DateTime date)
	{
		return sprint.Tickets
		.Where(x => x.IsInSprintOnDate(sprint.SprintName, date.AddDays(-1), sprint.SprintEndDate) && !x.IsInSprintOnDate(sprint.SprintName, date, sprint.SprintEndDate));
	}
	
	private IEnumerable<Ticket> GetAddedToSprintTicketsOnDate(Sprint sprint, DateTime date)
	{
		return sprint.Tickets
			.Where(x => !x.IsInSprintOnDate(sprint.SprintName, date.AddDays(-1), sprint.SprintEndDate) && x.IsInSprintOnDate(sprint.SprintName, date, sprint.SprintEndDate));
	}
	
	private async Task<List<Sprint>> GetData(SprintAnalyzeRequest request)
	{
		var sprintsIds = (await _datasetRepository.GetByFilterAsync(x => x.Id == request.DatasetId))
		.SelectMany(x => x.SprintsIds).Where(request.Sprints.Contains).ToList();
		
		var sprints =(await _sprintRepository.GetByFilterAsync(x => sprintsIds.Contains(x.Id)))
		.ToList();
		
		sprints.ForEach(async sprint =>
		{
			sprint.Tickets = (await _ticketRepository.GetByFilterAsync(x => sprint.EntityIds.Contains(x.EntityId) && request.Teams.Contains(x.Area ?? ""))).ToList();
		});
		
		return sprints;
	}
	
	private Metrics GetMetrics(Sprint sprint, DateTime date, int day)
	{
		var sumOfCreatedPoints = GetSumOfCreatedTicketsOnDate(sprint, date);
		var sumOfInWorkPoints = GetSumOfInWorkTicketsOnDate(sprint, date);
		var sumOfDonePoints = GetSumOfDoneTicketsOnDate(sprint, date);
		var sumOfRemovedPoints = GetSumOfRemovedTicketsOnDate(sprint, date);
		var commonPoints = sumOfCreatedPoints + sumOfInWorkPoints + sumOfDonePoints + sumOfRemovedPoints;
		
		var percentOfCreatedPoints = (double)sumOfCreatedPoints / commonPoints * 100;
		var percentOfInWorkPoints = (double)sumOfInWorkPoints / commonPoints * 100;
		var percentOfDonePoints = (double)sumOfDonePoints / commonPoints * 100;
		var percentOfRemovedPoints = (double)sumOfRemovedPoints / commonPoints * 100;
		
		var sumOfLeavedFromSprint = GetLeavedFromSprintTicketsOnDate(sprint, date).Count();
		var sumOfAddedToSprint = GetAddedToSprintTicketsOnDate(sprint, date).Count();
		var sumOfLeavedFromSprintPoints = GetLeavedFromSprintTicketsOnDate(sprint, date).HoursSum();
		var sumOfAddedToSprintPoints = GetAddedToSprintTicketsOnDate(sprint, date).HoursSum();

		
		return new Metrics(day, sumOfCreatedPoints, percentOfCreatedPoints, sumOfInWorkPoints, percentOfInWorkPoints,
		sumOfDonePoints, percentOfDonePoints, sumOfRemovedPoints, percentOfRemovedPoints, 0, 0, sumOfLeavedFromSprint,
		sumOfLeavedFromSprintPoints, sumOfAddedToSprint, sumOfAddedToSprintPoints, Random.Shared.Next());
	}
}
