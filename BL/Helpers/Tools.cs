namespace Helpers;

using BO;
using DO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using static System.Net.WebRequestMethods;

internal static class Tools
{
    public static string ToStringProperty<T>(this T t)
    {
        return string.Join(", ", t.GetType().GetProperties().Select(p =>
        {
            var value = p.GetValue(t);
            if (value is null)
                return $"{p.Name}: null";
            var valueType = value.GetType();
            if (valueType.IsArray || typeof(ICollection<>).IsAssignableFrom(valueType))
            {
                var items = value is Array array ? array.Cast<object>() : (ICollection<object>)value;
                return $"{p.Name}: [{string.Join(", ", items.Select(i => i.ToString()))}]";
            }

            return $"{p.Name}: {value}";
        }));
    }

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

        if (!response.IsSuccessStatusCode)
            throw new BlJsonException("Failed to retrieve coordinates for the given address.");

        var json = response.Content.ReadAsStringAsync().Result;

        var data = JsonDocument.Parse(json);

        if (data.RootElement.GetArrayLength() == 0)
            throw new BlInvalidAddressCoordinatesException("Address not found.");

        var firstResult = data.RootElement[0];

        var latitude = double.Parse(firstResult.GetProperty("lat").GetString() ?? string.Empty);
        var longitude = double.Parse(firstResult.GetProperty("lon").GetString() ?? string.Empty);

        if (latitude < -90 || latitude > 90 || longitude < -180 || longitude > 180)
            throw new BlInvalidLatitudeLongitudeException("Latitude or Longitude values are out of range.");

        return (latitude, longitude);
    }

    /// <summary>
    /// Calculates the distance between two addresses based on the specified distance type.
    /// </summary>
    /// <param name="distanceType">The type of distance to calculate (Aerial, Walking, or Driving).</param>
    /// <param name="lat1">The latitude of the first address.</param>
    /// <param name="lon1">The longitude of the first address.</param>
    /// <param name="lat2">The latitude of the second address.</param>
    /// <param name="lon2">The longitude of the second address.</param>
    /// <returns>The distance in kilometers between the two addresses.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the API request for walking or driving distance fails.</exception>
    internal static double CalculateDistance(double? lat1, double? lon1, double? lat2, double? lon2,
        DO.ShipmentType distanceType)
    {
        // Check if the addresses are valid.
        if (lat1 == null || lon1 == null || lat2 == null || lon2 == null)
            return 0; //throw new BlInvalidAddressException("One or both addresses are invalid or not found.");

        // Handle aerial distance calculation
        if (distanceType == DO.ShipmentType.Aerial)
        {
            return CalculateHaversineDistance(lat1.Value, lon1.Value, lat2.Value, lon2.Value);
        }

        // Handle walking/driving distance using an external API
        var url = distanceType switch
        {
            DO.ShipmentType.Walk or DO.ShipmentType.Bicycle =>
                $"https://routing.openstreetmap.de/routed-foot/route/v1/driving/{lon1},{lat1};{lon2},{lat2}?overview=false",
            DO.ShipmentType.Car or DO.ShipmentType.Motorcycle =>
                $"https://routing.openstreetmap.de/routed-car/route/v1/driving/{lon1},{lat1};{lon2},{lat2}?overview=false",
            _ => throw new BlInvalidOperationException("Invalid DistanceType provided.")
        };

        using var client = new HttpClient();
        var response = client.GetAsync(url).Result;

        if (!response.IsSuccessStatusCode)
            throw new BlJsonException("Failed to retrieve distance from API.");

        var json = response.Content.ReadAsStringAsync().Result;

        // Parse the JSON response
        var data = JsonDocument.Parse(json);
        var distance = data.RootElement
            .GetProperty("routes")[0]
            .GetProperty("distance")
            .GetDouble() / 1000.0; // Convert meters to kilometers

        return distance;
    }

    /// <summary>
    /// Calculates the aerial distance between two coordinates using the Haversine formula.
    /// </summary>
    /// <param name="lat1">The latitude of the first coordinate.</param>
    /// <param name="lon1">The longitude of the first coordinate.</param>
    /// <param name="lat2">The latitude of the second coordinate.</param>
    /// <param name="lon2">The longitude of the second coordinate.</param>
    /// <returns>The distance in kilometers between the two coordinates.</returns>
    public static double CalculateHaversineDistance(double? lat1, double? lon1, double? lat2, double? lon2)
    {
        if (lat1 == null || lon1 == null || lat2 == null || lon2 == null)
            return 0;
        
        const double R = 6371; // Earth's radius in kilometers
        var dLat = DegreesToRadians((double)(lat2 - lat1));
        var dLon = DegreesToRadians((double)(lon2 - lon1));

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(DegreesToRadians((double)lat1)) * Math.Cos(DegreesToRadians((double)lat2)) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    /// <summary>
    /// Converts degrees to radians.
    /// </summary>
    /// <param name="degrees">The value in degrees.</param>
    /// <returns>The value in radians.</returns>
    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180;
}