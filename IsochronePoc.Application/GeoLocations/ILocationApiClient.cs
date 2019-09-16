using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IsochronePoc.Application.GeoLocations
{
    public interface ILocationApiClient
    {
        Task<Location> GetGeoLocationData(string postCode);

    }
}
