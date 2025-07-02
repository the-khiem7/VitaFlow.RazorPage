using System.Collections.Generic;
using System.Threading.Tasks;
using VitaFlow.Core.Entities;
using VitaFlow.Core.Interfaces.Services;

namespace VitaFlow.Services
{
    /// <summary>
    /// Implementation of the ILocationService interface.
    /// </summary>
    public class LocationService : ILocationService
    {
        public Task<double> CalculateDistanceAsync(double lat1, double lon1, double lat2, double lon2)
        {
            // Implementation goes here
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<Location>> GetLocationsWithinRadiusAsync(double latitude, double longitude, double radiusInKm)
        {
            // Implementation goes here
            throw new System.NotImplementedException();
        }
    }
}
