
using static HealthSprintAnalyzer.Engine.Services.SprintAnalyze.Constants.Constants;

using SprintHealthAnalyzer.Entities;
using Microsoft.Identity.Client;

public static class TicketHelper
{
	public static string GetLastStatus(this Ticket ticket)
	{
		var history = ticket.History;
		
		return history
		.Where(x => x.GetStatusChange() != null).Select(x => x.GetStatusChange())
		.MaxBy(x => x.ChangeDate)?.StatusTo ?? "<empty>";
	}
	
	public static string GetLastStatusOnDate(this Ticket ticket, DateTime date, DateTime deadlineDate)
	{
		var history = ticket.History.OrderBy(x => x.HistoryDate);
		
		if (history.Any(x => x.IsChangeTypeCreated()) && !history.Any(x => x.IsStatusChange() && x.HistoryDate.HasValue && x.HistoryDate.Value.Date  <= date.Date && x.HistoryDate.Value.Date <= deadlineDate.Date))
		{
			return "Создано";
		}
		
		var status = history
		.Where(x => x.GetStatusChange() != null && x.HistoryDate.HasValue && x.HistoryDate.Value.Date <= date.Date && x.HistoryDate.Value.Date <= deadlineDate.Date).Select(x => x.GetStatusChange())
		.MaxBy(x => x.ChangeDate)?.StatusTo ?? "<empty>";
		
		return status;
	}
	
	public static string GetLastSprintOnDate(this Ticket ticket, DateTime date, DateTime deadlineDate)
	{
		var history = ticket.History.OrderBy(x => x.HistoryDate);
		
		var sprint = history
		.Where(x => x.GetSprintChange() != null && x.HistoryDate.Value.Date <= date.Date && x.HistoryDate.Value.Date <= deadlineDate.Date).Select(x => x.GetSprintChange())
		.MaxBy(x => x.ChangeDate)?.StatusTo ?? "<empty>";
		
		return sprint;
	}
	
	public static string GetLastResoluitionOnDate(this Ticket ticket, DateTime date, DateTime deadlineDate)
	{
		var history = ticket.History;
		
		if (!history.Any(x => x.IsResolutionChange() && x.HistoryDate.Value.Date <= date.Date && x.HistoryDate.Value.Date <= deadlineDate.Date))
		{
			return string.IsNullOrEmpty(ticket.Resolution) ? "<empty>" : ticket.Resolution;
		}
				
		return history
		.Where(x => x.GetResolutionChange() != null && x.HistoryDate.HasValue && x.HistoryDate.Value.Date <= date.Date && x.HistoryDate.Value.Date <= deadlineDate.Date).Select(x => x.GetResolutionChange())
		.MaxBy(x => x?.ChangeDate ?? default)?.StatusTo ?? "<empty>";
	}
	
	public static bool IsCreatedOnDate(this Ticket ticket, DateTime date, DateTime deadlineDate)
	{		
		return CreatedStatuses.Contains(ticket.GetLastStatusOnDate(date, deadlineDate));
	}
	
	public static bool IsInProgressOnDate(this Ticket ticket, DateTime date, DateTime deadlineDate)
	{		
		return !ticket.IsDoneOnDate(date, deadlineDate) && !ticket.IsRemovedOnDate(date, deadlineDate) && !ticket.IsCreatedOnDate(date, deadlineDate);
	}
	
	public static bool IsDoneOnDate(this Ticket ticket, DateTime date, DateTime deadlineDate)
	{		
		return DoneStatuses.Contains(ticket.GetLastStatusOnDate(date, deadlineDate)) 
		&& !ticket.IsRemovedOnDate(date, deadlineDate);
	}
	
	public static bool IsRemovedOnDate(this Ticket ticket, DateTime date, DateTime deadlineDate)
	{
		if(ticket.IsTask())
		{
			return DoneStatuses.Contains(ticket.GetLastStatusOnDate(date, deadlineDate))
			 && RemovedResolutionForTask.Contains(ticket.GetLastResoluitionOnDate(date, deadlineDate));
		}
		else if (ticket.IsBug())
		{
			return RemovedResolutionForBug.Contains(ticket.GetLastResoluitionOnDate(date, deadlineDate));
		}
		
		return false;
	}
	
	public static DateTime FindDateWhenTicketAddedInSprint(this Ticket ticket, Sprint sprint)
	{
		var history = ticket.History;
		
		return history.Where(x => x.HistoryDate.HasValue && x.HistoryDate.Value.Date <= sprint.SprintStartDate.Date)
		.Where(x => x.GetSprintChange() != null && x.GetSprintChange().StatusTo == sprint.SprintName).Select(x => x.HistoryDate)
		.MaxBy(x => x.Value) ?? DateTime.MaxValue;
	}
	
	public static bool IsSpintChangedFromDate(this Ticket ticket, DateTime date)
	{
		var history = ticket.History;

		return history.Where(x => x.HistoryDate.HasValue && x.HistoryDate.Value.Date > date.Date).Any(x => x.GetSprintChange() != null);
	}

	
	public static int HoursSum(this IEnumerable<Ticket> ticket)
	{		
		return ticket.Select(x => x.Estimation?.Hours ?? 0)
		.Sum();
	}
	
	public static bool IsInSprintOnDate(this Ticket ticket, string sprintName, DateTime Date, DateTime deadlineDate)
	{
		var lastSprint = ticket.GetLastSprintOnDate(Date, deadlineDate);
		
		return lastSprint == sprintName;
	}	
}