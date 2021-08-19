using System.Threading.Tasks;

namespace TheGame.DataService
{
    public interface IUnitOfWork
    {
        IPlayerRepository Players { get; }
        Task CompleteAsync();
        Task EnsureCreated();
    }
}