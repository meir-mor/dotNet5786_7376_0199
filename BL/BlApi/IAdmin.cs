using BO;

namespace BlApi;

/// <summary>
/// Business-layer service interface for administration tasks (database and configuration).
/// Implementations must wrap DAL calls in try/catch, translate DO exceptions to BO-level exceptions,
/// and must NOT keep BL-level collections of DO/BO entities (only operations).
/// </summary>
public interface IAdmin
{
    #region Stage 5
    void AddConfigObserver(Action configObserver);
    void RemoveConfigObserver(Action configObserver);
    void AddClockObserver(Action clockObserver);
    void RemoveClockObserver(Action clockObserver);
    #endregion Stage 5

    /// <summary>
    /// Reset the whole database to initial state.
    /// - Resets all configuration values to their initial defaults.
    /// - Empties all entity data lists (couriers, orders, deliveries, etc.).
    /// No parameters, no return value.
    /// Implementations should call the DAL Reset/clear methods and persist any configuration defaults.
    /// </summary>
    void ResetDB();

    /// <summary>
    /// Initialize database with starting data.
    /// Behavior:
    /// 1. Calls ResetDB().
    /// 2. Inserts initial data for all entities according to initialization requirements.
    /// No parameters, no return value.
    /// Implementations should use DAL create operations to populate initial data.
    /// </summary>
    void InitializeDB();

    /// <summary>
    /// Returns the current business clock value.
    /// </summary>
    /// <returns>DateTime - current system clock used across the system (BL-level clock)</returns>
    DateTime GetClock();

    /// <summary>
    /// Advance the business clock by one unit.
    /// The supplied <paramref name="unit"/> selects the time unit to advance (minute/hour/day/month/year).
    /// Implementations should compute the new DateTime and forward it to the BL helper that updates the clock
    /// (for example: AdminManager.UpdateClock(newTime)).
    /// </summary>
    /// <param name="unit">Time unit to advance the clock by one step.</param>
    void ForwardClock(TimeUnit unit);

    /// <summary>
    /// Get configuration values relevant to the presentation layer.
    /// Returns a BO.Config instance with the configuration fields that are exposed to the UI
    /// (do not include internal-only fields such as running id counters).
    /// </summary>
    /// <returns>BO.Config with current configuration values.</returns>
    BO.Config GetConfig();

    /// <summary>
    /// Set configuration values coming from the presentation layer.
    /// Implementations must update only the configuration fields that originate from the UI
    /// (exclude internal-only fields such as running id counters) and should persist changes via DAL.
    /// Only values that have changed since the previous update should be written.
    /// </summary>
    /// <param name="config">BO.Config containing new configuration values from UI.</param>
    void SetConfig(BO.Config config);
}