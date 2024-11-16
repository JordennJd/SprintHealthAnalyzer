using HealthSprintAnalyzer.Storage.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace HealthSprintAnalyzer.Storage.Exrtensions;

public static class RepositoryDependency
{
	public static void ConfigureRepositories(this IServiceCollection services)
	{
		services.AddScoped<IDatasetRepository, DatasetRepository>();
		services.AddScoped<ISprintRepository, SprintRepository>();
		services.AddScoped<ITicketRepository, TicketRepository>();
		services.AddScoped<ITicketHistoryRepository, TicketHistoryRepository>();
	}
}