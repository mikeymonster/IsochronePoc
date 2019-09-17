using System.Collections.Generic;
using System.Threading.Tasks;

namespace IsochronePoc.Application.TravelTimeFilterApi
{
    public interface ITravelTimeFilterApiClient
    {
        Task<IList<DistanceSearchResult>> Search(string postcode, decimal latitude, decimal longitude, IList<Application.Location> locations);
    }
}