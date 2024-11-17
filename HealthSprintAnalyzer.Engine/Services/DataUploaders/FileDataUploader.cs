using System.Diagnostics;
using System.Transactions;
using HealthSprintAnalyzer.Contracts.Models;
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

	public async Task<DatasetView> UploadFileAsync(Stream sprintFile, Stream ticketFile, Stream ticketHistoryFile)
	{
		try
		{
			
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
									
			dataset.From = sprints.MinBy(x => x.SprintStartDate).SprintStartDate;
			dataset.To = sprints.MaxBy(x => x.SprintEndDate).SprintEndDate;
			dataset.Teams = tickets.Select(x => x.Area).Distinct().ToList();
			
			try
			{
				using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
				await sprintRepository.AddOrUpdateManyAsync(sprints);
				
				dataset.SprintsIds = sprints.Select(x => x.Id).ToList();
				dataset.ParsingTime = watch.Elapsed;
				await ticketRepository.AddOrUpdateManyAsync(tickets);				
				await datasetRepository.AddOrUpdateManyAsync(new List<Dataset> { dataset });
				await ticketHistoryRepository.AddOrUpdateManyAsync(ticketHistories);
				scope.Complete();

			}
			catch (Exception e)
			{
				throw new InvalidOperationException("Upload Data to DB failed", e);
			}
			
			
			return await GetDatasetView(dataset); 
		}
		catch (InvalidOperationException e)
		{
			throw new InvalidOperationException("Parsing failed, invalid data", e);
		}
		catch (Exception e)
		{
			throw new InvalidOperationException("Parsing failed", e);
		}
		
	}
	public async Task<List<DatasetView>> GetAll() 
	{ 
		return (await datasetRepository.GetAllAsync()).Select(x => GetDatasetView(x).Result).ToList();
	}
	
	public async Task<DatasetView> GetById(string id) 
	{ 
		return await GetDatasetView(await datasetRepository.GetByIdAsync(Guid.Parse(id)));
	}
	
	private async Task<DatasetView> GetDatasetView(Dataset dataset) 
	{
		var sprintsIds = dataset.SprintsIds;
		var sprints = (await sprintRepository.GetByFilterAsync(x => sprintsIds.Contains(x.Id))).ToList();
		
		return new DatasetView
		(
			dataset.Id,
			dataset.LoadTime,
			dataset.ParsingTime,
			dataset.From,
			dataset.To,
			dataset.Teams,
			sprints.Select(x => x.Id).ToList(),
			sprints.Select(x => x.SprintName).ToList()
		);
	}
}