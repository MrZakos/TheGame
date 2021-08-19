using NUnit.Framework;
using Serilog;
using System.Threading.Tasks;
using TheGame.BootstrapService;

namespace TheGame.UnitTests
{
    public class LoggerTest
    {
        [SetUp]
        public async Task Setup()
        {
            Bootstrap.ConsoleApplicationBoostrap();
        }

        [Test]
        public async Task Test()
        {
            Log.Logger.Information("Information");
            Log.Logger.Debug("Debug");
            Log.Logger.Error("Error");
            Log.Logger.Verbose("Verbose");
            Log.Logger.Fatal("Fatal");
            Log.Logger.Warning("Warning");
            Assert.IsTrue(true);
        }    
    }
}