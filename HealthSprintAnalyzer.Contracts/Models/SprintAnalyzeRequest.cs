using System;
using System.Collections.Generic;

namespace HealthSprintAnalyzer.Contracts.Models;

public record SprintAnalyzeRequest(
	string DatasetId,
	List<long> Sprints,
	List<string> Teams,
	DateTime To,
	double? WeightUniformity = null,       // Вес равномерности
	double? WeightRemovedPoints = null,    // Вес штрафа за удаленные задачи
	double? WeightLateDone = null,         // Вес штрафа за поздние задачи
	double? WeightAddedTasks = null,       // Вес добавленных задач
	double? WeightVelocity = null,         // Вес стабильности скорости
	double? WeightUnfinishedTasks = null,  // Вес незавершенных задач
	double? WeightLargeTasks = null,       // Вес завершения крупных задач
	double? WeightTransformation = null   // Вес коэффициента трансформации (только для первой половины)
);
