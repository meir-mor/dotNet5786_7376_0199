using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BO;

namespace PL.Order;

public partial class OrderListWindow : Window
{
    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    public BO.ShipmentType? SelectedShipment { get; set; } = null;

    public BO.OrderInList? SelectedOrder { get; set; }

    public IEnumerable<BO.OrderInList>? OrderList
    {
       
        get  =>   (IEnumerable<BO.OrderInList>)GetValue(OrderListProperty); 
        set  =>  SetValue(OrderListProperty, value); 

    }

    public static readonly DependencyProperty OrderListProperty =
        DependencyProperty.Register(nameof(OrderList), typeof(IEnumerable<BO.OrderInList>),
            typeof(OrderListWindow), new PropertyMetadata(null));

    private readonly int _managerId;

    public OrderListWindow()
    {
        InitializeComponent();
        DataContext = this;
        _managerId = s_bl.Admin.GetConfig().ManagerId;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            s_bl.Order.AddObserver(RefreshOrders);
            RefreshOrders();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load orders: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        try
        {
            s_bl.Order.RemoveObserver(RefreshOrders);
        }
        catch { }
    }

    private void ShipmentFilter_SelectionChanged(object? sender, System.Windows.Controls.SelectionChangedEventArgs e)
        => ApplyShipmentFilter();

    private void ApplyShipmentFilter()
    {
        RefreshOrders();
    }

    private void RefreshOrders()
    {
        try
        {
            var managerId = s_bl.Admin.GetConfig().ManagerId;

            OrderList = SelectedShipment == null
                ? s_bl.Order.GetOrdersList(managerId).ToList()
                : s_bl.Order.GetOrdersList(
                    managerId,
                    filterBy: BO.OrderListFilterBy.ShipmentType,
                    filterValue: SelectedShipment
                  ).ToList();
        }
        catch (BO.BlException)
        {
            MessageBox.Show("Unable to load orders", "Load Orders",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (Exception)
        {
            MessageBox.Show("An unexpected error occurred while loading orders.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }


    private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        try
        {
            if (SelectedOrder != null)
            {
                var wnd = new OrderWindow(SelectedOrder.OrderId);
                wnd.Show();
            }
        }
        catch
        {
            MessageBox.Show("Cannot open Order details", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnAdd_Click(object sender, RoutedEventArgs e)
    {
        var wnd = new AddOrderWindow();
        wnd.ShowDialog();
        RefreshOrders();
    }

    private void CancelOrder_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not Button btn || btn.CommandParameter is not int orderId)
                return;

            var confirm = MessageBox.Show("Are you sure you want to cancel the selected order?",
                 "Confirm Cancel", MessageBoxButton.YesNo, MessageBoxImage.Question);
   
            if (confirm != MessageBoxResult.Yes) return;

            var order = s_bl.Order.GetOrderDetails(_managerId, orderId);

            if (order.OrderStatus == BO.OrderStatus.InProgress)
            {
                NotifyCourierOrderCancelled(order);
            }

            s_bl.Order.CancelOrder(_managerId, orderId);
            RefreshOrders();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void NotifyCourierOrderCancelled(BO.Order order)
    {
        var activeDelivery = order.Deliveries?.FirstOrDefault(d => d.DeliveryEndType == null);
        if (activeDelivery?.CourierId is not int courierId)
            return;

        var courier = s_bl.Courier.GetCourierDetails(courierId);
        if (courier == null)
            return;

        // מייל:
        }
}
