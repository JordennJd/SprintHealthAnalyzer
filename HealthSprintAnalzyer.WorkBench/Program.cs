using SprintHealthAnalyzer.Entities;

var ticket = new Ticket
{
	EntityId = 1,
	Area = "Area",
	Type = "Task",
	Resolution = "Resolved",
	Estimation = TimeSpan.FromDays(1),
	History = new List<TicketHistory>()
	{
		new TicketHistory
		{
			HistoryDate = DateTime.Now,
			HistoryPropertyName = "Задача",
			HistoryChangeType = "CREATED",
			HistoryChange = null
		},
		// new TicketHistory
		// {
		// 	HistoryDate = DateTime.Now,
		// 	HistoryPropertyName = "Резолюция",
		// 	HistoryChangeType = "FIELD_CHANGED",
		// 	HistoryChange = " -> <empty>"
		// },
		new TicketHistory
		{
			HistoryDate = DateTime.Now.AddDays(1),
			HistoryPropertyName = "Статус",
			HistoryChangeType = "FIELD_CHANGED",
			HistoryChange = "created -> inProgress"
		}
	}
	
};

Console.WriteLine(ticket.GetLastStatusOnDate(DateTime.Now, DateTime.Now));