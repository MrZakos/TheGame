using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Threading.Tasks;
using TheGame.BootstrapService;
using TheGame.DataService;
using System;

namespace TheGame.UnitTests
{
    public class DataServiceTest
    {
        IUnitOfWork unitOfWork;

        [SetUp]
        public async Task Setup()
        {
            Bootstrap.ConsoleApplicationBoostrap();
            unitOfWork = ActivatorUtilities.GetServiceOrCreateInstance<IUnitOfWork>(Bootstrap.IHost.Services);
            await unitOfWork.EnsureCreated();
        }

        [Test]
        public async Task AddPlayer()
        {
            var player = new Common.Models.Player
            {
                DeviceId = Guid.NewGuid(),
                IsOnline = false
            };
            await unitOfWork.Players.Add(player);
            await unitOfWork.CompleteAsync();
            player = await unitOfWork.Players.GetById(player.Id);
            Assert.NotNull(player);
        }

        [Test]
        public async Task DeletePlayer()
        {
            var player = new Common.Models.Player
            {
                DeviceId = Guid.NewGuid(),
                IsOnline = false
            };
            await unitOfWork.Players.Add(player);
            await unitOfWork.CompleteAsync();
            player = await unitOfWork.Players.GetById(player.Id);
            Assert.NotNull(player);
            await unitOfWork.Players.Delete(player.Id);
            await unitOfWork.CompleteAsync();
            player = await unitOfWork.Players.GetById(player.Id);
            Assert.Null(player);
        }

        [Test]
        public async Task GetAllPlayers()
        {
            var allPlayers = await unitOfWork.Players.All();
            Assert.NotNull(allPlayers);
        }

        [TestCase(Common.Models.ResourceType.Coin, 72.0)]
        public async Task AddResourceToANewPlayer(Common.Models.ResourceType resourceType, double ammount)
        {
            var player = new Common.Models.Player
            {
                DeviceId = Guid.NewGuid(),
                IsOnline = false
            };
            await unitOfWork.Players.Add(player);
            await unitOfWork.CompleteAsync();
            player.Resources.Add(new Common.Models.Resource
            {
                PlayerId = player.Id,
                ResourceType = resourceType,
                ResourceValue = ammount
            });
            await unitOfWork.CompleteAsync();
            player = await unitOfWork.Players.GetById(player.Id);
            Assert.IsNotEmpty(player.Resources);

        }
    }
}