using System;
using System.Threading.Tasks;
using Coflnet.Sky.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Coflnet.Sky.Filter;
using Coflnet.Sky.Commands.Shared;
using Coflnet.Sky.Commands;

namespace SkyCommands
{
    public class Program
    {
        public static void Main(string[] args)
        {
            dev.Logger.Instance.Info("sky-commands");
            var FilterEngine = new FilterEngine();
            ItemPrices.AddFilters = FilterEngine.AddFilters;
            var server = new Server();
            var itemLoad = ItemDetails.Instance.LoadLookup();
            var serverTask = Task.Run(() => server.Start()).ConfigureAwait(false);

            // hook up cache refreshing
            CacheService.Instance.OnCacheRefresh += Server.ExecuteCommandHeadless;

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }

    public static class CommandSettings
    {
        public static bool Migrated = true;
        public static string InstanceId = "commands";
    }
}
