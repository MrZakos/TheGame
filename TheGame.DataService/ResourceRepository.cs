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
    /// Resource Repository
    /// </summary>
    public class ResourceRepository : GenericRepository<Resource>, IResourceRepository
    {
        public ResourceRepository(
            TheGameDatabaseContext context,
            ILogger<ResourceRepository> logger) : base(context, logger)
        {
        }

        public override async Task<IEnumerable<Resource>> All()
        {
            try
            {
                return await dbSet.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Repo} All function error", typeof(PlayerRepository));
                return new List<Resource>();
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