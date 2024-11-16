using CsvHelper;
using CsvHelper.Configuration;
using HealthSprintAnalyzer.Engine.Contracts;
using SprintHealthAnalyzer.Entities;
using System.Globalization;

namespace HealthSprintAnalyzer.Engine.Parser;

public class CsvParser : IEntityParser
{

	public async Task<List<Sprint>> ParseSprintFileAsync(Stream sprintFile)
	{
		var sprints = new List<Sprint>();

		using (var reader = new StreamReader(sprintFile))
		using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
		{
			Delimiter = ";",                 
			HasHeaderRecord = true,       
			Quote = '"',                     
		}))
		{
			await reader.ReadLineAsync();

			var records = csv.GetRecords<dynamic>().ToList();

			foreach (var record in records)
			{
				var sprint = new Sprint
				{
					SprintName = record.sprint_name,
					SprintStatus = record.sprint_status,
					SprintStartDate = DateTime.Parse(record.sprint_start_date),
					SprintEndDate = DateTime.Parse(record.sprint_end_date),
					EntityIds = ParseEntityIds(record.entity_ids)
				};

				sprints.Add(sprint);
			}
		}

		return sprints;
	}
	
	public async Task<List<TicketHistory>> ParseTicketHistoryFileAsync(Stream fileStream)
	{
		var entityHistories = new List<TicketHistory>();

		using (var reader = new StreamReader(fileStream))
		{
			
			var lines = reader.ReadToEnd().Replace("Столбец1", "").Split('\n')
			.Select(line => System.Text.RegularExpressions.Regex.Replace(line, ",+\r$", ""))
			.Skip(1)
			.ToList();
			
			using (var csv = new CsvReader(new StringReader(string.Join(Environment.NewLine, lines)), new CsvConfiguration(CultureInfo.InvariantCulture)
			{
				Delimiter = ";",                 
				HasHeaderRecord = true,          
				Quote = '"',       
				AllowComments = true,   // Разрешить комментарии, если они есть в файле   
			}))
			{

				

				var records = csv.GetRecords<dynamic>().ToList();

				Parallel.ForEach(records, new ParallelOptions(){MaxDegreeOfParallelism = 4}, record => 
				{
					try
					{
						var entityHistory = new TicketHistory
						{
							TicketId = Convert.ToInt64(record.entity_id),
							HistoryPropertyName = string.IsNullOrEmpty(record.history_property_name) ? null : record.history_property_name,
							HistoryDate = string.IsNullOrEmpty(record.history_date) ? null : DateTime.TryParse(record.history_date, out DateTime historyDate) ? historyDate : DateTime.MinValue,
							HistoryVersion = string.IsNullOrEmpty(record.history_version) ? null : Convert.ToInt32(record.history_version),
							HistoryChangeType = string.IsNullOrEmpty(record.history_change_type) ? null : record.history_change_type,
							HistoryChange =  string.IsNullOrEmpty(record.history_change) ? null : record.history_change
						};

						entityHistories.Add(entityHistory);
						
					}catch
					{
					}
				});
			}

			return entityHistories;
			
		}
	}
	
	public async Task<List<Ticket>> ParseTicketFileAsync(Stream fileStream)
	{
		var tickets = new List<Ticket>();
		using (var reader = new StreamReader(fileStream))
		{
			var lines = reader.ReadToEnd().Replace("\"", "").Split('\n')
			.Select(line => System.Text.RegularExpressions.Regex.Replace(line, ",+\r$", ""))
			.Skip(1)
			.ToList();
			
			using (var csv = new CsvReader(new StringReader(string.Join(Environment.NewLine, lines)), new CsvConfiguration(CultureInfo.InvariantCulture)
			{
				Delimiter = ";",                 // Устанавливаем разделитель
				HasHeaderRecord = true,          // Первая строка — заголовки
				Quote = '"',                     // Строки могут быть в кавычках
				TrimOptions = TrimOptions.Trim,  // Удалять пробелы по краям полей
				BadDataFound = null,             // Игнорировать некорректные данные
				AllowComments = true,   // Разрешить комментарии, если они есть в файле
			}))
			{
				var records = csv.GetRecords<dynamic>().ToList();

				foreach (var record in records)
				{
					try
					{
						var ticket = new Ticket
						{
							EntityId = Convert.ToInt64(record.entity_id),
							Area = record.area,
							Type = record.type,
							Status = record.status,
							State = record.state,
							Priority = record.priority,
							TicketNumber = record.ticket_number,
							Name = record.name,
							CreateDate = DateTime.Parse(record.create_date),
							CreatedBy = record.created_by,
							UpdateDate = string.IsNullOrEmpty(record.update_date) ? (DateTime?)null : DateTime.TryParse(record.update_date, out DateTime result) ? result : null,
							UpdatedBy = record.updated_by,
							ParentTicketId = string.IsNullOrEmpty(record.parent_ticket_id) ? null : Convert.ToInt64(record.parent_ticket_id),
							Assignee = record.assignee,
							Owner = record.owner,
							DueDate = string.IsNullOrEmpty(record.due_date) ? (DateTime?) null : DateTime.TryParse(record.due_date, out DateTime result1) ? result1 : null,
							Rank = string.IsNullOrEmpty(record.rank) ? "" : record.rank,
							Estimation = string.IsNullOrEmpty(record.estimation) ? null : TimeSpan.FromSeconds(Convert.ToInt32(record.estimation)),
							Spent = string.IsNullOrEmpty(record.spent) ? null : TimeSpan.FromSeconds(Convert.ToInt32(record.spent)),
							Workgroup = record.workgroup,
							Resolution = record.resolution
						};

						tickets.Add(ticket);	
					}
					catch (Exception ex)
					{
						
					}
				}
			}
		}

		return tickets;
	}

	private List<long> ParseEntityIds(string entityIdsString)
	{
		entityIdsString = entityIdsString.Trim(new char[] { '{', '}' });
		var entityIds = entityIdsString.Split(',')
									   .Select(id => long.Parse(id.Trim()))
									   .ToList();
		return entityIds;
	}
}

