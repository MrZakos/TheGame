using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace TheGame.DataService
{
    /// <summary>
    /// Unit of Work (UoW) - abstraction over the idea of atomic operations
    /// - maintains lists of business objects in-memory which have been changed (inserted, updated, or deleted) during a transaction.
    /// - once the transaction is completed, all these updates are sent as one big unit of work to be persisted physically in a database in one go.
    /// </summary>
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly TheGameDatabaseContext _context;
        private readonly ILogger<UnitOfWork> _log;
        public IPlayerRepository Players { get; private set; }
        public IResourceRepository Resources { get; private set; }

        public UnitOfWork(
            TheGameDatabaseContext context,
            ILogger<UnitOfWork> log,
            IPlayerRepository playerRepository,
            IResourceRepository resourceRepository)
        {
            _context = context;
            _log = log;
            Players = playerRepository;
            Resources = resourceRepository;
        }

        public async Task CommitAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task EnsureCreated()
        {
            await _context.Database.EnsureCreatedAsync();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}