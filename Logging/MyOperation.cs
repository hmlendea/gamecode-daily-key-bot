using NuciLog.Core;

namespace GameCodeDailyKeyBot.Logging
{
    public sealed class MyOperation : Operation
    {
        MyOperation(string name)
            : base(name)
        {
            
        }

        public static Operation CrashRecovery => new MyOperation(nameof(CrashRecovery));

        public static Operation LogIn => new MyOperation(nameof(LogIn));

        public static Operation LogOut => new MyOperation(nameof(LogOut));

        public static Operation KeyGathering => new MyOperation(nameof(KeyGathering));

        public static Operation LocalKeySaving => new MyOperation(nameof(LocalKeySaving));

        public static Operation RemoteKeySaving => new MyOperation(nameof(RemoteKeySaving));
    }
}
