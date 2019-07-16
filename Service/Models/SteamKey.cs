using System;

namespace GameCodeDailyKeyBot.Service.Models
{
    public sealed class SteamKey
    {
        public string Id { get; set; }

        public DateTime DateReceived { get; set; }

        public string Username { get; set; }

        public string Code { get; set; }
    }
}
