using HealthSprintAnalyzer.Storage.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SprintHealthAnalyzer.Entities;

namespace HealthSprintAnalyzer.Storage.Services;

public class ApplicationDbContext : DbContext
{
	private static bool _isFirstInitialized = true;

	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) 
	{
		Database.EnsureCreated();
	}

	public DbSet<Dataset> Datasets { get; set; }
	public DbSet<Sprint> Sprints { get; set; }
	public DbSet<Ticket> Tickets { get; set; }
	public DbSet<TicketHistory> TicketHistories { get; set; }

	
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);
	}
}

