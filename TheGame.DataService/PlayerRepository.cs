using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheGame.Common.Models;

namespace TheGame.DataService
{
    /// <summary>
    /// PlayerRepository
    /// </summary>
    public class PlayerRepository : GenericRepository<Player>, IPlayerRepository
    {
        public PlayerRepository(
            TheGameDatabaseContext context,
            ILogger<PlayerRepository> logger) : base(context, logger)
        {
        }

        public override async Task<Player> GetById(int id)
        {
            return await dbSet.Include(x => x.Resources).SingleOrDefaultAsync(i => i.Id == id);
        }

        public override async Task<IEnumerable<Player>> All()
        {
            try
            {
                return await dbSet.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{typeof(PlayerRepository)}.{nameof(All)}");
                return new List<Player>();
            }
        }

        public override async Task<bool> Delete(int id)
        {
            try
            {
                var exist = await dbSet.Where(x => x.Id == id).FirstOrDefaultAsync();

                if (exist == null) return false;

                dbSet.Remove(exist);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{typeof(PlayerRepository)}.{nameof(Delete)}");
                return false;
            }
        }
    }
}