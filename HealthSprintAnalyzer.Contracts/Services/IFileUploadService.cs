using HealthSprintAnalyzer.Contracts.Models;

namespace HealthSprintAnalyzer.Contracts.Services;

public interface IFileUploadService
{
	Task<DatasetView> UploadFileAsync(Stream sprintFile, Stream ticketFile, Stream ticketHistoryFile);
	
	Task<List<DatasetView>> GetAll();
	
	Task<DatasetView> GetById(string ID);
}