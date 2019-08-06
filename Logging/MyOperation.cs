using NuciLog.Core;

namespace GameCodeDailyKeyBot.Logging
{
    public sealed class MyOperation : Operation
    {
        MyOperation(string name)
            : base(name)
        {
            
        }

        public static Operation LogIn => new MyOperation(nameof(LogIn));

        public static Operation LogOut => new MyOperation(nameof(LogOut));

        public static Operation KeyGathering => new MyOperation(nameof(KeyGathering));

        public static Operation KeySaving => new MyOperation(nameof(KeySaving));
    }
}
