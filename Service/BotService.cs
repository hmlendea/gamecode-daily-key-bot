using System;
using System.Collections.Generic;

using NuciDAL.Repositories;
using NuciLog.Core;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

using GameCodeDailyKeyBot.DataAccess.DataObjects;
using GameCodeDailyKeyBot.Service.Mappings;
using GameCodeDailyKeyBot.Service.Models;
using GameCodeDailyKeyBot.Service.Processors;

namespace GameCodeDailyKeyBot.Service
{
    public class BotService : IBotService
    {
        public bool IsRunning { get; private set; }

        readonly IRepository<SteamAccountEntity> accountRepository;

        readonly ILogger logger;

        public BotService(
            IRepository<SteamAccountEntity> accountRepository,
            ILogger logger)
        {
            this.accountRepository = accountRepository;
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
            foreach (SteamAccount account in accounts)
            {
                IWebDriver driver = SetupDriver();

                try
                {
                    ProcessAccount(account, driver);
                }
                catch (Exception ex)
                {
                    driver.Quit();
                    logger.Error(Operation.Unknown, OperationStatus.Failure, ex);
                }
                finally
                {
                    driver.Quit();
                }
            }
        }

        void ProcessAccount(SteamAccount account, IWebDriver driver)
        {
            SteamProcessor steamProcessor = new SteamProcessor(driver, account, logger);
            steamProcessor.LogIn();

            GameCodeProcessor gameCodeProcessor = new GameCodeProcessor(driver, account, logger);
            gameCodeProcessor.LogIn();
            string key = gameCodeProcessor.GatherKey();

            Console.WriteLine(key);
        }

        IWebDriver SetupDriver()
        {
            ChromeOptions options = new ChromeOptions();
            options.PageLoadStrategy = PageLoadStrategy.None;
            options.AddArgument("--silent");
            options.AddArgument("--no-sandbox");
			options.AddArgument("--disable-translate");
			options.AddArgument("--disable-infobars");
            //options.AddArgument("--headless");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--window-size=1920,1080");
            options.AddArgument("--start-maximized");
            options.AddArgument("--blink-settings=imagesEnabled=false");
            options.AddUserProfilePreference("profile.default_content_setting_values.images", 2);

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
