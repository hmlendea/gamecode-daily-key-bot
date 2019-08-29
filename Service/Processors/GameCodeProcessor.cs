using System;
using System.Collections.Generic;

using NuciLog.Core;
using NuciWeb;

using NuciExtensions;
using OpenQA.Selenium;

using GameCodeDailyKeyBot.Logging;
using GameCodeDailyKeyBot.Service.Models;

namespace GameCodeDailyKeyBot.Service.Processors
{
    public sealed class GameCodeProcessor : WebProcessor
    {
        public string HomePageUrl => "https://gamecode.win";
        public string LogInUrl => $"{HomePageUrl}/login";
        public string LogOutUrl => $"{HomePageUrl}/logout";
        public string KeyExchangeUrl => $"{HomePageUrl}/exchange/keys";

        SteamAccount account;
        ILogger logger;
        IEnumerable<LogInfo> logInfos;

        public GameCodeProcessor(
            IWebDriver driver,
            SteamAccount account,
            ILogger logger)
            : base(driver)
        {
            this.account = account;
            this.logger = logger;
            this.logInfos = new List<LogInfo>
            {
                new LogInfo(MyLogInfoKey.Username, account.Username)
            };
        }

        public void LogIn()
        {
            logger.Info(MyOperation.LogIn, OperationStatus.Started, logInfos);

            GoToUrl(LogInUrl);

            By popupSelector = By.XPath(@"//cloudflare-app[1]");
            By popupWiggleButtonSelector = By.XPath(@"//div[contains(@class,'csa-wiggle')]");
            By popupCloseButtonSelector = By.XPath(@"/html/body/cloudflare-app[1]/div/div[3]/a");
            By popupSlidingSelector = By.XPath(@"//div[contains(@class,'csa-slide-in-bottom')]");
            By usernameSelector = By.Name("email");
            By passwordSelector = By.Name("password");
            By logInButtonSelector = By.XPath(@"//*[@id='loginForm']/form/button");
            By giveawayButtonSelector = By.Id("gamesToggle_0");
            
            SetText(usernameSelector, account.Username + "@yopmail.com");
            SetText(passwordSelector, account.Password);

            WaitForElementToExist(popupCloseButtonSelector, TimeSpan.FromSeconds(1));
            if (IsElementVisible(popupSelector))
            {
                Click(popupSelector);
                Click(popupCloseButtonSelector);
            }
            
            Click(logInButtonSelector);
            
            WaitForElementToExist(giveawayButtonSelector);

            logger.Debug(MyOperation.LogIn, OperationStatus.Success, logInfos);
        }

        public string GatherKey()
        {
            logger.Info(MyOperation.KeyGathering, OperationStatus.Started, logInfos);

            GoToUrl(KeyExchangeUrl);

            By keyToSendInputSelector = By.Id("steam_key");
            By receivedKeyInputSelector = By.Id("inputKey");
            By submitButtonSelector = By.Id("getKey");
            By giveawaysButtonSelector = By.XPath("/html/body/div[2]/div[1]/div/a[1]");
            By clockSelector = By.ClassName("flip-clock-active");
            
            WaitForAnyElementToBeVisible(clockSelector, keyToSendInputSelector);

            if (IsElementVisible(clockSelector))
            {
                logger.Error(MyOperation.KeyGathering, OperationStatus.Failure, "This account already claimed a key today", logInfos);
                return null;
            }

            string randomKey = GenerateRandomKey();

            // TODO: Trick to bypass the ads
            Click(giveawaysButtonSelector);
            Wait();
            NewTab(KeyExchangeUrl);

            SetText(keyToSendInputSelector, randomKey);

            Click(submitButtonSelector);
            WaitForElementToExist(receivedKeyInputSelector);

            if (string.IsNullOrWhiteSpace(GetText(receivedKeyInputSelector)))
            {
                Wait(1000);
            }

            string key = GetText(receivedKeyInputSelector).ToUpper().Replace("YOUR KEY", "").Trim();

            logger.Debug(MyOperation.KeyGathering, OperationStatus.Success, logInfos);
            
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
