using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IsochronePoc.Application.GeoLocations
{
    public interface ILocationApiClient
    {
        Task<(bool, string)> IsTerminatedPostcode(string postcode);

        Task<(bool, string)> IsValidPostcode(string postcode);

        Task<Location> GetGeoLocationData(string postcode);

        Task<Location> GetTerminatedPostcodeGeoLocationData(string postcode);
    }
}
                   