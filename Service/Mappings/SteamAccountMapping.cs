using System.Collections.Generic;
using System.Linq;

using GameCodeDailyKeyBot.DataAccess.DataObjects;
using GameCodeDailyKeyBot.Service.Models;

namespace GameCodeDailyKeyBot.Service.Mappings
{
    internal static class SteamAccountMapping
    {
        internal static SteamAccount ToServiceModel(this SteamAccountEntity dataObject)
        {
            SteamAccount serviceModel = new SteamAccount();
            serviceModel.Username = dataObject.Username;
            serviceModel.Password = dataObject.Password;

            return serviceModel;
        }

        internal static IEnumerable<SteamAccount> ToServiceModels(this IEnumerable<SteamAccountEntity> dataObjects)
        {
            IEnumerable<SteamAccount> serviceModels = dataObjects.Select(dataObject => dataObject.ToServiceModel());

            return serviceModels;
        }
    }
}
