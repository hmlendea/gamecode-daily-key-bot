using System;
using System.Net;
using System.Threading;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NuciLog;
using NuciLog.Core;

using GameCodeDailyKeyBot.Entities;
using GameCodeDailyKeyBot.Service;

namespace SteamGiveawaysBot
{
    public sealed class Program
    {
        static string[] UsernameOptions = { "-u", "--user", "--username" };
        static string[] SecretKeyOptions = { "-k", "--ssk", "--key", "--secretkey", "--sharedsecretkey" };

        static TimeSpan RetryDelay => TimeSpan.FromSeconds(10);

        static ILogger logger;
        
        static TimeSpan ConnectionWaitTime => TimeSpan.FromMinutes(5);

        static void Main(string[] args)
        {
            IServiceProvider serviceProvider = new ServiceCollection()
                .AddSingleton<ILogger, NuciLogger>()
                .AddSingleton<IBotService, BotService>()
                .BuildServiceProvider();

            logger = serviceProvider.GetService<ILogger>();
            logger.SetSourceContext<Program>();
            logger.Info(Operation.StartUp, $"Application started");

            while (true)
            {
                try
                {
                    RunLoop(serviceProvider);
                }
                catch (AggregateException ex)
                {
                    foreach (Exception innerException in ex.InnerExceptions)
                    {
                        logger.Fatal(Operation.Unknown, OperationStatus.Failure, innerException);
                    }

                    break;
                }
                catch (Exception ex)
                {
                    logger.Fatal(Operation.Unknown, OperationStatus.Failure, ex);
                    break;
                }
            }

            logger.Info(Operation.ShutDown, $"Application stopped");
        }

        static void RunLoop(IServiceProvider serviceProvider)
        {
            AccountDetails account = new AccountDetails();
            account.Username = "xxx";
            account.Password = "xxx";
            
            IBotService botService = new BotService(account, logger);

            botService.Run();
        }
    }
}
