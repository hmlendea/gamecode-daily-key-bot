using NuciLog.Core;

namespace GameCodeDailyKeyBot.Logging
{
    public sealed class MyOperation : Operation
    {
        MyOperation(string name)
            : base(name)
        {
            
        }

        public static Operation SteamLogIn => new MyOperation(nameof(SteamLogIn));

        public static Operation GameCodeLogIn => new MyOperation(nameof(GameCodeLogIn));

        public static Operation KeyGathering => new MyOperation(nameof(KeyGathering));
    }
}
