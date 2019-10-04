using GameCodeDailyKeyBot.Service.Models;

namespace GameCodeDailyKeyBot.Service.Processors
{
    public interface IGameCodeProcessor
    {
        void LogIn(SteamAccount account);

        void LogOut();

        string GatherKey();

        void ClearCookies();
    }
}
