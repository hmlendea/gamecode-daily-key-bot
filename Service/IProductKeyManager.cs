using System;

using GameCodeDailyKeyBot.Service.Models;

namespace GameCodeDailyKeyBot.Service
{
    public interface IProductKeyManager
    {
        void StoreProductKey(SteamKey key);

        void StoreProductKeyLocally(SteamKey key);

        void StoreProductKeyRemotely(SteamKey key);

        DateTime GetLatestClaimDate(string username);
    }
}
