using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

using NuciDAL.Repositories;
using NuciLog.Core;
using NuciSecurity.HMAC;

using GameCodeDailyKeyBot.DataAccess.DataObjects;
using GameCodeDailyKeyBot.Configuration;
using GameCodeDailyKeyBot.Logging;
using GameCodeDailyKeyBot.Service.Mappings;
using GameCodeDailyKeyBot.Service.Models;

namespace GameCodeDailyKeyBot.Service
{
    public sealed class ProductKeyManager : IProductKeyManager
    {
        HttpClient httpClient;

        readonly IHmacEncoder<StoreProductKeyRequest> storeProductKeyRequestHmacEncoder;
        readonly IRepository<SteamKeyEntity> keyRepository;
        readonly ProductKeyManagerSettings productKeyManagerSettings;
        readonly ILogger logger;

        public ProductKeyManager(
            IHmacEncoder<StoreProductKeyRequest> storeProductKeyRequestHmacEncoder,
            IRepository<SteamKeyEntity> keyRepository,
            ProductKeyManagerSettings productKeyManagerSettings,
            ILogger logger)
        {
            httpClient = new HttpClient();

            this.storeProductKeyRequestHmacEncoder = storeProductKeyRequestHmacEncoder;
            this.keyRepository = keyRepository;
            this.productKeyManagerSettings = productKeyManagerSettings;
            this.logger = logger;
        }

        public void StoreProductKey(SteamKey key)
        {
            StoreProductKeyLocally(key);
            StoreProductKeyRemotely(key);
        }

        public void StoreProductKeyLocally(SteamKey key)
        {
            logger.Info(MyOperation.LocalKeySaving, OperationStatus.Started, new LogInfo(MyLogInfoKey.ProductKey, key.Code));

            keyRepository.Add(key.ToDataObject());
            keyRepository.ApplyChanges();
            
            logger.Debug(MyOperation.LocalKeySaving, OperationStatus.Success, new LogInfo(MyLogInfoKey.ProductKey, key.Code));
        }

        public void StoreProductKeyRemotely(SteamKey key)
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


        public DateTime GetLatestClaimDate(string username)
        {
            IEnumerable<SteamKey> claimedKeys = keyRepository
                .GetAll()
                .ToServiceModels()
                .Where(x => x.Username == username);
            
            if (!claimedKeys.Any())
            {
                return DateTime.MinValue;
            }

            SteamKey latestClaimedKey = claimedKeys
                .OrderBy(x => x.DateReceived)
                .Last();

            return latestClaimedKey.DateReceived;
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
