using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using BO;

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
        DependencyProperty.Register(nameof(Couriers), typeof(IEnumerable<BO.CourierInList>), typeof(CourierListWindow), new PropertyMetadata(null));

    /// <summary>
    /// Initializes a new instance of <see cref="CourierListWindow"/>.
    /// </summary>
    public CourierListWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Refresh couriers from BL (single BL call), cache them and apply current shipment-type filter.
    /// </summary>
    private void RefreshCouriers()
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
            MessageBox.Show($"Unable to load couriers: {ex.Message}", "Load Couriers", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (Exception)
        {
            MessageBox.Show("An unexpected error occurred while loading couriers. Please try again or contact support.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Apply the SelectedShipment filter to the cached _allCouriers and update the Couriers DP.
    /// Filtering is performed in PL because the BL interface does not expose a shipment-type filter.
    /// </summary>
    private void ApplyShipmentFilter()
    {
        if (_allCouriers == null) _allCouriers = new List<BO.CourierInList>();
        Couriers = SelectedShipment == null
            ? _allCouriers
            : _allCouriers.Where(c => c.ShipmentType == SelectedShipment.Value).ToList();
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
            MessageBox.Show("Failed to initialize courier list view. Please try again.", "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
}
