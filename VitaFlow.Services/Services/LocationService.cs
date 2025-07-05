using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VitaFlow.Core.Entities;
using VitaFlow.Core.Interfaces.Repositories;
using VitaFlow.Core.Interfaces.Services;

namespace VitaFlow.Services.Services
{
    /// <summary>
    /// Implementation of the ILocationService interface.
    /// </summary>
    public class LocationService : ILocationService
    {
        private readonly ILogger<LocationService> _logger;
        
        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        /// <param name="logger">The logger instance</param>
        public LocationService(ILogger<LocationService> logger)
        {
            _logger = logger;
        }
        
        /// <summary>
        /// Calculates the distance between two geographical coordinates using the Haversine formula.
        /// </summary>
        /// <param name="lat1">Latitude of the first point</param>
        /// <param name="lon1">Longitude of the first point</param>
        /// <param name="lat2">Latitude of the second point</param>
        /// <param name="lon2">Longitude of the second point</param>
        /// <returns>Distance in kilometers</returns>
        public Task<double> CalculateDistanceAsync(double lat1, double lon1, double lat2, double lon2)
        {
            try
            {
                _logger.LogInformation("Calculating distance between ({Lat1}, {Lon1}) and ({Lat2}, {Lon2})", lat1, lon1, lat2, lon2);
                
                // Implementation of the Haversine formula
                const double earthRadiusKm = 6371.0; // Earth radius in kilometers
                
                var dLat = DegreesToRadians(lat2 - lat1);
                var dLon = DegreesToRadians(lon2 - lon1);
                
                lat1 = DegreesToRadians(lat1);
                lat2 = DegreesToRadians(lat2);
                
                var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                        Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * 
                        Math.Cos(lat1) * Math.Cos(lat2);
                var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                var distance = earthRadiusKm * c;
                
                return Task.FromResult(distance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating distance between coordinates");
                throw;
            }
        }
        
        private double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        /// <summary>
        /// Gets all locations within the specified radius of a central point.
        /// </summary>
        /// <param name="latitude">Latitude of the central point</param>
        /// <param name="longitude">Longitude of the central point</param>
        /// <param name="radiusInKm">Search radius in kilometers</param>
        /// <returns>List of locations within the radius</returns>
        public Task<IEnumerable<Location>> GetLocationsWithinRadiusAsync(double latitude, double longitude, double radiusInKm)
        {
            try
            {
                _logger.LogInformation("Finding locations within {Radius}km of ({Lat}, {Lon})", radiusInKm, latitude, longitude);
                
                // This is a placeholder implementation since we don't have access to the actual location repository
                // In a real implementation, we would:
                // 1. Either use a geospatial query if the database supports it (e.g., SQL Server's geography type, PostgreSQL's PostGIS)
                // 2. Or retrieve all locations and filter them in-memory

                // For this skeleton implementation, we'll return an empty list
                // In a real-world scenario, you'd inject and use a location repository
                return Task.FromResult<IEnumerable<Location>>(new List<Location>());
                
                /* Example of how this would be implemented with a repository:
                var allLocations = await _locationRepository.GetAllAsync();
                var locationsWithinRadius = new List<Location>();
                
                foreach (var location in allLocations)
                {
                    var distance = await CalculateDistanceAsync(latitude, longitude, location.Latitude, location.Longitude);
                    if (distance <= radiusInKm)
                    {
                        locationsWithinRadius.Add(location);
                    }
                }
                
                return locationsWithinRadius;
                */
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding locations within radius");
                throw;
            }
        }
    }
}
