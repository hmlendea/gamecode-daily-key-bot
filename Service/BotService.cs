using System;
using System.Collections.Generic;

using NuciDAL.Repositories;
using NuciLog.Core;

using OpenQA.Selenium;

using GameCodeDailyKeyBot.Configuration;
using GameCodeDailyKeyBot.DataAccess.DataObjects;
using GameCodeDailyKeyBot.Logging;
using GameCodeDailyKeyBot.Service.Mappings;
using GameCodeDailyKeyBot.Service.Models;

namespace GameCodeDailyKeyBot.Service
{
    public class BotService : IBotService
    {
        public bool IsRunning { get; private set; }

        readonly IGameCodeKeyGatherer gameCodeProcessor;
        readonly IRepository<SteamAccountEntity> accountRepository;
        readonly IProductKeySaver productKeyManager;

        readonly BotSettings botSettings;
        readonly DebugSettings debugSettings;
        readonly ILogger logger;

        public BotService(
            IGameCodeKeyGatherer gameCodeProcessor,
            IRepository<SteamAccountEntity> accountRepository,
            IProductKeySaver productKeyManager,
            BotSettings botSettings,
            DebugSettings debugSettings,
            ILogger logger)
        {
            this.gameCodeProcessor = gameCodeProcessor;
            this.accountRepository = accountRepository;
            this.productKeyManager = productKeyManager;
            this.botSettings = botSettings;
            this.debugSettings = debugSettings;
            this.logger = logger;
        }

        public void Run()
        {
            IsRunning = true;

            IEnumerable<SteamAccount> accounts = accountRepository.GetAll().ToServiceModels();

            foreach (SteamAccount account in accounts)
            {
                DateTime today = DateTime.Now;
                DateTime latestClaimDay = productKeyManager.GetLatestClaimDate(account.Username);

                if (today < latestClaimDay.AddDays(1))
                {
                    continue;
                }

                SteamKey key = TryGatherKey(account);
            
                if (key is null)
                {
                    continue;
                }

                if (key.Code.Replace("-", "").Length == 15)
                {
                    productKeyManager.StoreProductKey(key);
                }
                else
                {
                    productKeyManager.StoreProductKeyLocally(key);
                }
            }
        }

        SteamKey TryGatherKey(SteamAccount account)
        {
            SteamKey key = null;

            try
            {
                gameCodeProcessor.LogIn(account);
                key = GatherKey(account);
            }
            catch (WebDriverException)
            {
                throw;
            }
            catch (KeyAlreadyClaimedException)
            {
                key = new SteamKey();
                key.Username = account.Username;
            }
            catch (Exception ex)
            {
                logger.Error(MyOperation.KeyGathering, OperationStatus.Failure, ex, new LogInfo(MyLogInfoKey.Username, account.Username));
            }
            finally
            {
                gameCodeProcessor.LogOut();
            }

            return key;
        }

        SteamKey GatherKey(SteamAccount account)
        {
            string keyCode = null;
            
            keyCode = gameCodeProcessor.GatherKey();

            if (string.IsNullOrWhiteSpace(keyCode))
            {
                return null;
            }

            SteamKey key = new SteamKey();
            key.Id = Guid.NewGuid().ToString();
            key.DateReceived = DateTime.Now;
            key.Username = account.Username;
            key.Code = keyCode;

            return key;
        }
    }
}
