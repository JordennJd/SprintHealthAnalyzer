
using static HealthSprintAnalyzer.Engine.Services.SprintAnalyze.Constants.Constants;

using SprintHealthAnalyzer.Entities;

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
		var history = ticket.History;
		
		if (!history.Any())
		{
			return ticket.Status;
		}
		
		return history
		.Where(x => x.GetStatusChange() != null && x.HistoryDate <= date && x.HistoryDate <= deadlineDate).Select(x => x.GetStatusChange())
		.MaxBy(x => x.ChangeDate)?.StatusTo ?? "<empty>";
	}
	
	public static string GetLastResoluitionOnDate(this Ticket ticket, DateTime date, DateTime deadlineDate)
	{
		var history = ticket.History;
		
		if (!history.Any())
		{
			return ticket.Resolution;
		}
		
		return history
		.Where(x => x.GetResolutionChange() != null && x.HistoryDate <= date && x.HistoryDate <= deadlineDate).Select(x => x.GetStatusChange())
		.MaxBy(x => x.ChangeDate)?.StatusTo ?? "<empty>";
	}
	
	public static bool IsCreatedOnDate(this Ticket ticket, DateTime date, DateTime deadlineDate)
	{		
		return CreatedStatuses.Contains(ticket.GetLastStatusOnDate(date, deadlineDate));
	}
	
	public static bool IsInProgressOnDate(this Ticket ticket, DateTime date, DateTime deadlineDate)
	{		
		return !ticket.IsDoneOnDate(date, deadlineDate) && !ticket.IsRemovedOnDate(date, deadlineDate);
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
	
	public static int DaysSum(this IEnumerable<Ticket> ticket)
	{		
		return ticket.Select(x => x.Estimation?.Days ?? 0)
		.Sum();
	}
}