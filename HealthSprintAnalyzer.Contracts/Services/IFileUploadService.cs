namespace HealthSprintAnalyzer.Contracts.Services;

public interface IFileUploadService
{
	Task UploadFileAsync(Stream sprintFile, Stream ticketFile, Stream ticketHistoryFile);
}