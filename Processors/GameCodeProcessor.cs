using NuciLog.Core;
using NuciWeb;

using NuciExtensions;
using OpenQA.Selenium;

using GameCodeDailyKeyBot.Entities;
using GameCodeDailyKeyBot.Logging;

namespace GameCodeDailyKeyBot.Processors
{
    public sealed class GameCodeProcessor : WebProcessor
    {
        public string HomePageUrl => "https://gamecode.win/";
        public string LogInUrl => $"{HomePageUrl}/login";
        public string KeyExchangeUrl => $"{HomePageUrl}/exchange/keys";

        AccountDetails account;
        ILogger logger;

        public GameCodeProcessor(
            IWebDriver driver,
            AccountDetails account,
            ILogger logger)
            : base(driver)
        {
            this.account = account;
            this.logger = logger;
        }

        public void LogIn()
        {
            logger.Info(
                MyOperation.GameCodeLogIn,
                OperationStatus.Started,
                new LogInfo(MyLogInfoKey.Username, account.Username));

            GoToUrl(LogInUrl);

            By usernameSelector = By.Name("email");
            By passwordSelector = By.Name("password");
            By giveawayButtonSelector = By.Id("gamesToggle_0");

            SetText(usernameSelector, account.Username + "@yopmail.com");
            SetText(passwordSelector, account.Password);
            
            Click(By.XPath(@"//*[@id='loginForm']/form/button"));
            
            WaitForElementToExist(giveawayButtonSelector);

            logger.Debug(
                MyOperation.GameCodeLogIn,
                OperationStatus.Success);
        }

        public string GatherKey()
        {
            logger.Info(
                MyOperation.KeyGathering,
                OperationStatus.Started,
                new LogInfo(MyLogInfoKey.Username, account.Username));

            GoToUrl(KeyExchangeUrl);

            By keyToSendInputSelector = By.Id("steam_key");
            By receivedKeyInputSelector = By.Id("inputKey");
            By submitButtonSelector = By.Id("getKey");
            By giveawaysButtonSelector = By.XPath("/html/body/div[2]/div[1]/div/a[1]");

            string randomKey = GenerateRandomKey();

            // TODO: Trick to bypass the ads
            Click(giveawaysButtonSelector);
            Wait();
            NewTab(KeyExchangeUrl);

            SetText(keyToSendInputSelector, randomKey);

            Click(submitButtonSelector);
            WaitForElementToExist(receivedKeyInputSelector);

            string key = GetText(receivedKeyInputSelector).Replace("Your key", "").Trim();

            logger.Debug(
                MyOperation.KeyGathering,
                OperationStatus.Success);
            
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
