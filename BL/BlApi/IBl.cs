namespace BlApi;

/// <summary>
/// Root business-layer interface that aggregates all logical service interfaces.
/// Implementations expose read-only properties for each service interface (no initialization here).
/// </summary>
public interface IBl
{
    ICourier Courier { get; }
    IOrder Order { get; }
    IAdmin Admin { get; }
}