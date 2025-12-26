using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PL.Courier;

public partial class OpenOrdersWindow : Window
{
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
    private readonly int _courierId;
    
    // Observers
    private Action _listObserver;

    public IEnumerable<BO.OpenOrderInList>? OpenOrders
    {
        get => (IEnumerable<BO.OpenOrderInList>?)GetValue(OpenOrdersProperty);
        set => SetValue(OpenOrdersProperty, value);
    }
    public static readonly DependencyProperty OpenOrdersProperty =
        DependencyProperty.Register(nameof(OpenOrders), typeof(IEnumerable<BO.OpenOrderInList>), typeof(OpenOrdersWindow), new PropertyMetadata(null));

    public OpenOrdersWindow(int courierId)
    {
        _courierId = courierId;
        InitializeComponent();
        
        // Define observer action (thread-safe dispatch)
        _listObserver = () => Dispatcher.Invoke(LoadOpenOrders);
        
        // Handle closing to unsubscribe
        Closing += Window_Closing;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        // Initialize ComboBoxes
        var types = Enum.GetValues(typeof(BO.OrderType)).Cast<BO.OrderType>().ToList();
        CbFilterType.ItemsSource = new List<object> { "All" }.Concat(types.Cast<object>());
        CbFilterType.SelectedIndex = 0; // "All"

        CbSortBy.ItemsSource = Enum.GetValues(typeof(BO.OpenOrderSortBy));
        CbSortBy.SelectedItem = BO.OpenOrderSortBy.ScheduleStatus; // Default sort

        // Initial Load
        LoadOpenOrders();

        // Register Observer
        try
        {
            s_bl.Order.AddObserver(_listObserver);
        }
        catch (Exception ex) 
        { 
            // Logging or silent fail if observer not supported yet 
        }
    }

    private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        try
        {
            s_bl.Order.RemoveObserver(_listObserver);
        }
        catch { }
    }

    private void LoadOpenOrders()
    {
        try
        {
            // Get Filter
            BO.OrderType? typeFilter = null;
            if (CbFilterType.SelectedItem is BO.OrderType t) typeFilter = t;

            // Get Sort
            BO.OpenOrderSortBy? sortBy = null;
            if (CbSortBy.SelectedItem is BO.OpenOrderSortBy s) sortBy = s;

            // Fetch
            OpenOrders = s_bl.Order.GetOpenOrdersForCourier(_courierId, _courierId, typeFilter, sortBy);
        }
        catch (Exception ex)
        {
            // If window is closing, we might get exceptions on Dispatcher, can ignore or log
        }
    }

    private void CbFilterType_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        LoadOpenOrders();
    }

    private void CbSortBy_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        LoadOpenOrders();
    }

    private void BtnRefresh_Click(object sender, RoutedEventArgs e)
    {
        LoadOpenOrders();
    }

    private void BtnPick_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not Button btn || btn.CommandParameter is not int orderId) return;

            var confirm = MessageBox.Show("Pick this order for delivery?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            s_bl.Order.AssignOrderToCourier(_courierId, _courierId, orderId);

            MessageBox.Show("Order assigned to you.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);

            // refresh lists (observer will likely also trigger, but explicit refresh ensures immediate feedback)
            LoadOpenOrders();
            
            // notify owner to refresh (courier main window)
            if (Owner is CourierMainWindow c) c.LoadCourier();
            
            // Close this window? or stay open? 
            // Usually "Pick" implies taking a job. Maybe we should close or let them pick another?
            // User requirement: "Select an order to handle".
            // Typically couriers handle one at a time or few. 
            // I'll keep it open but they will see list updated.
        }
        catch (BO.BlException ex)
        {
            MessageBox.Show($"Assign failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Unexpected error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}