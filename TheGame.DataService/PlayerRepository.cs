﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    context.Dispose();
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
