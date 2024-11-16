namespace SprintHealthAnalyzer.Entities;

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class TicketHistory
{
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public long Id { get; set; }
	public long TicketId { get; set; }
	public string? HistoryPropertyName { get; set; }
	public DateTime? HistoryDate { get; set; }
	public int? HistoryVersion { get; set; }
	public string? HistoryChangeType { get; set; }
	public string? HistoryChange { get; set; }
}