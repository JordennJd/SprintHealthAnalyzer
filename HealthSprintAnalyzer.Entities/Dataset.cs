using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SprintHealthAnalyzer.Entities;

public class Dataset
{
	[Key]
	public string Id { get; set; }
	public DateTime LoadTime { get; set; }
	public TimeSpan ParsingTime { get; set; }
	public DateTime From { get; set; }
	public DateTime To { get; set; }
	public List<string> Teams { get; set; }
	
	public List<long> SprintsIds { get; set; } = new List<long>();
	public List<Sprint> Sprints { get; set; } = new List<Sprint>();
	
}

