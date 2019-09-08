using NuciSecurity.HMAC;

using GameCodeDailyKeyBot.Service.Models;

namespace GameCodeDailyKeyBot.Security
{
    public sealed class StoreProductKeyRequestHmacEncoder : HmacEncoder<StoreProductKeyRequest>
    {
        public override string GenerateToken(StoreProductKeyRequest obj, string sharedSecretKey)
        {
            string stringForSigning =
                obj.Store +
                obj.Product +
                obj.Key +
                obj.Owner +
                obj.Status;

            string hmacToken = ComputeHmacToken(stringForSigning, sharedSecretKey);

            return hmacToken;
        }
    }
}
