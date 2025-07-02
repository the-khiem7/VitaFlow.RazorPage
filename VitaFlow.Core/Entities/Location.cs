using System;

namespace VitaFlow.Core.Entities
{
    /// <summary>
    /// Represents a geographical location.
    /// </summary>
    public class Location
    {
        public int Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; }
    }
}
