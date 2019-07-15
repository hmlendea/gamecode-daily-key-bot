namespace GameCodeDailyKeyBot.Service
{
    public interface IBotService
    {
        bool IsRunning { get; }

        void Run();
    }
}
