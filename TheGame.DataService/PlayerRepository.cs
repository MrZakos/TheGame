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
        public PlayerRepository(TheGameDatabaseContext context, ILogger logger) : base(context, logger)
        {
        }

        public override async Task<IEnumerable<Player>> All()
        {
            _logger.LogInformation("getting all players");
            try
            {
                return await dbSet.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Repo} All function error", typeof(PlayerRepository));
                return new List<Player>();
            }
        }

        public override async Task<bool> Update(Player entity)
        {
            try
            {
                var existingUser = await dbSet.Where(x => x.Id == entity.Id).FirstOrDefaultAsync();

                if (existingUser == null)
                    return await Add(entity);

                //existingUser.FirstName = entity.FirstName;
                //existingUser.LastName = entity.LastName;
                //existingUser.Email = entity.Email;

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Repo} Upsert function error", typeof(PlayerRepository));
                return false;
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
                _logger.LogError(ex, "{Repo} Delete function error", typeof(PlayerRepository));
                return false;
            }
        }
    }
}
