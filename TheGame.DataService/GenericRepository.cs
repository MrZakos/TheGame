using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace TheGame.DataService
{
    /// <summary>
    /// GenericRepository
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected TheGameDatabaseContext context;
        internal DbSet<T> dbSet;
        public ILogger<GenericRepository<T>> _logger;

        public GenericRepository(
            TheGameDatabaseContext context,
            ILogger<GenericRepository<T>> logger)
        {
            this.context = context;
            this.dbSet = context.Set<T>();
            _logger = logger;
        }

        public virtual async Task<T> GetById(int id)
        {
            return await dbSet.FindAsync(id);
        }

        public virtual async Task<bool> Add(T entity)
        {
            await dbSet.AddAsync(entity);
            return true;
        }

        public async virtual Task<bool> Delete(int id)
        {
            throw new NotImplementedException();
        }

        public async virtual Task<IEnumerable<T>> All()
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<T>> Find(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, string includeProperties = "")
        {
            IQueryable<T> query = dbSet;
            IEnumerable<T> result;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                result = await orderBy(query).ToListAsync().ConfigureAwait(false);
            }
            else
            {
                result = await query.ToListAsync().ConfigureAwait(false);
            }
            return result;
        }

        public void Update(T entity)
        {
            context.Entry(entity).State = EntityState.Modified;
        }
    }
}
