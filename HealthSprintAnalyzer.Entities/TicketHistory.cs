namespace SprintHealthAnalyzer.Entities;

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class TicketHistory
{
	private const string _separator = " -> ";
	private const string _status = "Статус";
	private const string _resolution = "Резолюция";

	public TicketHistory() { }

	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public long Id { get; set; }
	public long TicketId { get; set; }
	public string? HistoryPropertyName { get; set; }
	public DateTime? HistoryDate { get; set; }
	public int? HistoryVersion { get; set; }
	public string? HistoryChangeType { get; set; }
	public string? HistoryChange { get; set; }
	
	public Change GetStatusChange()
	{
		if(HistoryPropertyName != _status)
		{
			return null;
		}
		
		var splitted = HistoryChange?.Split(_separator) ?? ["<empty>", "<empty>"];
			
		return new Change(splitted[0], splitted[1], TicketId, HistoryDate ?? default);	
	}
	
	public Change GetResolutionChange()
	{
		if(HistoryPropertyName != _resolution)
		{
			return null;
		}
		
		var splitted = HistoryChange?.Split(_separator) ?? ["<empty>", "<empty>"];
			
		return new Change(splitted[0], splitted[1], TicketId, HistoryDate ?? default);	
	}
}

public record Change(string StatusFrom, string StatusTo, long TicketId, DateTime ChangeDate);