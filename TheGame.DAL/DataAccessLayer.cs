using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheGame.Common.Models;
using TheGame.DataService;

namespace TheGame.DAL
{
    public class DataAccessLayer
    {
        private readonly ILogger<DataAccessLayer> _log;
        private readonly IUnitOfWork _unitOfWork;

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

        public async Task SetOnlineStatusAsync(int playerId, bool isOnline)
        {
            var player = await _unitOfWork.Players.GetById(playerId);
            player.IsOnline = isOnline;
            await _unitOfWork.Players.Update(player);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<bool> IsPlayerByDeviceIdExistsAsync(Guid deviceId)
        {
            var player = await FindPlayerByDeviceIdAsync(deviceId);
            return player != null;
        }

        public async Task<Player> FindPlayerByDeviceIdAsync(Guid deviceId)
        {
            var player = await _unitOfWork.Players.Find(x => x.DeviceId == deviceId, null, Common.Constants.STRING_Resources);
            return player.FirstOrDefault();
        }

        public async Task AddOrUpdateResourceForPlayer(int playerId,ResourceType resourceType,double resourceValue)
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
            await _unitOfWork.Players.Update(player);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<Player> RegisterPlayerAsync(Guid deviceId,bool isOnline)
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
                return newPlayer;
            }
        }

    }
}