using HealthSprintAnalyzer.Contracts.Models;
using HealthSprintAnalyzer.Contracts.Services;
using Microsoft.AspNetCore.Mvc;

namespace HealthSprintAnalyzer.WebApi.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class SprintAnalyzeController : ControllerBase
{
	private readonly ISprintAnalyzer sprintAnalyzeService;

	public SprintAnalyzeController(ISprintAnalyzer sprintAnalyzeService)
	{
		this.sprintAnalyzeService = sprintAnalyzeService;
	}

	[HttpPost]
	public async Task<IActionResult> GetSprintAnalyze(SprintAnalyzeRequest request ) 
	{
		return Ok(await sprintAnalyzeService.AnalyzeSprintAsync(request));
	}
}