using SprintHealthAnalyzer.Entities;

var ticket = new Ticket
{
	EntityId = 1,
	Area = "Area",
	Type = "Задача",
	Resolution = "Резолюция",
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
		new TicketHistory
		{
			HistoryDate = DateTime.Now,
			HistoryPropertyName = "Резолюция",
			HistoryChangeType = "FIELD_CHANGED",
			HistoryChange = "<empty> -> Отклонено"
		},
		new TicketHistory
		{
			HistoryDate = DateTime.Now,
			HistoryPropertyName = "Статус",
			HistoryChangeType = "FIELD_CHANGED",
			HistoryChange = "created -> done"
		}
	}
	
};

Console.WriteLine(ticket.IsRemovedOnDate(DateTime.Now, DateTime.Now));