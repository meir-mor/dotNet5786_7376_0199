using BO;

namespace BlApi;

/// <summary>
/// Business-layer service interface for courier-related use cases.
/// Implementations must wrap DAL calls in try/catch, translate DO exceptions to BO-level exceptions,
/// and must NOT keep BL-level collections of DO/BO entities (only operations).
/// All methods receive a caller id (requesterId) for authorization/audit and must validate permissions accordingly.
/// </summary>
public interface ICourier : IObservable //stage 5
{
    /// <summary>
    /// Authenticate a user by username and password.
    /// Returns the user's role on success.
    /// Throws BO.InvalidCredentialsException on failure.
    /// </summary>
    /// <param name="username">username (non-empty)</param>
    /// <param name="password">password</param>
    /// <returns>UserRole</returns>
    UserRole Login(string username, string password);

    /// <summary>
    /// Request list of couriers for management screen.
    /// - requesterId: caller id (Tz) for authorization/audit.
    /// - isActiveFilter: if null return all; otherwise filter by active/inactive.
    /// - sortBy: if null sort by Id; otherwise sort by chosen property.
    /// Returns collection of BO.CourierInList.
    /// </summary>
    IEnumerable<BO.CourierInList> GetCouriersList(int requesterId, bool? isActiveFilter = null,
        CourierListSortBy? sortBy = null);

    IEnumerable<BO.CourierInList> GetCouriersListByFiltering(int requesterId, BO.ShipmentType filterBy);

    /// <summary>
    /// Request detailed courier data (BO.Courier), includes current order-in-progress if exists.
    /// Throws BO.BlDoesNotExistException if courier not found.
    /// </summary>
    BO.Courier GetCourierDetails(int courierId);

    /// <summary>
    /// Update courier data. The BO.Courier parameter is full and replaces existing values.
    /// Validates that shipment type is not changed while an order is in progress.
    /// </summary>
    void UpdateCourier(int requesterId, BO.Courier courier);

    /// <summary>
    /// Delete courier by id. Allowed only if courier has no active delivery and never completed deliveries.
    /// Allowed only for Manager role.
    /// Throws BO.BlInvalidRequestException if requester is not authorized.
    /// </summary>
    void DeleteCourier(int requesterId, int courierId);

    /// <summary>
    /// Add a new courier (BO.Courier). Caller must be authorized.
    /// Validates that ID is greater than 0 and unique.
    /// </summary>
    void AddCourier(BO.Courier courier);

    IEnumerable<BO.CourierInList> GetAllCouriersInList(Func<BO.CourierInList, bool>? customFilter = null,
        BO.ShipmentType? filter = null);
}