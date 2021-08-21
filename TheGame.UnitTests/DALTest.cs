using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Threading.Tasks;
using TheGame.BootstrapService;
using TheGame.DAL;

namespace TheGame.UnitTests
{
    public class DALTest
    {
        DataAccessLayer dal;

        [SetUp]
        public async Task Setup()
        {
            Bootstrap.ConsoleApplicationBoostrap();
            dal = ActivatorUtilities.GetServiceOrCreateInstance<DataAccessLayer>(Bootstrap.IHost.Services);
            await dal.EnsureCreated();
        }

        [Test]
        public async Task Test()
        {
            var player = await dal.RegisterPlayerAsync(System.Guid.NewGuid(), false);
            var a = await dal.IsPlayerExistsAsync(player.Id);
            var b = await dal.IsPlayerExistsAsync(50000);
            var c = await dal.FindPlayerByDeviceIdAsync(player.DeviceId);
            var d = await dal.IsPlayerByDeviceIdExistsAsync(System.Guid.NewGuid());
            await dal.SetResourceForPlayerAsync(player.Id, Common.Models.ResourceType.Coin, 10);
            await dal.SetResourceForPlayerAsync(player.Id, Common.Models.ResourceType.Coin, 50);
            await dal.SetResourceForPlayerAsync(1, Common.Models.ResourceType.Roll, 50);
            await dal.SetResourceForPlayerAsync(1, Common.Models.ResourceType.Roll, 200);
        }
    }
}