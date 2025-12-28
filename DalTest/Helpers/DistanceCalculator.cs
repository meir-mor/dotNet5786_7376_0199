using System.Text.Json;
using System.Globalization;

namespace DalTest.Helpers;

public static class DistanceCalculator
{
    private static readonly HttpClient Client = new();

    /// <summary>
    /// Resolve coordinates for an address using external geocoding service.
    /// Returns (null, null) when address not found; converts unexpected errors to <see cref="BO.BlException"/>.
    /// Uses `var` and `TryParse` for numeric parsing.
    /// </summary>
    public static (double? Latitude, double? Longitude) GetCoordinates(string? address)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(address))
                return (null, null);

            var url = $"https://geocode.maps.co/search?q={address}&api_key=69304622ebba1769305894zyn57146b";

            var response = Client.GetAsync(url).GetAwaiter().GetResult();
            if (!response.IsSuccessStatusCode)
                return (null, null);

            var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.ValueKind != JsonValueKind.Array || doc.RootElement.GetArrayLength() == 0)
                return (null, null);

            var first = doc.RootElement[0];

            var latStr = first.GetProperty("lat").GetString();
            var lonStr = first.GetProperty("lon").GetString();

            var latOk = double.TryParse(latStr, NumberStyles.Float | NumberStyles.AllowThousands,
                CultureInfo.InvariantCulture, out var lat);
            var lonOk = double.TryParse(lonStr, NumberStyles.Float | NumberStyles.AllowThousands,
                CultureInfo.InvariantCulture, out var lon);

            if (!latOk || !lonOk) return (null, null);
            if (lat is < -90 or > 90 || lon is < -180 or > 180) return (null, null);

            return (lat, lon);
        }
        catch (Exception ex)
        {
            // convert unexpected errors to BO-level exception per PL/BL contract
            throw new DO.DalDistanceException("Error while resolving coordinates.", ex);
        }
    }

    /// <summary>
    /// Calculate route distance (kilometers) between two coordinates using routing service.
    /// Returns 0 on missing data or service failure; unexpected exceptions are wrapped as <see cref="BO.BlException"/>.
    /// </summary>
    public static double CalculateDistance(double? lat1, double? lon1, double? lat2, double? lon2,
        DO.ShipmentType distanceType)
    {
        try
        {
            if (lat1 == null || lon1 == null || lat2 == null || lon2 == null)
                return 0.0;

            var coordA =
                $"{lon1.Value.ToString(CultureInfo.InvariantCulture)},{lat1.Value.ToString(CultureInfo.InvariantCulture)}";
            var coordB =
                $"{lon2.Value.ToString(CultureInfo.InvariantCulture)},{lat2.Value.ToString(CultureInfo.InvariantCulture)}";

            var url = distanceType switch
            {
                DO.ShipmentType.Walk =>
                    $"https://routing.openstreetmap.de/routed-foot/route/v1/driving/{coordA};{coordB}?overview=false",
                DO.ShipmentType.Car =>
                    $"https://routing.openstreetmap.de/routed-car/route/v1/driving/{coordA};{coordB}?overview=false",
                DO.ShipmentType.Bicycle =>
                    $"https://routing.openstreetmap.de/routed-bike/route/v1/driving/{coordA};{coordB}?overview=false",
                DO.ShipmentType.Motorcycle =>
                    $"https://routing.openstreetmap.de/routed-car/route/v1/driving/{coordA};{coordB}?overview=false",
                _ => null
            };

            if (url == null) return 0.0;

            var response = Client.GetAsync(url).GetAwaiter().GetResult();
            if (!response.IsSuccessStatusCode) return 0.0;

            var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("routes", out var routes) || routes.ValueKind != JsonValueKind.Array ||
                routes.GetArrayLength() == 0)
                return 0.0;

            var firstRoute = routes[0];
            if (!firstRoute.TryGetProperty("distance", out var distProp) || distProp.ValueKind != JsonValueKind.Number)
                return 0.0;

            var meters = distProp.GetDouble();
            return meters / 1000.0;
        }
        catch (Exception ex)
        {
            throw new DO.DalDistanceException("Error while calculating distance.", ex);
        }
    }
}