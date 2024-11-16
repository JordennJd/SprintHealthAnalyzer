using System;
using System.Collections.Generic;

namespace HealthSprintAnalyzer.Contracts.Models;

public record SprintAnalyzeRequest(
	string DatasetId,
	List<long> Sprints,
	List<string> Teams,
	DateTime To
);
