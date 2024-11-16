namespace SprintHealthAnalyzer.Entities;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class Ticket
{
	[Key]
	public long EntityId { get; set; }
	public string? Area { get; set; }
	public string? Type { get; set; }
	public string? Status { get; set; }
	public string? State { get; set; }
	public string? Priority { get; set; }
	public string TicketNumber { get; set; }
	public string? Name { get; set; }
	public DateTime CreateDate { get; set; }
	public string? CreatedBy { get; set; }
	public DateTime? UpdateDate { get; set; }
	public string? UpdatedBy { get; set; }
	public long? ParentTicketId { get; set; }
	public string? Assignee { get; set; }
	public string? Owner { get; set; }
	public DateTime? DueDate { get; set; }
	public string? Rank { get; set; }
	public TimeSpan? Estimation { get; set; }
	public TimeSpan? Spent { get; set; }
	public string? Workgroup { get; set; }
	public string? Resolution { get; set; }
	public List<TicketHistory> History { get; set; } = new List<TicketHistory>();
	
	private const string _task = "Задача";
	private const string _bug = "Дефект";

	public bool IsTask() => Type == _task;
	public bool IsBug() => Type == _bug;
}
