using System;
using System.Threading.Tasks;

namespace TheGame.DataService
{
    public interface IUnitOfWork : IDisposable
    {
        IPlayerRepository Players { get; }
        Task CommitAsync();
        Task EnsureCreated();
    }
}