using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using GameCodeDailyKeyBot.DataAccess.DataObjects;
using GameCodeDailyKeyBot.Service.Models;

namespace GameCodeDailyKeyBot.Service.Mappings
{
    static class SteamKeyMapping
    {
        const string DateFormat = "yyyy.MM.dd";
        
        internal static SteamKey ToServiceModel(this SteamKeyEntity dataObject)
        {
            SteamKey serviceModel = new SteamKey();
            serviceModel.Id = dataObject.Id;
            serviceModel.DateReceived = DateTime.ParseExact(dataObject.DateReceived, DateFormat, CultureInfo.InvariantCulture);
            serviceModel.Username = dataObject.Username;
            serviceModel.Code = dataObject.Code;

            return serviceModel;
        }

        internal static SteamKeyEntity ToDataObject(this SteamKey serviceModel)
        {
            SteamKeyEntity dataObject = new SteamKeyEntity();
            dataObject.Id = serviceModel.Id;
            dataObject.DateReceived = serviceModel.DateReceived.ToString(DateFormat);
            dataObject.Username = serviceModel.Username;
            dataObject.Code = serviceModel.Code;

            return dataObject;
        }

        internal static IEnumerable<SteamKey> ToServiceModels(this IEnumerable<SteamKeyEntity> dataObjects)
        {
            IEnumerable<SteamKey> serviceModels = dataObjects.Select(dataObject => dataObject.ToServiceModel());

            return serviceModels;
        }

        internal static IEnumerable<SteamKeyEntity> ToEntities(this IEnumerable<SteamKey> serviceModels)
        {
            IEnumerable<SteamKeyEntity> dataObjects = serviceModels.Select(serviceModel => serviceModel.ToDataObject());

            return dataObjects;
        }
    }
}
