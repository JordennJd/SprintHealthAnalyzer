using System.Collections.Concurrent;
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
			answer.Add(AnalyzeSprint(sprint,
			request.WeightUniformity,
			request.WeightRemovedPoints,
			request.WeightLateDone,
			request.WeightAddedTasks,
			request.WeightVelocity,
			request.WeightUnfinishedTasks,
			request.WeightLargeTasks));
		});
		return answer;
	}
	

	public SprintAnalyze AnalyzeSprint(
		Sprint sprint, 
		double? weightUniformity, double? weightRemovedPoints, double? weightLateDone,
		double? weightAddedTasks, double? weightVelocity, double? weightUnfinishedTasks, double? weightLargeTasks)
	{
		var SprintAnalyze = new SprintAnalyze(sprint.SprintName, new List<Metrics>(), sprint.SprintStartDate, sprint.SprintEndDate);

		var past = new PastInfo();
	
		var metricsBag = new ConcurrentBag<Metrics>();

		int day = 0;

		Parallel.ForEach(
			Enumerable.Range(0, (sprint.SprintEndDate - sprint.SprintStartDate).Days), 
			new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, 
			i =>
			{
				DateTime currentDate = sprint.SprintStartDate.AddDays(i);
				int currentDay = Interlocked.Increment(ref day); // Increment day in a thread-safe manner
			
				var metric = GetMetrics(sprint, currentDate, past, currentDay, SprintAnalyze.Metrics, weightUniformity, weightRemovedPoints, weightLateDone, weightAddedTasks, weightVelocity, weightUnfinishedTasks, weightLargeTasks);
				metricsBag.Add(metric);
			});

		// Transfer the items from the ConcurrentBag back to the list
		SprintAnalyze.Metrics.AddRange(metricsBag);
		SprintAnalyze.Metrics = SprintAnalyze.Metrics.OrderBy(x => x.Day).ToList();
		return SprintAnalyze;
	}

	
	public IEnumerable<Ticket> GetSumOfCreatedTicketsOnDate(Sprint sprint, DateTime date)
	{
		return sprint.Tickets
		.Where(x => x.IsCreatedOnDate(date, sprint.SprintEndDate) && x.IsInSprintOnDate(sprint.SprintName, date, sprint.SprintEndDate));
	}
	
	public IEnumerable<Ticket> GetSumOfInWorkTicketsOnDate(Sprint sprint, DateTime date)
	{
		return sprint.Tickets
		.Where(x => x.IsInProgressOnDate(date, sprint.SprintEndDate) && x.IsInSprintOnDate(sprint.SprintName, date, sprint.SprintEndDate));
	}
	
	public IEnumerable<Ticket> GetSumOfDoneTicketsOnDate(Sprint sprint, DateTime date)
	{
		return sprint.Tickets
		.Where(x => x.IsDoneOnDate(date, sprint.SprintEndDate) && x.IsInSprintOnDate(sprint.SprintName, date, sprint.SprintEndDate)
		&& !x.IsRemovedOnDate(date, sprint.SprintEndDate));
	}
	
	public IEnumerable<Ticket>	 GetSumOfRemovedTicketsOnDate(Sprint sprint, DateTime date)
	{
		return sprint.Tickets
		.Where(x => x.IsRemovedOnDate(date, sprint.SprintEndDate) && x.IsInSprintOnDate(sprint.SprintName, date, sprint.SprintEndDate));
	}
	
	public IEnumerable<Ticket> GetLeavedFromSprintTicketsOnDate(Sprint sprint, DateTime date)
	{
		return sprint.Tickets
		.Where(x => x.IsInSprintOnDate(sprint.SprintName, date.AddDays(-1), sprint.SprintEndDate) && !x.IsInSprintOnDate(sprint.SprintName, date, sprint.SprintEndDate));
	}
	
	public IEnumerable<Ticket> GetAddedToSprintTicketsOnDate(Sprint sprint, DateTime date)
	{
		if(date == sprint.SprintStartDate) return GetSumOfCreatedTicketsOnDate(sprint, date).Union(GetSumOfRemovedTicketsOnDate(sprint, date)
		.Union(GetSumOfDoneTicketsOnDate(sprint, date)).Union(GetSumOfInWorkTicketsOnDate(sprint, date)));
		
		return sprint.Tickets
			.Where(x => !x.IsInSprintOnDate(sprint.SprintName, date.AddDays(-1), sprint.SprintEndDate) && x.IsInSprintOnDate(sprint.SprintName, date, sprint.SprintEndDate));
	}
	
	public async Task<List<Sprint>> GetData(SprintAnalyzeRequest request)
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
	
	public Metrics GetMetrics(Sprint sprint, DateTime date, PastInfo past, int day, List<Metrics> metrics,
		double? weightUniformity = 0.15,      // Вес равномерности
		double? weightRemovedPoints = 0.05,   // Вес штрафа за удаленные задачи
		double? weightLateDone = 0.1,         // Вес штрафа за поздние задачи
		double? weightAddedTasks = 0.1,       // Вес добавленных задач
		double? weightVelocity = 0.2,         // Вес стабильности скорости
		double? weightUnfinishedTasks = 0.2,  // Вес незавершенных задач
		double? weightLargeTasks = 0.2        // Вес завершения крупных задач
)
	{
		var sumOfCreatedPoints = GetSumOfCreatedTicketsOnDate(sprint, date);
		var sumOfInWorkPoints = GetSumOfInWorkTicketsOnDate(sprint, date);
		var sumOfDonePoints = GetSumOfDoneTicketsOnDate(sprint, date);
		var sumOfRemovedPoints = GetSumOfRemovedTicketsOnDate(sprint, date);
		var commonPoints = sumOfCreatedPoints.HoursSum() + sumOfInWorkPoints.HoursSum() + sumOfDonePoints.HoursSum() + sumOfRemovedPoints.HoursSum();
		
		var percentOfCreatedPoints = (double)sumOfCreatedPoints.HoursSum() / commonPoints * 100;
		var percentOfInWorkPoints = (double)sumOfInWorkPoints.HoursSum() / commonPoints * 100;
		var percentOfDonePoints = (double)sumOfDonePoints.HoursSum() / commonPoints * 100;
		var percentOfRemovedPoints = (double)sumOfRemovedPoints.HoursSum() / commonPoints * 100;
		
		var sumOfLeavedFromSprint = GetLeavedFromSprintTicketsOnDate(sprint, date).Count();
		var sumOfAddedToSprint = GetAddedToSprintTicketsOnDate(sprint, date).Count();
		var sumOfLeavedFromSprintPoints = GetLeavedFromSprintTicketsOnDate(sprint, date).HoursSum();
		var sumOfAddedToSprintPoints = GetAddedToSprintTicketsOnDate(sprint, date).HoursSum();
		
		var backlogchangedPercent = (double)(sumOfAddedToSprintPoints + sumOfLeavedFromSprintPoints) / commonPoints;
		
		if(day == 1)
		backlogchangedPercent += sumOfAddedToSprintPoints + sumOfLeavedFromSprintPoints;
				
		return new Metrics(day, sumOfCreatedPoints.HoursSum(), percentOfCreatedPoints, sumOfInWorkPoints.HoursSum(), percentOfInWorkPoints,
		sumOfDonePoints.HoursSum(), percentOfDonePoints, sumOfRemovedPoints.HoursSum(), percentOfRemovedPoints, backlogchangedPercent, 0, sumOfLeavedFromSprint,
		sumOfLeavedFromSprintPoints, sumOfAddedToSprint, sumOfAddedToSprintPoints, CalculateSprintHealth(sprint, date,
		 weightUniformity, weightRemovedPoints, weightLateDone, weightAddedTasks, weightVelocity, weightUnfinishedTasks, weightLargeTasks));
	}
	
	double CalculateSprintHealth(
		Sprint sprint,
		DateTime date,
		double? weightUniformity = 0.15,      // Вес равномерности
		double? weightRemovedPoints = 0.05,   // Вес штрафа за удаленные задачи
		double? weightLateDone = 0.1,         // Вес штрафа за поздние задачи
		double? weightAddedTasks = 0.1,       // Вес добавленных задач
		double? weightVelocity = 0.2,         // Вес стабильности скорости
		double? weightUnfinishedTasks = 0.2,  // Вес незавершенных задач
		double? weightLargeTasks = 0.2        // Вес завершения крупных задач
	)
	{
		var createdPoints = GetSumOfCreatedTicketsOnDate(sprint, date).HoursSum();
		var inWorkPoints = GetSumOfInWorkTicketsOnDate(sprint, date).HoursSum();
		var donePoints = GetSumOfDoneTicketsOnDate(sprint, date).HoursSum();
		var removedPoints = GetSumOfRemovedTicketsOnDate(sprint, date).HoursSum();
		var commonPoints = createdPoints + inWorkPoints + donePoints + removedPoints;

		if (commonPoints == 0) return 100;

		double sprintProgress = (date - sprint.SprintStartDate).TotalDays / (sprint.SprintEndDate - sprint.SprintStartDate).TotalDays;
		sprintProgress = Math.Min(sprintProgress, 1);

		double smoothTransitionCoefficient = (1 - Math.Cos(Math.PI * sprintProgress)) / 2; // Косинусное значение от 0 до 1

		double expectedDoneTasksPercentage = sprintProgress * 100; 
	
		double actualDoneTasksPercentage = (double)donePoints / commonPoints * 100;

		double overdoneTaskPenalty = 0;
		if (actualDoneTasksPercentage > expectedDoneTasksPercentage + 20)
		{
			overdoneTaskPenalty = (actualDoneTasksPercentage - (expectedDoneTasksPercentage + 20)) * 0.2; 
		}

		double uniformityCoefficient = 1 - smoothTransitionCoefficient * (1 - CalculateUniformityCoefficient(sprint, date));
		double lateDonePenalty = smoothTransitionCoefficient * CalculateLateDonePercentage(sprint, date);
		double velocityConsistency = smoothTransitionCoefficient * (1 - CalculateVelocityConsistency(sprint, date));
		double unfinishedTasksPercent = smoothTransitionCoefficient * CalculateUnfinishedTasksPercentage(sprint, date);
		double addedTasksPercent = smoothTransitionCoefficient * CalculateAddedTasksPercentage(sprint, date);
		double largeTasksCompletion = smoothTransitionCoefficient * (1 - CalculateLargeTasksCompletion(sprint, date));

		double sprintHealth = 100 - (
			(weightUniformity ?? 0.15) * uniformityCoefficient +
			(weightRemovedPoints ?? 0.05) * removedPoints +
			(weightLateDone ?? 0.1) * lateDonePenalty +
			(weightAddedTasks ?? 0.1) * addedTasksPercent +
			(weightVelocity ?? 0.2) * velocityConsistency +
			(weightUnfinishedTasks ?? 0.2) * unfinishedTasksPercent +
			(weightLargeTasks ?? 0.2) * largeTasksCompletion +
			overdoneTaskPenalty // Штраф за слишком быстрые завершения задач
		);


		return Math.Clamp(sprintHealth, 0, 100);
	}



	public double CalculateUniformityCoefficient(Sprint sprint, DateTime date)
	{
		var progressByDays = GetDailyProgress(sprint, date); // Возвращает список количества завершённых задач по дням
		if (progressByDays.Count <= 1) return 1; // Если только один день, равномерность максимальная

		double average = progressByDays.Average();
		double standardDeviation = Math.Sqrt(progressByDays.Select(d => Math.Pow(d - average, 2)).Sum() / progressByDays.Count);

		return Math.Max(0, 1 - (standardDeviation / average)); // Чем ближе к 1, тем равномернее прогресс
	}

	public double CalculateLateDonePercentage(Sprint sprint, DateTime date)
	{
		var lastThreeDays = GetDaysBeforeSprintEnd(sprint, date, 3);
		var tasksDoneInLastDays = lastThreeDays.Sum(d => GetSumOfDoneTicketsOnDate(sprint, d).Count());
		var totalDoneTasks = GetSumOfDoneTicketsOnDate(sprint, date).Count();

		return totalDoneTasks == 0 ? 0 : (double)tasksDoneInLastDays / totalDoneTasks * 100;
	}

	public double CalculateVelocityConsistency(Sprint sprint, DateTime date)
	{
		var dailyVelocities = GetDailyVelocities(sprint, date); 
		if (dailyVelocities.Count <= 1) return 1;

		double average = dailyVelocities.Average();
		double standardDeviation = Math.Sqrt(dailyVelocities.Select(v => Math.Pow(v - average, 2)).Sum() / dailyVelocities.Count);

		return Math.Max(0, 1 - (standardDeviation / average));
	}

	public double CalculateUnfinishedTasksPercentage(Sprint sprint, DateTime date)
	{
		var inWorkTasks = GetSumOfInWorkTicketsOnDate(sprint, date).Count();
		var totalTasks = sprint.Tickets.Count();

		return totalTasks == 0 ? 0 : (double)inWorkTasks / totalTasks * 100;
	}

	double CalculateAddedTasksPercentage(Sprint sprint, DateTime date)
	{
		var addedTasks = GetAddedToSprintTicketsOnDate(sprint, date).Count();
		var totalTasks = sprint.Tickets.Count();

		return totalTasks == 0 ? 0 : (double)addedTasks / totalTasks * 100;
	}

	double CalculateLargeTasksCompletion(Sprint sprint, DateTime date)
	{
		var largeTasks = GetLargeTasks(sprint);
		if (!largeTasks.Any()) return 1;

		var completedLargeTasks = GetSumOfDoneTicketsOnDate(sprint, date).Intersect(largeTasks);

		return (double)completedLargeTasks.Count() / largeTasks.Count();
	}

	public List<int> GetDailyProgress(Sprint sprint, DateTime date)
	{
		var dailyProgress = new List<int>();
		for (DateTime currentDate = sprint.SprintStartDate; currentDate <= sprint.SprintEndDate; currentDate = currentDate.AddDays(1))
		{
			var doneToday = GetSumOfDoneTicketsOnDate(sprint, currentDate).Count();
			var doneYesterday = currentDate == sprint.SprintStartDate ? 0 : GetSumOfDoneTicketsOnDate(sprint, currentDate.AddDays(-1)).Count();
			dailyProgress.Add(doneToday - doneYesterday);
		}
		return dailyProgress;
	}

	public List<int> GetDailyVelocities(Sprint sprint, DateTime date) 
	{ 
		var storyPointsCounts = new List<int>();
		for (DateTime currentDate = sprint.SprintStartDate; currentDate < sprint.SprintEndDate; currentDate = currentDate.AddDays(1))
		{
			storyPointsCounts.Add(GetSumOfDoneTicketsOnDate(sprint, currentDate).HoursSum() - GetSumOfDoneTicketsOnDate(sprint, currentDate.AddDays(-1)).HoursSum()); 
		}
		
		return storyPointsCounts;
	}
	public List<DateTime> GetDaysBeforeSprintEnd(Sprint sprint, DateTime date, int days)
	{
		return Enumerable.Range(1, days).Select(i => sprint.SprintEndDate.AddDays(-i)).ToList();
	}
	public IEnumerable<Ticket> GetLargeTasks(Sprint sprint) 
	{ 
		
		return sprint.Tickets.Where(x => x.Estimation.HasValue && x.Estimation.Value.Hours > 8);
	}
	
	double CalculateTransformationCoefficient(Sprint sprint, DateTime date)
	{
		var createdPoints = GetSumOfCreatedTicketsOnDate(sprint, date).HoursSum();
		var inWorkPoints = GetSumOfInWorkTicketsOnDate(sprint, date).HoursSum();

		if (createdPoints == 0) return 1;

		return Math.Min(1, (double)inWorkPoints / createdPoints);
	}

	
	
}
