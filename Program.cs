using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NuciDAL.Repositories;
using NuciLog;
using NuciLog.Core;

using GameCodeDailyKeyBot.Configuration;
using GameCodeDailyKeyBot.DataAccess.DataObjects;
using GameCodeDailyKeyBot.Service;

namespace SteamGiveawaysBot
{
    public sealed class Program
    {
        static ILogger logger;

        static DataSettings dataSettings;
        static DebugSettings debugSettings;

        static IServiceProvider serviceProvider;

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
            dataSettings = new DataSettings();
            debugSettings = new DebugSettings();
            
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            config.Bind(nameof(DataSettings), dataSettings);
            config.Bind(nameof(DebugSettings), debugSettings);

            return config;
        }

        static IServiceProvider CreateIOC()
        {
            return new ServiceCollection()
                .AddSingleton(dataSettings)
                .AddSingleton(debugSettings)
                .AddSingleton<ILogger, NuciLogger>()
                .AddSingleton<IRepository<SteamAccountEntity>>(s => new CsvRepository<SteamAccountEntity>(dataSettings.AccountsStorePath))
                .AddSingleton<IRepository<SteamKeyEntity>>(s => new CsvRepository<SteamKeyEntity>(dataSettings.KeysStorePath))
                .AddSingleton<IBotService, BotService>()
                .BuildServiceProvider();
        }

        static void Run()
        {
            IBotService bot = serviceProvider.GetService<IBotService>();

            try
            {
                bot.Run();
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
        }
    }
}
