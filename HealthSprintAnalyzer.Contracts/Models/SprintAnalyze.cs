using System;
using System.Collections.Generic;

namespace HealthSprintAnalyzer.Contracts.Models;

public record SprintAnalyze(
	double Hp,
	string SprintName,
	List<TaskMoving> TaskMoving
);

public record TaskMoving(
	int Day,
	double PercentOfOutDatedTasks,
	int ClosedTaskCount,
	int ClosedTaskPower,
	double PercentOfCanceledTasks,
	double PercentOfCreatedTasks,
	double PercentOfBacklogChanging
);