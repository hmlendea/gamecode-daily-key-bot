using System;
using System.Linq;
using System.Security.Authentication;

using NuciLog.Core;
using NuciWeb;

using NuciExtensions;
using OpenQA.Selenium;

using GameCodeDailyKeyBot.Logging;
using GameCodeDailyKeyBot.Service.Models;

namespace GameCodeDailyKeyBot.Service
{
    public sealed class GameCodeKeyGatherer : IGameCodeKeyGatherer
    {
        public string HomePageUrl => "https://gamecode.win";
        public string LogInUrl => $"{HomePageUrl}/login";
        public string LogOutUrl => $"{HomePageUrl}/logout";
        public string KeyExchangeUrl => $"{HomePageUrl}/exchange/keys";

        readonly IWebDriver webDriver;
        readonly IWebProcessor webProcessor;
        readonly ILogger logger;

        public GameCodeKeyGatherer(
            IWebDriver webDriver,
            IWebProcessor webProcessor,
            ILogger logger)
        {
            this.webDriver = webDriver;
            this.webProcessor = webProcessor;
            this.logger = logger;
        }

        public void LogIn(SteamAccount account)
        {
            LogInfo usernameLogInfo = new LogInfo(MyLogInfoKey.Username, account.Username);
            logger.Info(MyOperation.LogIn, OperationStatus.Started, usernameLogInfo);

            webProcessor.GoToUrl(LogInUrl);

            By popupSelector = By.XPath(@"//cloudflare-app[1]");
            By popupWiggleButtonSelector = By.XPath(@"//div[contains(@class,'csa-wiggle')]");
            By popupCloseButtonSelector = By.XPath(@"/html/body/cloudflare-app[1]/div/div[3]/a");
            By popupSlidingSelector = By.XPath(@"//div[contains(@class,'csa-slide-in-bottom')]");
            By usernameSelector = By.Name("email");
            By passwordSelector = By.Name("password");

            By bannerSelector = By.XPath(@"/html/body/div[8]/div[1]/div");
            By logInButtonSelector = By.XPath(@"//*[@id='loginForm']/form/button");
            By logOutButtonSelector = By.XPath(@"//a[contains(@href,'" + LogOutUrl + "')]");

            By invalidLoginSelector = By.XPath(@"/html/body/div[3]/div[1]/div/div[1]/ul/li");
            By giveawayButtonSelector = By.Id("gamesToggle_0");
            
            webProcessor.SetText(usernameSelector, account.Username + "@yopmail.com");
            webProcessor.SetText(passwordSelector, account.Password);

            webProcessor.WaitForElementToExist(popupCloseButtonSelector, TimeSpan.FromSeconds(1));
            if (webProcessor.IsElementVisible(popupSelector))
            {
                webProcessor.Click(popupSelector);
                webProcessor.Click(popupCloseButtonSelector);
            }
            
            webProcessor.Click(logInButtonSelector);
            webProcessor.WaitForAnyElementToExist(invalidLoginSelector, giveawayButtonSelector);

            if (webProcessor.IsElementVisible(invalidLoginSelector))
            {
                logger.Error(MyOperation.LogIn, OperationStatus.Failure, usernameLogInfo);
                throw new AuthenticationException(account.Username);
            }

            logger.Debug(MyOperation.LogIn, OperationStatus.Success, usernameLogInfo);
        }

        public void LogOut()
        {
            logger.Info(MyOperation.LogOut, OperationStatus.Started);
            
            By bannerSelector = By.XPath(@"/html/body/div[8]/div[1]/div");

            webDriver.WindowHandles
                .Skip(1)
                .ToList()
                .ForEach(tab =>
                {
                    webDriver.SwitchTo().Window(tab);
                    webDriver.Close();
                });
                
            webProcessor.NewTab(LogOutUrl);
            webProcessor.WaitForElementToExist(bannerSelector);
            webDriver.Manage().Cookies.DeleteAllCookies();

            logger.Debug(MyOperation.LogOut, OperationStatus.Success);
        }

        public string GatherKey()
        {
            logger.Info(MyOperation.KeyGathering, OperationStatus.Started);

            webProcessor.GoToUrl(KeyExchangeUrl);

            By keyToSendInputSelector = By.Id("steam_key");
            By receivedKeyInputSelector = By.Id("inputKey");
            By submitButtonSelector = By.Id("getKey");
            By giveawaysButtonSelector = By.XPath("/html/body/div[2]/div[1]/div/a[1]");
            By clockSelector = By.ClassName("flip-clock-active");
            
            webProcessor.WaitForAnyElementToBeVisible(clockSelector, keyToSendInputSelector);

            if (webProcessor.IsElementVisible(clockSelector))
            {
                Exception ex = new KeyAlreadyClaimedException();
                logger.Error(MyOperation.KeyGathering, OperationStatus.Failure, ex);
                throw ex;
            }

            string randomKey = GenerateRandomKey();

            // TODO: Trick to bypass the ads
            webProcessor.Click(giveawaysButtonSelector);
            webProcessor.Wait();
            webProcessor.NewTab(KeyExchangeUrl);

            webProcessor.SetText(keyToSendInputSelector, randomKey);

            webProcessor.Click(submitButtonSelector);
            webProcessor.WaitForElementToExist(receivedKeyInputSelector);

            if (string.IsNullOrWhiteSpace(webProcessor.GetText(receivedKeyInputSelector)))
            {
                webProcessor.Wait(1000);
            }

            string key = webProcessor.GetText(receivedKeyInputSelector).ToUpper().Replace("YOUR KEY", "").Trim();

            logger.Debug(MyOperation.KeyGathering, OperationStatus.Success);
            
            return key;
        }

        string GenerateRandomKey()
        {
            string allowedChars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string key = string.Empty;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    key += allowedChars.GetRandomElement();
                }

                key += "-";
            }

            key = key.Substring(0, key.Length - 1);

            return key;
        }
    }
}
