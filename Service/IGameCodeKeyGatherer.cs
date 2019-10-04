using GameCodeDailyKeyBot.Service.Models;

namespace GameCodeDailyKeyBot.Service
{
    public interface IGameCodeKeyGatherer
    {
        void LogIn(SteamAccount account);

        void LogOut();

        string GatherKey();
    }
}
