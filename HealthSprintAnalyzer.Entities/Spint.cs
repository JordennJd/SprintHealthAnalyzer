namespace SprintHealthAnalyzer.Entities;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Sprint
{
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public long Id { get; set; }
	public string SprintName { get; set; }
	public string SprintStatus { get; set; }
	public DateTime SprintStartDate { get; set; }
	public DateTime SprintEndDate { get; set; }
	public List<long> EntityIds { get; set; } = new List<long>();
	public List<Ticket> Tickets { get; set; }	
}
