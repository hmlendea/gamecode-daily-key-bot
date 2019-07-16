namespace GameCodeDailyKeyBot.Service.Models
{
    public sealed class SteamAccount
    {
        string username;

        public string Username
        {
            get { return username.ToLower(); }
            set { username = value; }
        }

        public string Password { get; set; }
    }
}
