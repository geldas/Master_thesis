using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace appDiplo.Models
{
    //===================================================================================
    // Objects for Deserialization of TomTom request
    public class TomTomMatrixResponse
    {
        public string formatVersion { get; set; }
        public Cell[][] matrix { get; set; }
        public Summary summary { get; set; }
    }

    public class Matrix
    {
        public IList<Cell> cell { get; set; }
    }
    public class Row
    {
        public Cell cell { get; set; }
    }

    public class Cell
    {
        public int statusCode { get; set; }
        public Response response { get; set; }
    }

    public class Response
    {
        public RouteSummary routeSummary { get; set; }
    }

    public class RouteSummary
    {
        public int lengthInMeters { get; set; }
        public int travelTimeInSeconds { get; set; }
        public int trafficDelayInSeconds { get; set; }
        public int trafficLengthInMeters { get; set; }
        public string departureTime { get; set; }
        public string arrivalTime { get; set; }
    }

    public class Summary
    {
        public int successfulRoutes { get; set; }
        public int totalRoutes { get; set; }
    }

    //===================================================================================
    // Objects for Matrix request to TomTom

    public class BingRequest
    {
        public List<Point> origins { get; set; }
        public List<Point> destinations { get; set; }
        public string travelMode { get; set; }
        public string timeUnit { get; set; }
    }

    public class BingMatrixResponse
    {
        public string authenticationResultCode { get; set; }
        public string brandLogoUri { get; set; }
        public string copyright { get; set; }
        public Cell2[] resourceSets { get; set; }
        public int statusCode { get; set; }
        public string statusDescription { get; set; }
        public string traceId { get; set; }
    }

    public class ResourceSet
    {
        public Cell2 cell { get; set; }
    }

    public class Cell2
    {
        public int estimatedTotal { get; set; }
        public List<Resources> resources { get; set; }

    }

    public class Resources
    {
        public List<Point> destinations { get; set; }
        public List<Point> origins { get; set; }
        public List<Results2> results { get; set; }
    }
    public class Results2
    {
        public int destinationIndex { get; set; }
        public int originIndex { get; set; }
        public double totalWalkDuration { get; set; }
        public double travelDistance { get; set; }
        public double travelDuration { get; set; }
    }
    public class TomTomRequest
    {
        public List<Origins> origins { get; set; }
        public List<Origins> destinations { get; set; }
    }

    public class Origins
    {
        public Point point { get; set; }
    }

    public class Destinations
    {
        public Point point { get; set; }
    }

    public class TopPoint
    {
        public Point point { get; set; }
    }
    public class Point
    {
        public double latitude { get; set; }
        public double longitude { get; set; }
    }
    //===================================================================================
}
