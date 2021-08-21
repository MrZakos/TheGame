using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using TheGame.Common.Models;
using TheGame.DataService;

namespace TheGame.DAL
{
    /// <summary>
    /// Data Access Layer - database operations logic on top of DatService
    /// </summary>
    public class DataAccessLayer
    {
        private readonly ILogger<DataAccessLayer> _log;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private const string _defaultPlayerIncludeProperties = "Resources";

        public DataAccessLayer(
            ILogger<DataAccessLayer> logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            _log = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task EnsureCreated()
        {
            using (var serviceScope = _serviceScopeFactory.CreateScope())
            {
                var unitOfWork = serviceScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                await unitOfWork.EnsureCreated();
            }
        }

        public async Task<bool> IsPlayerExistsAsync(int playerId)
        {
            using (var serviceScope = _serviceScopeFactory.CreateScope())
            {
                var unitOfWork = serviceScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var player = await unitOfWork.Players.GetById(playerId);
                return player != null;
            }
        }

        public async Task<Player> GetPlayerAsync(int playerId)
        {
            using (var serviceScope = _serviceScopeFactory.CreateScope())
            {
                var unitOfWork = serviceScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                return await unitOfWork.Players.GetById(playerId);
            }

        }

        public async Task SetOnlineStatusAsync(int playerId, bool isOnline)
        {
            using (var serviceScope = _serviceScopeFactory.CreateScope())
            {
                var unitOfWork = serviceScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var player = await unitOfWork.Players.GetById(playerId);
                player.IsOnline = isOnline;
                unitOfWork.Players.Update(player);
                await unitOfWork.CommitAsync();
                _log.LogInformation($@"{player.DeviceId} was set to {(isOnline ? "online" : "offline")}");
            }
        }

        public async Task<bool> IsPlayerByDeviceIdExistsAsync(Guid deviceId)
        {
            var player = await FindPlayerByDeviceIdAsync(deviceId);
            return player != null;
        }

        public async Task<Player> FindPlayerByDeviceIdAsync(Guid deviceId)
        {
            using (var serviceScope = _serviceScopeFactory.CreateScope())
            {
                var unitOfWork = serviceScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var player = await unitOfWork.Players.Find(x => x.DeviceId == deviceId, null, _defaultPlayerIncludeProperties);
                return player.FirstOrDefault();
            }
        }

        public async Task SetResourceForPlayerAsync(int playerId, ResourceType resourceType, double resourceValue)
        {
            using (var serviceScope = _serviceScopeFactory.CreateScope())
            {
                var unitOfWork = serviceScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var player = await unitOfWork.Players.GetById(playerId);
                var playerResource = player.Resources.FirstOrDefault(x => x.ResourceType == resourceType);
                var doesPlayerHasResource = playerResource != null;
                if (doesPlayerHasResource)
                {
                    playerResource.ResourceValue = resourceValue;
                }
                else
                {
                    player.Resources.Add(new Resource
                    {
                        PlayerId = player.Id,
                        ResourceType = resourceType,
                        ResourceValue = resourceValue
                    });
                }
                unitOfWork.Players.Update(player);
                await unitOfWork.CommitAsync();
                _log.LogInformation($"{resourceValue} {resourceType.ToString().ToLower()}s had been set to player {player}");
            }
        }

        public async Task<Player> RegisterPlayerAsync(Guid deviceId, bool isOnline)
        {
            using (var serviceScope = _serviceScopeFactory.CreateScope())
            {
                var unitOfWork = serviceScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var playerByDevice = await FindPlayerByDeviceIdAsync(deviceId);
                var playerExists = playerByDevice != null;
                if (playerExists)
                {
                    return playerByDevice;
                }
                else
                {
                    var newPlayer = new Player
                    {
                        DeviceId = deviceId,
                        IsOnline = isOnline
                    };
                    await unitOfWork.Players.Add(newPlayer);
                    await unitOfWork.CommitAsync();
                    _log.LogInformation($"player ({newPlayer}) had been regiserted");
                    return newPlayer;
                }
            }
        }
    }
}