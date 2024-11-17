using System;
using System.Collections.Generic;

namespace HealthSprintAnalyzer.Contracts.Models;

public record SprintAnalyze(
	string SprintName,
	List<Metrics> Metrics
);

public record Metrics(
	int Day,
	int CreatedTicketPoints,
	int InWorkTicketPoints,
	int DoneTicketPoints,
	int RemoveTicketPoints,
	double BacklogchangedPercent,
	int BlocketTicketPoints,
	int ExcludedTicketPoints,
	int AddedTicketPoints,
	int AddedToday,
	int RemovedToday
	
);