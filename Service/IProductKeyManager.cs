using System;

using GameCodeDailyKeyBot.Service.Models;

namespace GameCodeDailyKeyBot.Service
{
    public interface IProductKeyManager
    {
        void StoreProductKey(SteamKey key);

        DateTime GetLatestClaimDate(string username);
    }
}
