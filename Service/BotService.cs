using System;
using System.Collections.Generic;
using System.Linq;

using NuciExtensions;
using NuciLog.Core;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

using GameCodeDailyKeyBot.Entities;
using GameCodeDailyKeyBot.Logging;
using GameCodeDailyKeyBot.Processors;

namespace GameCodeDailyKeyBot.Service
{
    public class BotService : IBotService
    {
        public bool IsRunning { get; private set; }

        readonly ILogger logger;

        readonly AccountDetails account;

        public BotService(
            AccountDetails account,
            ILogger logger)
        {
            this.logger = logger;
            this.account = account;
        }

        public void Run()
        {
            IsRunning = true;

            RunLoop();
        }

        void RunLoop()
        {
            while (IsRunning)
            {
                IWebDriver driver = SetupDriver();

                try
                {
                    DoStuff(driver);
                }
                catch (Exception ex)
                {
                    driver.Quit();
                    logger.Error(Operation.Unknown, OperationStatus.Failure, ex);
                }
            }
        }

        void DoStuff(IWebDriver driver)
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
