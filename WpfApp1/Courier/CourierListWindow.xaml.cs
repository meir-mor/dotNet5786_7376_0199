using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BO;
using PL.Helpers;

namespace PL.Courier;

/// <summary>
/// Courier list window (presentation layer).
/// Exposes a dependency property 'Couriers' bound to the DataGrid ItemsSource.
/// Supports filtering by BO.ShipmentType using a ComboBox populated from PL.ShipmentTypeItemsCollection.
/// </summary>
public partial class CourierListWindow : Window
{
    /// <summary>
    /// BL facade instance used to call logical services.
    /// </summary>
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    /// <summary>
    /// Backing storage of full courier list retrieved from BL (unfiltered).
    /// </summary>
    private List<BO.CourierInList> _allCouriers = new();

    // Stage 7: Mutex for thread-safe observer updates
    private readonly ObserverMutex _courierListMutex = new();

    /// <summary>
    /// Selected shipment-type filter (bound TwoWay from ComboBox SelectedValue).
    /// Null means "All".
    /// </summary>
    public BO.ShipmentType? SelectedShipment { get; set; } = null;

    /// <summary>
    /// List of couriers exposed to the view (DataGrid ItemsSource).
    /// Bind via XAML: ItemsSource="{Binding Couriers}".
    /// </summary>
    public IEnumerable<BO.CourierInList>? Couriers
    {
        get => (IEnumerable<BO.CourierInList>?)GetValue(CouriersProperty);
        set => SetValue(CouriersProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="Couriers"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty CouriersProperty =
        DependencyProperty.Register(nameof(Couriers), typeof(IEnumerable<BO.CourierInList>), typeof(CourierListWindow),
            new PropertyMetadata(null));

    /// <summary>
    /// Initializes a new instance of <see cref="CourierListWindow"/>.
    /// </summary>
    public CourierListWindow()
    {
        InitializeComponent();
        DataContext = this;
    }


    /// <summary>
    /// Refresh couriers from BL (single BL call), cache them and apply current shipment-type filter.
    /// </summary>
    private void RefreshCouriers()
    {
        #region Stage 7 (for multithreading)
        if (_courierListMutex.CheckAndSetLoadInProgressOrRestartRequired())
            return;

        _ = Dispatcher.BeginInvoke(async () =>
        {
            try
            {
                var managerId = s_bl.Admin.GetConfig().ManagerId;
                // single BL call to get couriers (BL does not support shipment-type filter currently)
                _allCouriers = s_bl.Courier.GetCouriersList(managerId).ToList();
                ApplyShipmentFilter();
            }
            catch (BO.BlException ex)
            {
                MessageBox.Show($"Unable to load couriers", "Load Couriers", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception)
            {
                MessageBox.Show("An unexpected error occurred while loading couriers. Please try again or contact support.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (await _courierListMutex.UnsetLoadInProgressAndCheckRestartRequested())
                RefreshCouriers();
        });
        #endregion Stage 7 (for multithreading)
    }

    /// <summary>
    /// Apply the SelectedShipment filter to the cached _allCouriers and update the Couriers DP.
    /// Filtering is performed in PL because the BL interface does not expose a shipment-type filter.
    /// </summary>
    private void ApplyShipmentFilter()
    {
        Couriers = SelectedShipment == null
            ? s_bl?.Courier.GetAllCouriersInList()!
            : s_bl?.Courier.GetAllCouriersInList(null, SelectedShipment.Value)!;
    }

    /// <summary>
    /// ComboBox SelectionChanged handler for shipment-type filter.
    /// Reapplies local filter; no extra BL calls are made for each selection.
    /// </summary>
    private void ShipmentFilter_SelectionChanged(object? sender, System.Windows.Controls.SelectionChangedEventArgs e)
        => ApplyShipmentFilter();

    /// <summary>
    /// Window Loaded handler: register observer and perform initial load.
    /// Observer callback will call RefreshCouriers to update cache and UI automatically.
    /// </summary>
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            s_bl.Courier.AddObserver(RefreshCouriers);
            RefreshCouriers();
        }
        catch (Exception)
        {
            MessageBox.Show("Failed to initialize courier list view. Please try again.", "Initialization Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Window Closed handler: unregister observer to avoid callbacks after close.
    /// </summary>
    private void Window_Closed(object? sender, EventArgs e)
    {
        try
        {
            s_bl.Courier.RemoveObserver(RefreshCouriers);
        }
        catch
        {
            // ignore cleanup errors
        }
    }

    /// <summary>
    /// The currently selected item in the DataGrid (bound from XAML).
    /// Regular property (updated by binding from view -> code-behind).
    /// </summary>
    public BO.CourierInList? SelectedCourier { get; set; }

    /// <summary>
    /// DataGrid MouseDoubleClick handler: open selected courier in detail window (update mode).
    /// </summary>
    private void DataGrid_MouseDoubleClick(object? sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        try
        {
            if (SelectedCourier != null)
            {
                var wnd = new CourierMainWindow(SelectedCourier.Id, UserRole.Manager);
                wnd.Show();
            }
        }
        catch (Exception)
        {
            MessageBox.Show($"Cannot open courier details", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Add button click handler: open empty courier window (add mode).
    /// </summary>
    private void BtnAdd_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var wnd = new CourierWindow(0); // id=0 -> add mode
            wnd.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Cannot open add-courier window", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Delete button handler for a row. CommandParameter is bound to the courier Id in XAML.
    /// Asks for confirmation, calls BL.DeleteCourier and refreshes the list on success.
    /// </summary>
    private void DeleteCourier_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not Button btn || btn.CommandParameter is not int courierId)
                return;

            var confirm = MessageBox.Show("Are you sure you want to delete the selected courier?", "Confirm Delete",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            var managerId = s_bl.Admin.GetConfig().ManagerId;
            s_bl.Courier.DeleteCourier(managerId, courierId);

            // BL should notify observers and update the list; refresh here to ensure immediate feedback.
            RefreshCouriers();

            MessageBox.Show("Courier deleted.", "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (BO.BlException ex)
        {
            MessageBox.Show($"Delete failed", "Delete Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (Exception)
        {
            MessageBox.Show("Unexpected error while deleting courier. Please try again.", "Error", MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}