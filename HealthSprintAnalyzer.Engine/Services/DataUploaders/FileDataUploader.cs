using System.Diagnostics;
using System.Transactions;
using HealthSprintAnalyzer.Contracts.Services;
using HealthSprintAnalyzer.Engine.Contracts;
using HealthSprintAnalyzer.Storage.Repositories;
using SprintHealthAnalyzer.Entities;

namespace HealthSprintAnalyzer.Engine.Services.DataUploaders;

public class FileDataUploader : IFileUploadService
{
	private readonly IEntityParser parser;
	private readonly ISprintRepository sprintRepository;
	private readonly ITicketRepository ticketRepository;
	private readonly IDatasetRepository datasetRepository;

	private readonly ITicketHistoryRepository ticketHistoryRepository;

	public FileDataUploader(IEntityParser parser,
	 ITicketHistoryRepository ticketHistoryRepository,
	  ITicketRepository ticketRepository,
	   ISprintRepository sprintRepository,
	   IDatasetRepository datasetRepository)
	{
		this.parser = parser;
		this.ticketHistoryRepository = ticketHistoryRepository;
		this.ticketRepository = ticketRepository;
		this.sprintRepository = sprintRepository;
		this.datasetRepository = datasetRepository;
	}

	public async Task UploadFileAsync(Stream sprintFile, Stream ticketFile, Stream ticketHistoryFile)
	{
		try
		{
			using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
			
			var watch = Stopwatch.StartNew();
			
			var dataset = new Dataset();
			dataset.LoadTime = DateTime.UtcNow;
			
			var sprintTask = parser.ParseSprintFileAsync(sprintFile);
			var ticketsTask = parser.ParseTicketFileAsync(ticketFile);
			var ticketHistoriesTask = parser.ParseTicketHistoryFileAsync(ticketHistoryFile);

			dataset.Id = Guid.NewGuid().ToString();
			
			var sprints = await sprintTask;
			var tickets = (await ticketsTask).DistinctBy(x => x.EntityId).ToList();
			var ticketHistories = (await ticketHistoriesTask).Where(x => tickets.Select(y => y.EntityId).Contains(x.TicketId)).ToList();
						
			dataset.ParsingTime = watch.Elapsed;
			dataset.From = sprints.MinBy(x => x.SprintStartDate).SprintStartDate;
			dataset.To = sprints.MaxBy(x => x.SprintEndDate).SprintEndDate;
			dataset.Teams = tickets.DistinctBy(x => x.TicketNumber).Select(x => x.TicketNumber.Split('-')[0]).ToList();
			
			await sprintRepository.AddOrUpdateManyAsync(sprints);
			
			dataset.SprintsIds = sprints.Select(x => x.Id).ToList();
			
			await datasetRepository.AddOrUpdateManyAsync(new List<Dataset> { dataset });
			await ticketRepository.AddOrUpdateManyAsync(tickets);
			await ticketHistoryRepository.AddOrUpdateManyAsync(ticketHistories);
			
			scope.Complete();
		}
		catch (Exception e)
		{
			throw new InvalidOperationException("Parsing failed", e);
		}
	}
}