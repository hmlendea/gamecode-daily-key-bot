using NuciLog.Core;
using NuciWeb;

using OpenQA.Selenium;

using GameCodeDailyKeyBot.Logging;
using GameCodeDailyKeyBot.Service.Models;

namespace GameCodeDailyKeyBot.Service.Processors
{
    public sealed class SteamProcessor : WebProcessor
    {
        public string HomePageUrl => "https://store.steampowered.com";
        public string LogInUrl => $"{HomePageUrl}/login/?redir=&redir_ssl=1";

        SteamAccount account;
        ILogger logger;

        public SteamProcessor(
            IWebDriver driver,
            SteamAccount account,
            ILogger logger)
            : base(driver)
        {
            this.account = account;
            this.logger = logger;
        }

        public void LogIn()
        {
            logger.Info(
                MyOperation.SteamLogIn,
                OperationStatus.Started,
                new LogInfo(MyLogInfoKey.Username, account.Username));

            GoToUrl(LogInUrl);

            By usernameSelector = By.Id("input_username");
            By passwordSelector = By.Id("input_password");
            By avatarSelector = By.XPath(@"//a[contains(@class,'playerAvatar')]");

            SetText(usernameSelector, account.Username);
            SetText(passwordSelector, account.Password);
            
            Click(By.XPath(@"//*[@id='login_btn_signin']/button"));
            
            WaitForElementToExist(avatarSelector);

            logger.Debug(
                MyOperation.SteamLogIn,
                OperationStatus.Success);
        }
    }
}
