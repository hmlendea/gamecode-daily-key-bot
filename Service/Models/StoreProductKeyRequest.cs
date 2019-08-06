namespace GameCodeDailyKeyBot.Service.Models
{
    public sealed class StoreProductKeyRequest
    {
        public string Store { get; set; }

        public string Product { get; set; }

        public string Key { get; set; }

        public string Status { get; set; }

        public string Hmac { get; set; }
    }
}
