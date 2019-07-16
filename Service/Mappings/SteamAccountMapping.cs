using System.Collections.Generic;
using System.Linq;

using GameCodeDailyKeyBot.DataAccess.DataObjects;
using GameCodeDailyKeyBot.Service.Models;

namespace GameCodeDailyKeyBot.Service.Mappings
{
    static class SteamAccountMapping
    {
        internal static SteamAccount ToServiceModel(this SteamAccountEntity dataObject)
        {
            SteamAccount serviceModel = new SteamAccount();
            serviceModel.Username = dataObject.Username;
            serviceModel.Password = dataObject.Password;

            return serviceModel;
        }

        internal static SteamAccountEntity ToDataObject(this SteamAccount serviceModel)
        {
            SteamAccountEntity dataObject = new SteamAccountEntity();
            dataObject.Username = serviceModel.Username;
            dataObject.Password = serviceModel.Password;

            return dataObject;
        }

        internal static IEnumerable<SteamAccount> ToServiceModels(this IEnumerable<SteamAccountEntity> dataObjects)
        {
            IEnumerable<SteamAccount> serviceModels = dataObjects.Select(dataObject => dataObject.ToServiceModel());

            return serviceModels;
        }

        internal static IEnumerable<SteamAccountEntity> ToEntities(this IEnumerable<SteamAccount> serviceModels)
        {
            IEnumerable<SteamAccountEntity> dataObjects = serviceModels.Select(serviceModel => serviceModel.ToDataObject());

            return dataObjects;
        }
    }
}