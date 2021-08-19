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
            var a = await dal.IsPlayerExistsAsync(1);
            var b = await dal.IsPlayerExistsAsync(2);
            var c = await dal.FindPlayerByDeviceIdAsync(new System.Guid("83ED631F-7E78-45E6-B323-32FFCB29AA43"));
            var d = await dal.IsPlayerByDeviceIdExistsAsync(new System.Guid("83ED631F-7E78-45E6-B323-32FFCB29AA44"));
            await dal.AddOrUpdateResourceForPlayer(1, Common.Models.ResourceType.Coin, 10);
            await dal.AddOrUpdateResourceForPlayer(1, Common.Models.ResourceType.Coin, 500);
        }
    }
}