using System;
using System.Collections.Generic;
using System.Linq;

using NuciDAL.Repositories;
using NuciLog.Core;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

using GameCodeDailyKeyBot.Configuration;
using GameCodeDailyKeyBot.DataAccess.DataObjects;
using GameCodeDailyKeyBot.Logging;
using GameCodeDailyKeyBot.Service.Mappings;
using GameCodeDailyKeyBot.Service.Models;
using GameCodeDailyKeyBot.Service.Processors;

namespace GameCodeDailyKeyBot.Service
{
    public class BotService : IBotService
    {
        public bool IsRunning { get; private set; }

        readonly IRepository<SteamAccountEntity> accountRepository;
        readonly IRepository<SteamKeyEntity> keyRepository;

        readonly DebugSettings debugSettings;
        readonly ILogger logger;

        public BotService(
            IRepository<SteamAccountEntity> accountRepository,
            IRepository<SteamKeyEntity> keyRepository,
            DebugSettings debugSettings,
            ILogger logger)
        {
            this.accountRepository = accountRepository;
            this.keyRepository = keyRepository;
            this.debugSettings = debugSettings;
            this.logger = logger;
        }

        public void Run()
        {
            IsRunning = true;

            IEnumerable<SteamAccount> accounts = accountRepository.GetAll().ToServiceModels();

            ProcessAccounts(accounts);
        }

        void ProcessAccounts(IEnumerable<SteamAccount> accounts)
        {
            IWebDriver driver = SetupDriver();
            IEnumerable<SteamKey> keys = keyRepository.GetAll().ToServiceModels();

            foreach (SteamAccount account in accounts)
            {
                if (keys.Any(x => x.Username == account.Username))
                {
                    DateTime today = DateTime.Now;
                    DateTime latestClaim = keys.Where(x => x.Username == account.Username).OrderBy(x => x.DateReceived).Last().DateReceived;

                    if (today < latestClaim.AddDays(1))
                    {
                        continue;
                    }
                }

                SteamKey key = TryGatherKey(account, driver);

                if (key is null)
                {
                    continue;
                }

                keyRepository.Add(key.ToDataObject());
                keyRepository.ApplyChanges();
            }

            driver.Quit();
        }

        SteamKey TryGatherKey(SteamAccount account, IWebDriver driver)
        {
            try
            {
                return GatherKey(account, driver);
            }
            catch (Exception ex)
            {
                logger.Error(MyOperation.KeyGathering, OperationStatus.Failure, ex, new LogInfo(MyLogInfoKey.Username, account.Username));

                return null;
            }
        }

        SteamKey GatherKey(SteamAccount account, IWebDriver driver)
        {
            string keyCode = null;

            using (GameCodeProcessor gameCodeProcessor = new GameCodeProcessor(driver, account, logger))
            {
                gameCodeProcessor.LogIn();
                keyCode = gameCodeProcessor.GatherKey();
                gameCodeProcessor.LogOut();
            }

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

        IWebDriver SetupDriver()
        {
            ChromeOptions options = new ChromeOptions();
            options.PageLoadStrategy = PageLoadStrategy.None;
            options.AddArgument("--silent");
            options.AddArgument("--no-sandbox");
			options.AddArgument("--disable-translate");
			options.AddArgument("--disable-infobars");

            if (debugSettings.IsHeadless)
            {
                options.AddArgument("--headless");
                options.AddArgument("--disable-gpu");
                options.AddArgument("--window-size=1920,1080");
                options.AddArgument("--start-maximized");
                options.AddArgument("--blink-settings=imagesEnabled=false");
                options.AddUserProfilePreference("profile.default_content_setting_values.images", 2);
            }

            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.SuppressInitialDiagnosticInformation = true;
            service.HideCommandPromptWindow = true;

            IWebDriver driver = new ChromeDriver(service, options);
            IJavaScriptExecutor scriptExecutor = (IJavaScriptExecutor)driver;
            string userAgent = (string)scriptExecutor.ExecuteScript("return navigator.userAgent;");

            if (userAgent.Contains("Headless"))
            {
                userAgent = userAgent.Replace("Headless", "");
                options.AddArgument($"--user-agent={userAgent}");

                driver.Quit();
                driver = new ChromeDriver(service, options);
            }

            driver.Manage().Window.Maximize();

            return driver;
        }
    }
}
