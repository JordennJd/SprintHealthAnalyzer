using HealthSprintAnalyzer.Storage.Services;
using Microsoft.EntityFrameworkCore;
using SprintHealthAnalyzer.Entities;
using System.Linq.Expressions;
using EFCore.BulkExtensions;

namespace HealthSprintAnalyzer.Storage.Repositories
{
	public class Repository<T> : IRepository<T> where T : class
	{
		private readonly ApplicationDbContext _context;
		private readonly DbSet<T> _dbSet;

		public Repository(ApplicationDbContext context)
		{
			_context = context;
			_dbSet = context.Set<T>();
		}

		public async Task<T> GetByIdAsync(Guid id)
		{
			return await _dbSet.FindAsync(id);
		}

		public async Task<IEnumerable<T>> GetAllAsync()
		{
			return await _dbSet.ToListAsync();
		}

		public async Task<IEnumerable<T>> GetByFilterAsync(Expression<Func<T, bool>> filter)
		{
			return await _dbSet.Where(filter).ToListAsync();
		}

		public async Task AddAsync(T entity)
		{
			await _dbSet.AddAsync(entity);
			await _context.SaveChangesAsync();
		}
		
		

		public async Task UpdateAsync(T entity)
		{
			_dbSet.Update(entity);
			await _context.SaveChangesAsync();
		}

		public async Task DeleteAsync(Guid id)
		{
			var entity = await _dbSet.FindAsync(id);
			if (entity != null)
			{
				_dbSet.Remove(entity);
				await _context.SaveChangesAsync();
			}
		}

		public async Task AddOrUpdateManyAsync(List<T> entity)
		{
			var bulkConfig = new BulkConfig { SetOutputIdentity = true }; // Обновляет идентификаторы
			await _context.BulkInsertOrUpdateAsync(entity, bulkConfig);
			await _context.SaveChangesAsync();
		}
	}

	public class DatasetRepository : Repository<Dataset>, IDatasetRepository
	{
		public DatasetRepository(ApplicationDbContext context) : base(context) { }
	}

	public class SprintRepository : Repository<Sprint>, ISprintRepository
	{
		public SprintRepository(ApplicationDbContext context) : base(context) { }
	}

	public class TicketRepository : Repository<Ticket>, ITicketRepository
	{
		public TicketRepository(ApplicationDbContext context) : base(context) { }
	}

	public class TicketHistoryRepository : Repository<TicketHistory>, ITicketHistoryRepository
	{
		public TicketHistoryRepository(ApplicationDbContext context) : base(context) { }
	}
}
