using System;
using System.Collections.Generic;

namespace HealthSprintAnalyzer.Contracts.Models;

public record SprintAnalyze(
	string SprintName,
	List<Metrics> Metrics
);

public record Metrics(
	int Day,
	int CreatedTickets,
	int InWorkTickets,
	int DoneTickets,
	int RemoveTickets,
	double BacklogchangedPercent,
	int BlocketTickets,
	int ExcludedTickets,
	int AddedTickets
);