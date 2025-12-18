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
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        LoadOpenOrders();
    }

    private void LoadOpenOrders()
    {
        try
        {
            // requesterId = courierId for courier actions
            OpenOrders = s_bl.Order.GetOpenOrdersForCourier(_courierId, _courierId, null, null).ToList();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load open orders: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
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

            // refresh lists
            LoadOpenOrders();
            // notify owner to refresh (courier main window)
            if (Owner is CourierMainWindow c) c.LoadCourier();
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