using NuciLog.Core;

namespace GameCodeDailyKeyBot.Logging
{
    public sealed class MyLogInfoKey : LogInfoKey
    {
        MyLogInfoKey(string name)
            : base(name)
        {
            
        }

        public static LogInfoKey Username => new MyLogInfoKey(nameof(Username));

        public static LogInfoKey ProductKey => new MyLogInfoKey(nameof(ProductKey));
    }
}
