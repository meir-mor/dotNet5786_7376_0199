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
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        LoadHistory();
    }

    private void LoadHistory()
    {
        try
        {
            // requester = courierId
            Deliveries = s_bl.Order.GetClosedDeliveriesForCourier(_courierId, _courierId, null, null)?.ToList();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed loading history: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}