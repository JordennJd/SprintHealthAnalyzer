using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthSprintAnalyzer.Contracts.Models;

public record SprintView(
	long Id,
	string SprintName,
	string SprintStatus,
	DateTime SprintStartDate,
	DateTime SprintEndDate
);
