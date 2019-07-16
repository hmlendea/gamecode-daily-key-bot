using System;

using NuciDAL.DataObjects;

namespace GameCodeDailyKeyBot.DataAccess.DataObjects
{
    public sealed class SteamKeyEntity : EntityBase
    {
        public DateTime DateReceived { get; set; }

        public string Username { get; set; }

        public string Code { get; set; }
    }
}
