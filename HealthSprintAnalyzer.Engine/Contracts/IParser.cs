using SprintHealthAnalyzer.Entities;

namespace HealthSprintAnalyzer.Engine.Contracts
{
	public interface IEntityParser
	{
		Task<List<Sprint>> ParseSprintFileAsync(Stream sprintFile);
		
		Task<List<Ticket>> ParseTicketFileAsync(Stream ticketFile);
		
		Task<List<TicketHistory>> ParseTicketHistoryFileAsync(Stream ticketHistoryFile);
	}
}
