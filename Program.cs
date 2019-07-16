using System;

using Microsoft.Extensions.DependencyInjection;

using NuciDAL.Repositories;
using NuciLog;
using NuciLog.Core;

using GameCodeDailyKeyBot.DataAccess.DataObjects;
using GameCodeDailyKeyBot.Service;

namespace SteamGiveawaysBot
{
    public sealed class Program
    {
        static ILogger logger;

        static IServiceProvider serviceProvider;

        static void Main(string[] args)
        {
            serviceProvider = CreateIOC();

            logger = serviceProvider.GetService<ILogger>();
            logger.SetSourceContext<Program>();

            logger.Info(Operation.StartUp, $"Application started");
            Run();
            logger.Info(Operation.ShutDown, $"Application stopped");
        }

        static IServiceProvider CreateIOC()
        {
            return new ServiceCollection()
                .AddSingleton<ILogger, NuciLogger>()
                .AddSingleton<IRepository<SteamAccountEntity>>(s => new CsvRepository<SteamAccountEntity>("Data/accounts.txt"))
                .AddSingleton<IRepository<SteamAccountEntity>>(s => new CsvRepository<SteamAccountEntity>("Data/keys.txt"))
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
