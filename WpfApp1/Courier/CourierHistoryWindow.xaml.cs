using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace PL.Courier;

public partial class CourierHistoryWindow : Window
{
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
    private readonly int _courierId;

    public IEnumerable<BO.ClosedDeliveryInList>? Deliveries
    {
        get => (IEnumerable<BO.ClosedDeliveryInList>?)GetValue(DeliveriesProperty);
        set => SetValue(DeliveriesProperty, value);
    }
    public static readonly DependencyProperty DeliveriesProperty =
        DependencyProperty.Register(nameof(Deliveries), typeof(IEnumerable<BO.ClosedDeliveryInList>), typeof(CourierHistoryWindow), new PropertyMetadata(null));

    public CourierHistoryWindow(int courierId)
    {
        _courierId = courierId;
        InitializeComponent();
        
        // Register observer for automatic updates
        s_bl.Order.AddObserver(NotificationCallback);
    }

    private void NotificationCallback()
    {
        // Dispatch to UI thread if needed
        Dispatcher.Invoke(() => LoadHistory());
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        // Populate Filter ComboBox
        var types = Enum.GetValues(typeof(BO.ShipmentType)).Cast<BO.ShipmentType>().ToList();
        CmbFilterType.ItemsSource = types;
        
        // Populate Sort ComboBox
        var sorts = Enum.GetValues(typeof(BO.ClosedDeliverySortBy)).Cast<BO.ClosedDeliverySortBy>().ToList();
        CmbSortBy.ItemsSource = sorts;
        CmbSortBy.SelectedItem = BO.ClosedDeliverySortBy.DeliveryEndTime; // Default sort

        LoadHistory();
    }
    
    // Unsubscribe when window closes to avoid memory leaks/errors
    protected override void OnClosed(EventArgs e)
    {
        s_bl.Order.RemoveObserver(NotificationCallback);
        base.OnClosed(e);
    }

    private void LoadHistory()
    {
        try
        {
            var filterType = CmbFilterType.SelectedItem as BO.ShipmentType?;
            var sortBy = CmbSortBy.SelectedItem as BO.ClosedDeliverySortBy?;

            Deliveries = s_bl.Order.GetClosedDeliveriesForCourier(_courierId, _courierId, filterType, sortBy)?.ToList();
        }
        catch (Exception ex)
        {
        }
    }

    private void CmbFilterType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        LoadHistory();
    }

    private void CmbSortBy_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        LoadHistory();
    }

    private void BtnRefresh_Click(object sender, RoutedEventArgs e)
    {
        CmbFilterType.SelectedItem = null;
        LoadHistory();
    }
}   