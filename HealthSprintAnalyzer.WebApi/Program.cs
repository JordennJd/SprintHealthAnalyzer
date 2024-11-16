using Microsoft.EntityFrameworkCore;
using HealthSprintAnalyzer.Storage.Services;
using Microsoft.EntityFrameworkCore.Design;
using HealthSprintAnalyzer.Storage.Exrtensions;
using HealthSprintAnalyzer.Contracts.Services;
using HealthSprintAnalyzer.Engine.Services.DataUploaders;
using HealthSprintAnalyzer.Engine.Contracts;
using HealthSprintAnalyzer.Engine.Parser;
using SprintHealthAnalyzer.Services;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Configure the HTTP request pipeline.

var MyConfig = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseNpgsql(MyConfig.GetValue<string>($"ConnectionStrings:{MachineNameHelper.GetMachineName()}")))
.ConfigureRepositories();

builder.Services.AddScoped<IFileUploadService, FileDataUploader>();
builder.Services.AddScoped<IEntityParser, CsvParser>();
builder.Services.AddScoped<ISprintAnalyzer, SprintAnalyzer>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();  // Добавляем Swagger генератор документации
builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.MapControllers();
app.UseRouting();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseHttpsRedirection();
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

app.Run();


public static class MachineNameHelper
{
	public static string GetMachineName()
	{
		var runningInContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
		if (runningInContainer)
			return "Docker";
		return Environment.MachineName;
	}
}

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
	public ApplicationDbContext CreateDbContext(string[] args)
	{
		var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

		var basePath = Directory.GetCurrentDirectory();
		var configuration = new ConfigurationBuilder()
			.SetBasePath(basePath)
			.AddJsonFile("appsettings.json")
			.Build();

		var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
		
		var connectionString = configuration.GetValue<string>($"ConnectionStrings:{MachineNameHelper.GetMachineName()}");
		optionsBuilder.UseNpgsql(connectionString,
		b =>
		{
			b.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
			b.MigrationsAssembly("HealthSprintAnalyzer.WebApi");
		});

		return new ApplicationDbContext(optionsBuilder.Options);
	}
}