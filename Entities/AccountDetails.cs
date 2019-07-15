namespace GameCodeDailyKeyBot.Entities
{
    public sealed class AccountDetails
    {
        string username;

        public string Username
        {
            get { return username.ToLower(); }
            set { username = value; }
        }

        public string Password { get; set; }

        public bool IsRegisteredOnGameCode { get; set; }
    }
}
