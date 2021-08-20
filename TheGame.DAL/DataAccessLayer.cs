using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using TheGame.Common.Models;
using TheGame.DataService;

namespace TheGame.DAL
{
    public class DataAccessLayer
    {
        private readonly ILogger<DataAccessLayer> _log;
        private readonly IUnitOfWork _unitOfWork;
        private const string _defaultPlayerIncludeProperties = "Resources";

        public DataAccessLayer(
            ILogger<DataAccessLayer> logger,
            IUnitOfWork unitOfWork)
        {
            _log = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task EnsureCreated()
        {
            await _unitOfWork.EnsureCreated();
        }

        public async Task<bool> IsPlayerExistsAsync(int playerId)
        {
            var player = await _unitOfWork.Players.GetById(playerId);
            return player != null;
        }

        public async Task<Player> GetPlayerAsync(int playerId)
        {
            return await _unitOfWork.Players.GetById(playerId);
        }

        public async Task SetOnlineStatusAsync(int playerId, bool isOnline)
        {
            var player = await _unitOfWork.Players.GetById(playerId);
            player.IsOnline = isOnline;
            _unitOfWork.Players.Update(player);
            await _unitOfWork.CompleteAsync();
            _log.LogInformation($@"{player.DeviceId} was set to {(isOnline ? "online" : "offline")}");
        }

        public async Task<bool> IsPlayerByDeviceIdExistsAsync(Guid deviceId)
        {
            var player = await FindPlayerByDeviceIdAsync(deviceId);
            return player != null;
        }

        public async Task<Player> FindPlayerByDeviceIdAsync(Guid deviceId)
        {
            var player = await _unitOfWork.Players.Find(x => x.DeviceId == deviceId, null, _defaultPlayerIncludeProperties);
            return player.FirstOrDefault();
        }

        public async Task AddOrUpdateResourceForPlayerAsync(int playerId, ResourceType resourceType, double resourceValue)
        {
            var player = await _unitOfWork.Players.GetById(playerId);
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
            _unitOfWork.Players.Update(player);
            await _unitOfWork.CompleteAsync();
            _log.LogInformation($"{resourceValue} {resourceValue}  has been added to player ({player})");
        }

        public async Task<Player> RegisterPlayerAsync(Guid deviceId, bool isOnline)
        {
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
                await _unitOfWork.Players.Add(newPlayer);
                await _unitOfWork.CompleteAsync();
                _log.LogInformation($"player ({newPlayer}) had been regiserted");
                return newPlayer;
            }
        }

    }
}