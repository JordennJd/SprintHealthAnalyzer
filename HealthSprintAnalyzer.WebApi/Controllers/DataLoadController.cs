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
		public async Task<IActionResult> UploadFile(IFormFile sprintFile, IFormFile ticketFile, IFormFile ticketHistoryFile)
		{
			await dataLoadService
			.UploadFileAsync(sprintFile.OpenReadStream(), ticketFile.OpenReadStream(), ticketHistoryFile.OpenReadStream());
			
			return Ok();
		}
	}
}
