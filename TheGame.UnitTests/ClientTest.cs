using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Threading.Tasks;
using TheGame.BootstrapService;
using TheGame.DataService;
using System;
using System.Linq;
using TheGame.Common.Interfaces;
using TheGame.ClientConsole;

namespace TheGame.UnitTests
{
    public class ClientTest
    {
        IClient client;

        [SetUp]
        public async Task Setup()
        {
            Bootstrap.ConsoleApplicationBoostrap();
            client = ActivatorUtilities.GetServiceOrCreateInstance<IClient>(Bootstrap.IHost.Services);
        }

        [TestCase("login 00000000-0000-0000-1000-000000000000")]
        [TestCase("update coin 50")]
        [TestCase("update roll 500")]
        [TestCase("gift 1 coin 250")]
        [TestCase("gift 2 roll 70")]
        public async Task CommandParserSuccessTest(string command)
        {
            var commandParser = new CommandParser(command);
            Assert.True(commandParser.IsValid);
        }

        [TestCase("login")]
        [TestCase("login 0")]
        [TestCase("login 00000000-0000-0000-1000-000000000000000 1 2")]
        [TestCase("update")]
        [TestCase("update coi 50")]
        [TestCase("update rol 500")]
        [TestCase("gift")]
        [TestCase("gift 1 coin")]
        [TestCase("gift 2 roll f")]
        public async Task CommandParserFailTest(string command)
        {
            var commandParser = new CommandParser(command);
            Assert.IsFalse(commandParser.IsValid);
        }

    }
}