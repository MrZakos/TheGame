using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace TheGame.DataService
{
    /// <summary>
    /// Unit of Work (UoW) - abstraction over the idea of atomic operations
    /// </summary>
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly TheGameDatabaseContext _context;
        private readonly ILogger<UnitOfWork> _log;

        public IPlayerRepository Players { get; private set; }

        public UnitOfWork(TheGameDatabaseContext context, ILogger<UnitOfWork> log)
        {
            _context = context;
            _log = log;
            Players = new PlayerRepository(context, _log);
        }

        public async Task CompleteAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task EnsureCreated()
        {
            await _context.Database.EnsureCreatedAsync();
        }


        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
