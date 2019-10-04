using System;
using System.Threading;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NuciDAL.Repositories;
using NuciLog;
using NuciLog.Configuration;
using NuciLog.Core;
using NuciSecurity.HMAC;

using GameCodeDailyKeyBot.Client;
using GameCodeDailyKeyBot.Client.Models;
using GameCodeDailyKeyBot.Client.Security;
using GameCodeDailyKeyBot.Configuration;
using GameCodeDailyKeyBot.DataAccess.DataObjects;
using GameCodeDailyKeyBot.Logging;
using GameCodeDailyKeyBot.Service;

namespace SteamGiveawaysBot
{
    public sealed class Program
    {
        static ILogger logger;

        static BotSettings botSettings;
        static DataSettings dataSettings;
        static DebugSettings debugSettings;
        static ProductKeyManagerSettings productKeyManagerSettings;
        static NuciLoggerSettings loggingSettings;

        static IServiceProvider serviceProvider;

        static TimeSpan RetryDelay => TimeSpan.FromMinutes(5);

        static void Main(string[] args)
        {
            LoadConfiguration();
            serviceProvider = CreateIOC();

            logger = serviceProvider.GetService<ILogger>();
            logger.SetSourceContext<Program>();

            logger.Info(Operation.StartUp, $"Application started");
            Run();
            logger.Info(Operation.ShutDown, $"Application stopped");
        }
        
        static IConfiguration LoadConfiguration()
        {
            botSettings = new BotSettings();
            dataSettings = new DataSettings();
            debugSettings = new DebugSettings();
            productKeyManagerSettings = new ProductKeyManagerSettings();
            loggingSettings = new NuciLoggerSettings();
            
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            config.Bind(nameof(BotSettings), botSettings);
            config.Bind(nameof(DataSettings), dataSettings);
            config.Bind(nameof(DebugSettings), debugSettings);
            config.Bind(nameof(ProductKeyManagerSettings), productKeyManagerSettings);
            config.Bind(nameof(NuciLoggerSettings), loggingSettings);

            return config;
        }

        static IServiceProvider CreateIOC()
        {
            return new ServiceCollection()
                .AddSingleton(botSettings)
                .AddSingleton(dataSettings)
                .AddSingleton(debugSettings)
                .AddSingleton(productKeyManagerSettings)
                .AddSingleton(loggingSettings)
                .AddSingleton<ILogger, NuciLogger>()
                .AddSingleton<IRepository<SteamAccountEntity>>(s => new CsvRepository<SteamAccountEntity>(dataSettings.AccountsStorePath))
                .AddSingleton<IRepository<SteamKeyEntity>>(s => new CsvRepository<SteamKeyEntity>(dataSettings.KeysStorePath))
                .AddSingleton<IHmacEncoder<StoreProductKeyRequest>, StoreProductKeyRequestHmacEncoder>()
                .AddSingleton<IProductKeyManagerClient, ProductKeyManagerClient>()
                .AddSingleton<IProductKeySaver, ProductKeySaver>()
                .AddSingleton<IBotService, BotService>()
                .BuildServiceProvider();
        }

        static void Run()
        {
            IBotService bot = serviceProvider.GetService<IBotService>();

            while (true)
            {
                try
                {
                    bot.Run();
                    break;
                }
                catch (AggregateException ex)
                {
                    foreach (Exception innerException in ex.InnerExceptions)
                    {
                        logger.Fatal(Operation.Unknown, OperationStatus.Failure, innerException);
                    }
                }
                catch (Exception ex)
                {
                    logger.Fatal(Operation.Unknown, OperationStatus.Failure, ex);
                }

                logger.Info(
                    MyOperation.CrashRecovery,
                    new LogInfo(MyLogInfoKey.RetryDelay, RetryDelay.TotalMilliseconds));
                    
                Thread.Sleep((int)RetryDelay.TotalMilliseconds);
            }
        }
    }
}
