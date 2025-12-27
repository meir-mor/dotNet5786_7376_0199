using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

    public BO.CourierInList? SelectedCourier { get; set; }

    // Command for Deleting a Courier
    public ICommand DeleteCourierCommand { get; private set; }

    /// <summary>
    /// Initializes a new instance of <see cref="CourierListWindow"/>.
    /// </summary>
    public CourierListWindow()
    {
        InitializeComponent();

        DeleteCourierCommand = new RelayCommand(ExecuteDeleteCourier);
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
        catch (Exception)
        {
            MessageBox.Show("We could not refresh the courier list. Please check your connection or contact support.", "System Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Apply the SelectedShipment filter to the cached _allCouriers and update the Couriers DP.
    /// Filtering is performed in PL because the BL interface does not expose a shipment-type filter.
    /// </summary>
    private void ApplyShipmentFilter()
    {
        Couriers = SelectedShipment == null
         ? _allCouriers
         : _allCouriers.Where(c => c.ShipmentType == SelectedShipment).ToList();
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
        catch
        {
            MessageBox.Show("Unable to load data. The window will close.", "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
        }
    }

    /// <summary>
    /// Window Closed handler: unregister observer to avoid callbacks after close.
    /// </summary>
    private void Window_Closed(object? sender, EventArgs e)
    {
        try { s_bl.Courier.RemoveObserver(RefreshCouriers); } catch { }
    }

    private void DataGrid_MouseDoubleClick(object? sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        // Using Property Binding (SelectedCourier) instead of sender
        try
        {
            if (SelectedCourier != null)
            {
                new CourierMainWindow(SelectedCourier.Id, UserRole.Manager).Show();
            }
        }
        catch
        {
            MessageBox.Show("Could not open courier details.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Add button click handler: open empty courier window (add mode).
    /// </summary>
    private void BtnAdd_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            new CourierWindow(0).Show();
        }
        catch
        {
            MessageBox.Show("Could not open the add courier window.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Delete button handler for a row. CommandParameter is bound to the courier Id in XAML.
    /// Asks for confirmation, calls BL.DeleteCourier and refreshes the list on success.
    /// </summary>
    private void ExecuteDeleteCourier(object? parameter)
    {
        if (parameter is not int courierId) return;

        var confirm = MessageBox.Show("Are you sure you want to delete this courier?", "Delete Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (confirm != MessageBoxResult.Yes) return;

        try
        {
            var managerId = s_bl.Admin.GetConfig().ManagerId;
            s_bl.Courier.DeleteCourier(managerId, courierId);
            
            MessageBox.Show("The courier was successfully deleted.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            // Observer will refresh the list automatically
        }
        catch (BO.BlException ex)
        {
            MessageBox.Show($"Could not delete courier: {ex.Message}", "Operation Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (Exception)
        {
            MessageBox.Show("An unexpected error occurred while deleting the courier.", "System Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}