using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SprintHealthAnalyzer.Entities;

namespace HealthSprintAnalyzer.Storage.Repositories
{
	public interface IRepository<T> where T : class
	{
		Task<T> GetByIdAsync(Guid id);
		Task<IEnumerable<T>> GetAllAsync();
		Task<IEnumerable<T>> GetByFilterAsync(Expression<Func<T, bool>> filter);
		Task AddAsync(T entity);
		Task AddOrUpdateManyAsync(List<T> entity);
		Task UpdateAsync(T entity);
		Task DeleteAsync(Guid id);
	}

	public interface IDatasetRepository : IRepository<Dataset> { }
	public interface ISprintRepository : IRepository<Sprint> { }
	public interface ITicketRepository : IRepository<Ticket> { }
	public interface ITicketHistoryRepository : IRepository<TicketHistory> { }
}
