using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

using NuciDAL.Repositories;
using NuciLog.Core;

using GameCodeDailyKeyBot.DataAccess.DataObjects;
using GameCodeDailyKeyBot.Client;
using GameCodeDailyKeyBot.Logging;
using GameCodeDailyKeyBot.Service.Mappings;
using GameCodeDailyKeyBot.Service.Models;

namespace GameCodeDailyKeyBot.Service
{
    public sealed class ProductKeySaver : IProductKeySaver
    {
        HttpClient httpClient;

        readonly IRepository<SteamKeyEntity> keyRepository;
        readonly IProductKeyManagerClient client;
        readonly ILogger logger;

        public ProductKeySaver(
            IRepository<SteamKeyEntity> keyRepository,
            IProductKeyManagerClient client,
            ILogger logger)
        {
            httpClient = new HttpClient();

            this.keyRepository = keyRepository;
            this.client = client;
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
            client.StoreProductKey(key);
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
    }
}
