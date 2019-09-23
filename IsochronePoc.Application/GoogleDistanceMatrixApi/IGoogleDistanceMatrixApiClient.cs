using System.Collections.Generic;
using System.Threading.Tasks;

namespace IsochronePoc.Application.GoogleDistanceMatrixApi
{
    public interface IGoogleDistanceMatrixApiClient
    {
        Task<IList<DistanceSearchResult>> Search(Location origin, IList<Location> venues);

        Task<IList<DistanceSearchResult>> SearchJourney(string workplace, List<Journey> destinations, string travelMode);
    }
}