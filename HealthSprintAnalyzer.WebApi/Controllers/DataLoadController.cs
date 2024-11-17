using HealthSprintAnalyzer.Contracts.Models;
using HealthSprintAnalyzer.Contracts.Services;
using Microsoft.AspNetCore.Mvc;

namespace HealthSprintAnalyzer.WebApi.Controllers
{
	[ApiController]
	[Route("api/[controller]/[action]")]
	public class DataLoadController : Controller
	{
		private readonly IFileUploadService dataLoadService;

		public DataLoadController(IFileUploadService dataLoadService)
		{
			this.dataLoadService = dataLoadService;
		}
		
		[HttpPost]
		public async Task<DatasetView> UploadFile(IFormFile sprintFile, IFormFile ticketFile, IFormFile ticketHistoryFile)
		{
			return await dataLoadService
			.UploadFileAsync(sprintFile.OpenReadStream(), ticketFile.OpenReadStream(), ticketHistoryFile.OpenReadStream());
		}
		
		[HttpGet]
		public async Task<List<DatasetView>> GetDatasets()
		{
			return await dataLoadService.GetAll();
		}
		
		[HttpGet]
		public async Task<DatasetView> GetById(string id)
		{
			return await dataLoadService.GetById(id);
		}
	}
}
