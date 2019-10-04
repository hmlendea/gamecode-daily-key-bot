using GameCodeDailyKeyBot.Service.Models;

namespace GameCodeDailyKeyBot.Client
{
    public interface IProductKeyManagerClient
    {
        void StoreProductKey(SteamKey key);
    }
}
