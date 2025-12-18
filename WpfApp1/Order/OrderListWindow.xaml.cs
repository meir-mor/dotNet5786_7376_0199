using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Windows.Services.Maps;

namespace PL.Order;

public partial class OrderListWindow : Window
{
    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    public BO.ShipmentType? SelectedShipment { get; set; }

    public List<BO.OrderInList> _allOrders = new();

    public BO.OrderInList? SelectedOrder { get; set; }


    public IEnumerable<BO.OrderInList> OrderList
    {
        get => (IEnumerable<BO.OrderInList>)GetValue(OrderListProperty);
        set => SetValue(OrderListProperty, value);
    }

    public static readonly DependencyProperty OrderListProperty =
        DependencyProperty.Register(nameof(OrderList), typeof(IEnumerable<BO.OrderInList>),
            typeof(OrderListWindow), new PropertyMetadata(null));

    public OrderListWindow()
    {
        InitializeComponent();
        DataContext = this;
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
            MessageBox.Show($"Failed to load orders: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

    private void ShipmentFilter_SelectionChangedOrder(object sender, SelectionChangedEventArgs e)
    {
        ApplyShipmentFilter();
    }

    private void RefreshOrders()
    {
        try
        {
            var managerId = s_bl.Admin.GetConfig().ManagerId;
            _allOrders = s_bl.Order.GetOrdersList(managerId).ToList();
            ApplyShipmentFilter();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load orders: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ApplyShipmentFilter()
    {
        OrderList = SelectedShipment == null
            ? _allOrders
            : _allOrders.Where(o => o.ShipmentType == SelectedShipment.Value);
    }


    private void Order_DoubleClick(object? sender, System.Windows.Input.MouseButtonEventArgs e)
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
            MessageBox.Show("Cannot open Order details", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnAdd_Click(object sender, RoutedEventArgs e)
    {
        new AddOrderWindow().ShowDialog();
        RefreshOrders();
    }

    private void CancelOrder_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not Button btn || btn.CommandParameter is not int orderId)
                return;

            var confirm = MessageBox.Show(
                "Are you sure you want to cancel the selected order?","Confirm Cancel",MessageBoxButton.YesNo,    
                MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            var managerId = s_bl.Admin.GetConfig().ManagerId;
            var order = s_bl.Order.GetOrderDetails(managerId, orderId);

            if (order.OrderStatus == DO.OrderStatus.InProgress)
            {
                NotifyCourierOrderCancelled(order);
            }

            s_bl.Order.CancelOrder(managerId, orderId);
            RefreshOrders();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message,
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        //MailService.SendMail(courier.Email,"Order Cancelled",
           // $"The order {order.Id} assigned to you has been cancelled."
        //);
    }
}
