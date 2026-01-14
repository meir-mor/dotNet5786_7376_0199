using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BO;
using PL.Helpers;

namespace PL.Order;

public partial class OrderListWindow : Window
{
    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    public BO.ShipmentType? SelectedShipment { get; set; } = null;

    public BO.OrderInList? SelectedOrder { get; set; }

    public IEnumerable<BO.OrderInList>? OrderList
    {
        get => (IEnumerable<BO.OrderInList>)GetValue(OrderListProperty);
        set => SetValue(OrderListProperty, value);
    }

    public static readonly DependencyProperty OrderListProperty =
        DependencyProperty.Register(nameof(OrderList), typeof(IEnumerable<BO.OrderInList>),
            typeof(OrderListWindow), new PropertyMetadata(null));

    private readonly int _managerId;

    // Stage 7: Mutex for thread-safe observer updates
    private readonly ObserverMutex _orderListMutex = new();

    public OrderListWindow()
    {
        InitializeComponent();
        DataContext = this;
        _managerId = s_bl.Admin.GetConfig().ManagerId;
    }

    /// <summary>
    /// Handles the <see cref="Window.Loaded"/> event for the window. Subscribes to order updates and refreshes the
    /// displayed orders.
    /// </summary>
    /// <remarks>This method adds an observer to monitor order updates and immediately refreshes the order
    /// display. If an error occurs during the process, an error message is displayed to the user.</remarks>
    /// <param name="sender">The source of the event, typically the window being loaded.</param>
    /// <param name="e">The event data associated with the <see cref="Window.Loaded"/> event.</param>
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            s_bl.Order.AddObserver(RefreshOrdersObserver);
            RefreshOrders();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load orders: {ex.Message}", "Error", MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void Window_Closed(object sender, EventArgs e)
    {     
            s_bl.Order.RemoveObserver(RefreshOrdersObserver);
    }

    private void ShipmentFilter_SelectionChanged(object? sender, System.Windows.Controls.SelectionChangedEventArgs e)
        => ApplyShipmentFilter();

    private void ApplyShipmentFilter()
    {
        RefreshOrders();
    }

    /// <summary>
    /// Stage 7: Observer method with mutex pattern for thread-safe UI updates
    /// </summary>
    private void RefreshOrdersObserver()
    {
        #region Stage 7 (for multithreading)
        if (_orderListMutex.CheckAndSetLoadInProgressOrRestartRequired())
            return;

        _ = Dispatcher.BeginInvoke(async () =>
        {
            RefreshOrders();

            if (await _orderListMutex.UnsetLoadInProgressAndCheckRestartRequested())
                RefreshOrdersObserver();
        });
        #endregion Stage 7 (for multithreading)
    }

    /// <summary>
    /// Refreshes the list of orders based on the currently selected shipment type.
    /// </summary>
    /// <remarks>If no shipment type is selected, the method retrieves all orders associated with the current
    /// manager. Otherwise, it filters the orders by the selected shipment type. In case of an error, a message box is
    /// displayed to inform the user about the issue.</remarks>
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

    /// <summary>
    /// Handles the double-click event on a DataGrid row to open the details of the selected order.
    /// </summary>
    /// <remarks>This method checks if an order is selected in the DataGrid. If a valid order is selected, it
    /// opens a new window displaying the details of the selected order. If an error occurs while attempting to open the
    /// details, an error message is displayed to the user.</remarks>
    /// <param name="sender">The source of the event, typically the DataGrid.</param>
    /// <param name="e">The event data associated with the mouse button event.</param>
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
       //RefreshOrders();
    }

    /// <summary>
    /// Handles the click event for the "Cancel Order" button, allowing the user to cancel a selected order.
    /// </summary>
    /// <remarks>This method prompts the user for confirmation before canceling the order. If the order is in
    /// progress,  additional notifications may be triggered. The method ensures that only valid orders are processed 
    /// and handles any exceptions that occur during the operation by displaying an error message.</remarks>
    /// <param name="sender">The source of the event, expected to be a <see cref="Button"/> with a valid <see
    /// cref="Button.CommandParameter"/> representing the order ID.</param>
    /// <param name="e">The event data associated with the click event.</param>
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

   /// <summary>
   /// Notifies the system of a courier order cancellation by identifying the active delivery associated with the
   /// specified order and retrieving the courier details.
   /// </summary>
   /// <remarks>This method identifies the active delivery for the given order, if any, and retrieves the
   /// associated courier details. If no active delivery is found or the courier details cannot be retrieved, the method
   /// exits without further action.</remarks>
   /// <param name="order">The order containing delivery details to evaluate for cancellation notification.</param>
    private void NotifyCourierOrderCancelled(BO.Order order)
    {
        var activeDelivery = order.Deliveries?.FirstOrDefault(d => d.DeliveryEndType == null);
        if (activeDelivery?.CourierId is not int courierId)
            return;

        var courier = s_bl.Courier.GetCourierDetails(courierId);
        if (courier == null)
            return;
    }
}