using System.Text.Json;

namespace DalTest.Helpers;

static class DistanceCalculator
{
    /// <summary>
    /// Synchronously retrieves the latitude and longitude for a given address.
    /// </summary>
    /// <param name="address">The address for which to retrieve the coordinates.</param>
    /// <returns>A tuple containing the latitude and longitude of the address. Returns null values if the address is not found.</returns>
    /// <exception cref="BlInvalidAddressCoordinatesException">Thrown if the address is null or invalid.</exception>
    internal static (double? Latitude, double? Longitude) GetCoordinates(string? address)
    {
        if (string.IsNullOrWhiteSpace(address))
            return (null, null);

        string url = "https://geocode.maps.co/search?q=" + address + "&api_key=69304622ebba1769305894zyn57146b";

        using HttpClient client = new HttpClient();

        var response = client.GetAsync(url).Result;

        if (!response.IsSuccessStatusCode) return (null, null);
        // throw new BlJsonException("Failed to retrieve coordinates for the given address.");

        var json = response.Content.ReadAsStringAsync().Result;

        var data = JsonDocument.Parse(json);

        if (data.RootElement.GetArrayLength() == 0) return (null, null);
        // throw new BlInvalidAddressCoordinatesException("Address not found.");

        var firstResult = data.RootElement[0];

        double latitude = double.Parse(firstResult.GetProperty("lat").GetString());
        double longitude = double.Parse(firstResult.GetProperty("lon").GetString());

        if (latitude < -90 || latitude > 90 || longitude < -180 || longitude > 180) return (null, null);
        // throw new BlInvalidLatitudeLongitudeException("Latitude or Longitude values are out of range.");

        return (latitude, longitude);
    }
    /// <summary>
    /// Calculates the distance between two addresses based on the specified distance type.
    /// </summary>
    /// <param name="address1">The first address.</param>
    /// <param name="address2">The second address.</param>
    /// <param name="distanceType">The type of distance to calculate (Aerial, Walking, or Driving).</param>
    /// <returns>The distance in kilometers between the two addresses.</returns>
    /// <exception cref="BlInvalidAddressException">Thrown if one or both addresses are invalid or if the distance type is invalid.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the API request for walking or driving distance fails.</exception>
    internal static double CalculateDistance(double? lat1, double? lon1, double? lat2, double? lon2, DO.ShipmentType distanceType)
    {
        // Check if the addresses are valid.
        if (lat1 == null || lon1 == null || lat2 == null || lon2 == null)
            return 0; //throw new BlInvalidAddressException("One or both addresses are invalid or not found.");

        string url = distanceType switch
        {
            DO.ShipmentType.Walk => 
                $"https://routing.openstreetmap.de/routed-foot/route/v1/driving/{lon1},{lat1};{lon2},{lat2}?overview=false",

            DO.ShipmentType.Car => 
                $"https://routing.openstreetmap.de/routed-car/route/v1/driving/{lon1},{lat1};{lon2},{lat2}?overview=false",

            DO.ShipmentType.Bicycle => 
                $"https://routing.openstreetmap.de/routed-bike/route/v1/driving/{lon1},{lat1};{lon2},{lat2}?overview=false",

            DO.ShipmentType.Motorcycle =>
                $"https://routing.openstreetmap.de/routed-car/route/v1/driving/{lon1},{lat1};{lon2},{lat2}?overview=false", // משתמש ברכב כתשתית

            // _ => throw new BlInvalidOperationException("Invalid DistanceType provided.")
        };

        using var client = new HttpClient();
        var response = client.GetAsync(url).Result;

        if (!response.IsSuccessStatusCode) return 0;
        // throw new BlJsonException("Failed to retrieve distance from API.");

        var json = response.Content.ReadAsStringAsync().Result;

        // Parse the JSON response
        var data = JsonDocument.Parse(json);
        double distance = data.RootElement
            .GetProperty("routes")[0]
            .GetProperty("distance")
            .GetDouble() / 1000.0; // Convert meters to kilometers

        return distance;
    }
}