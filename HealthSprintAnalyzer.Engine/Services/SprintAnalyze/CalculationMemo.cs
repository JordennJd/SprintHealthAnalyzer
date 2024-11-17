using HealthSprintAnalyzer.Contracts.Models;
using SprintHealthAnalyzer.Entities;
using SprintHealthAnalyzer.Services;

public class PastInfo
{
	public int BacklogHoursBeforeTwoDays { get; set; }
	public int BacklogHoursAfterTwoDays { get; set; }
}

public class CalculationMemo
{
	public double SumOfCreatedPoints { get; }
	public double SumOfInWorkPoints { get; }
	public double SumOfDonePoints { get; }
	public double SumOfRemovedPoints { get; }
	public double CommonPoints { get; }

	public bool IsFirstHalf { get; }
	public double PercentOfRemovedPoints { get; }
	public double UniformityCoefficient { get; }
	public double LateDonePenalty { get; }
	public double VelocityConsistency { get; }
	public double UnfinishedTasksPercent { get; }
	public double AddedTasksPercent { get; }
	public double LargeTasksCompletion { get; }
	private readonly SprintAnalyzer sprintAnalyzer;

	public CalculationMemo(Sprint sprint, DateTime date)
	{
		sprintAnalyzer = new SprintAnalyzer(null,null,null);
		SumOfCreatedPoints = sprintAnalyzer.GetSumOfCreatedTicketsOnDate(sprint, date).HoursSum();
		SumOfInWorkPoints = sprintAnalyzer.GetSumOfInWorkTicketsOnDate(sprint, date).HoursSum();
		SumOfDonePoints = sprintAnalyzer.GetSumOfDoneTicketsOnDate(sprint, date).HoursSum();
		SumOfRemovedPoints = sprintAnalyzer.GetSumOfRemovedTicketsOnDate(sprint, date).HoursSum();

		CommonPoints = SumOfCreatedPoints + SumOfInWorkPoints + SumOfDonePoints + SumOfRemovedPoints;

		IsFirstHalf = (date - sprint.SprintStartDate).TotalDays < (sprint.SprintEndDate - sprint.SprintStartDate).TotalDays / 2;

		PercentOfRemovedPoints = CommonPoints == 0 ? 0 : (SumOfRemovedPoints / CommonPoints) * 100;
		UniformityCoefficient = CommonPoints == 0 ? 1 : CalculateUniformityCoefficient(sprint, date);
		LateDonePenalty = CalculateLateDonePercentage(sprint, date);
		VelocityConsistency = CalculateVelocityConsistency(sprint, date);
		UnfinishedTasksPercent = CalculateUnfinishedTasksPercentage(sprint, date);
		AddedTasksPercent = CalculateAddedTasksPercentage(sprint, date);
		LargeTasksCompletion = CalculateLargeTasksCompletion(sprint, date);
	}

	private double CalculateUniformityCoefficient(Sprint sprint, DateTime date)
	{
		var progressByDays = sprintAnalyzer.GetDailyProgress(sprint, date);
		if (progressByDays.Count <= 1) return 1;

		double average = progressByDays.Average();
		double standardDeviation = Math.Sqrt(progressByDays.Select(d => Math.Pow(d - average, 2)).Sum() / progressByDays.Count);

		return Math.Max(0, 1 - (standardDeviation / average));
	}

	private double CalculateLateDonePercentage(Sprint sprint, DateTime date)
	{
		var lastThreeDays = sprintAnalyzer.GetDaysBeforeSprintEnd(sprint, date, 3);
		var tasksDoneInLastDays = lastThreeDays.Sum(d => sprintAnalyzer.GetSumOfDoneTicketsOnDate(sprint, d).Count());
		var totalDoneTasks = sprintAnalyzer.GetSumOfDoneTicketsOnDate(sprint, date).Count();

		return totalDoneTasks == 0 ? 0 : (double)tasksDoneInLastDays / totalDoneTasks * 100;
	}

	private double CalculateVelocityConsistency(Sprint sprint, DateTime date)
	{
		var dailyVelocities = sprintAnalyzer.GetDailyVelocities(sprint, date);
		if (dailyVelocities.Count <= 1) return 1;

		double average = dailyVelocities.Average();
		double standardDeviation = Math.Sqrt(dailyVelocities.Select(v => Math.Pow(v - average, 2)).Sum() / dailyVelocities.Count);

		return Math.Max(0, 1 - (standardDeviation / average));
	}

	private double CalculateUnfinishedTasksPercentage(Sprint sprint, DateTime date)
	{
		var inWorkTasks = sprintAnalyzer.GetSumOfInWorkTicketsOnDate(sprint, date).Count();
		var totalTasks = sprint.Tickets.Count();

		return totalTasks == 0 ? 0 : (double)inWorkTasks / totalTasks * 100;
	}

	private double CalculateAddedTasksPercentage(Sprint sprint, DateTime date)
	{
		var addedTasks = sprintAnalyzer.GetAddedToSprintTicketsOnDate(sprint, date).Count();
		var totalTasks = sprint.Tickets.Count();

		return totalTasks == 0 ? 0 : (double)addedTasks / totalTasks * 100;
	}

	private double CalculateLargeTasksCompletion(Sprint sprint, DateTime date)
	{
		var largeTasks = sprintAnalyzer.GetLargeTasks(sprint);
		if (!largeTasks.Any()) return 1;

		var completedLargeTasks = sprintAnalyzer.GetSumOfDoneTicketsOnDate(sprint, date).Intersect(largeTasks);

		return (double)completedLargeTasks.Count() / largeTasks.Count();
	}
}
