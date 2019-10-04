using System;
using System.Collections.Generic;
using System.Linq;

using NuciDAL.Repositories;
using NuciLog.Core;
using NuciWeb;

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
        readonly IProductKeySaver productKeyManager;

        readonly BotSettings botSettings;
        readonly DebugSettings debugSettings;
        readonly ILogger logger;

        IWebDriver driver;

        public BotService(
            IRepository<SteamAccountEntity> accountRepository,
            IProductKeySaver productKeyManager,
            BotSettings botSettings,
            DebugSettings debugSettings,
            ILogger logger)
        {
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

            driver = SetupDriver();

            try
            {
                ProcessAccounts(accounts);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (!(driver is null))
                {
                    driver.Quit();
                }
            }
        }

        void ProcessAccounts(IEnumerable<SteamAccount> accounts)
        {
            foreach (SteamAccount account in accounts)
            {
                DateTime today = DateTime.Now;
                DateTime latestClaimDay = productKeyManager.GetLatestClaimDate(account.Username);

                if (today < latestClaimDay.AddDays(1))
                {
                    continue;
                }

                SteamKey key = TryGatherKey(account, driver);
            
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

        SteamKey TryGatherKey(SteamAccount account, IWebDriver driver)
        {
            SteamKey key = null;

            try
            {
                key = GatherKey(account, driver);
            }
            catch (WebDriverException)
            {
                throw;
            }
            catch (KeyAlreadyClaimedException)
            {
                key = new SteamKey();
                key.Id = Guid.NewGuid().ToString();
                key.DateReceived = DateTime.Now;
                key.Username = account.Username;
                key.Code = string.Empty;
            }
            catch (Exception ex)
            {
                logger.Error(MyOperation.KeyGathering, OperationStatus.Failure, ex, new LogInfo(MyLogInfoKey.Username, account.Username));
            }

            return key;
        }

        SteamKey GatherKey(SteamAccount account, IWebDriver driver)
        {
            string keyCode = null;
            string mainWindow = driver.WindowHandles[0];
            
            using (WebProcessor webProcessor = new WebProcessor(driver))
            {
                GameCodeProcessor gameCodeProcessor = new GameCodeProcessor(webProcessor, account, logger);
                gameCodeProcessor.LogIn();
                keyCode = gameCodeProcessor.GatherKey();
                gameCodeProcessor.LogOut();
            }

            driver.WindowHandles.Where(w => w != mainWindow).ToList()
                .ForEach(w =>
                {
                    driver.SwitchTo().Window(w);
                    driver.Close();
                });

            driver.SwitchTo().Window(mainWindow);
            driver.Manage().Cookies.DeleteAllCookies();

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

            IWebDriver driver = new ChromeDriver(service, options, TimeSpan.FromSeconds(botSettings.PageLoadTimeout));
            IJavaScriptExecutor scriptExecutor = (IJavaScriptExecutor)driver;
            string userAgent = (string)scriptExecutor.ExecuteScript("return navigator.userAgent;");

            if (userAgent.Contains("Headless"))
            {
                userAgent = userAgent.Replace("Headless", "");
                options.AddArgument($"--user-agent={userAgent}");

                driver.Quit();
                driver = new ChromeDriver(service, options);
            }

            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(botSettings.PageLoadTimeout);
            driver.Manage().Window.Maximize();

            return driver;
        }
    }
}
