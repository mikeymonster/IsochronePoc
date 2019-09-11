using System.Collections.Generic;
using System.Threading.Tasks;

namespace IsochronePoc.Application.GoogleDistanceMatrixApi
{
    public interface IGoogleDistanceMatrixApiClient
    {
        Task<IList<DistanceSearchResult>> Search(Venue origin, IList<Venue> venues);
    }
}