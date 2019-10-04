using System;
using System.Net.Http;

using NuciLog.Core;
using NuciSecurity.HMAC;

using GameCodeDailyKeyBot.Client.Models;
using GameCodeDailyKeyBot.Configuration;
using GameCodeDailyKeyBot.Logging;
using GameCodeDailyKeyBot.Service.Models;

namespace GameCodeDailyKeyBot.Client
{
    public sealed class ProductKeyManagerClient : IProductKeyManagerClient
    {
        HttpClient httpClient;

        readonly IHmacEncoder<StoreProductKeyRequest> storeProductKeyRequestHmacEncoder;
        readonly ProductKeyManagerSettings productKeyManagerSettings;
        readonly ILogger logger;

        public ProductKeyManagerClient(
            IHmacEncoder<StoreProductKeyRequest> storeProductKeyRequestHmacEncoder,
            ProductKeyManagerSettings productKeyManagerSettings,
            ILogger logger)
        {
            httpClient = new HttpClient();

            this.storeProductKeyRequestHmacEncoder = storeProductKeyRequestHmacEncoder;
            this.productKeyManagerSettings = productKeyManagerSettings;
            this.logger = logger;
        }

        public void StoreProductKey(SteamKey key)
        {
            logger.Info(MyOperation.RemoteKeySaving, OperationStatus.Started, new LogInfo(MyLogInfoKey.ProductKey, key.Code));

            StoreProductKeyRequest request = BuildRequest(key.Code);
            string endpoint = BuildRequestUrl(request);

            // TODO: Broken async
            HttpResponseMessage httpResponse = httpClient.PostAsync(endpoint, null).Result;

            if (!httpResponse.IsSuccessStatusCode)
            {
                Exception exception = new HttpRequestException("Error sending the key to ProductKeyManager");
                logger.Debug(MyOperation.RemoteKeySaving, OperationStatus.Success, exception, new LogInfo(MyLogInfoKey.ProductKey, key.Code));

                throw exception;
            }
            
            logger.Debug(MyOperation.RemoteKeySaving, OperationStatus.Success, new LogInfo(MyLogInfoKey.ProductKey, key.Code));
        }

        StoreProductKeyRequest BuildRequest(string key)
        {
            StoreProductKeyRequest request = new StoreProductKeyRequest();
            request.Store = "Steam";
            request.Product = "Unknown - Daily GameCode key";
            request.Key = key;
            request.Status = "Unknown";
            request.Hmac = storeProductKeyRequestHmacEncoder.GenerateToken(request, productKeyManagerSettings.SharedSecretKey);

            return request;
        }

        string BuildRequestUrl(StoreProductKeyRequest request)
        {
            string endpoint =
                $"{productKeyManagerSettings.ApiUrl}" +
                $"?store={request.Store}" +
                $"&product={request.Product}" +
                $"&key={request.Key}" +
                $"&status={request.Status}" +
                $"&hmac={request.Hmac}";

            return endpoint;
        }
    }
}
