using System;
using System.Collections.Generic;

namespace HealthSprintAnalyzer.Contracts.Models;

public record SprintAnalyze(
	string SprintName,
	List<Metrics> Metrics,
	DateTime From,
	DateTime To
);

public record Metrics(
	int Day,
	int CreatedTicketPoints, //Созданно 
	double PercentOfCreated,
	int InWorkTicketPoints, // В работе
	double PercentOfInWork,
	int DoneTicketPoints, // Готовые
	double PercentOfDone,
	int RemoveTicketPoints, //Снятые
	double PercentOfRemove,
	
	double BacklogchangedPercent,
	int BlockedTicketPoints, //Заблокированные тикеты другой задачей
	
	int ExcluededToday, //Кол во тикетов удаленных из спринта на день
	int ExcludedTicketPoints, //Кол во суммы тикетов удаленных из спринта на день
	int AddedToday, //Кол во суммы тикетов добавленных из спринта на день
	int AddedTicketPoints, //Кол во тикетов добавленных из спринта на день
	
	double SprintHealthPoints
); 