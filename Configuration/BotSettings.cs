namespace GameCodeDailyKeyBot.Configuration
{
    public sealed class BotSettings
    {
        public string UserAgent { get; set; }

        public int PageLoadTimeout { get; set; }

        public int CrashRecoveryDelay { get; set; }
    }
}
