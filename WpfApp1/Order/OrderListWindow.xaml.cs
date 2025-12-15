using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PL.Order;

public partial class OrderListWindow : Window
{
    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    public BO.ShipmentType? SelectedShipment { get; set; } = null;

    public BO.OrderInList? SelectedOrder { get; set; }


    public IEnumerable<BO.OrderInList> OrderList
    {
        get => (IEnumerable<BO.OrderInList>)GetValue(OrderListProperty);
        set => SetValue(OrderListProperty, value);
    }

    public static readonly DependencyProperty OrderListProperty =
        DependencyProperty.Register(nameof(OrderList),typeof(IEnumerable<BO.OrderInList>),
            typeof(OrderListWindow),new PropertyMetadata(null));

    public OrderListWindow()
    {
        InitializeComponent();
    }

    private void OrderListObserver() => UpdateOrderList();

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            s_bl.Order.AddObserver(OrderListObserver);
            UpdateOrderList();
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
            s_bl.Order.RemoveObserver(OrderListObserver);
        }
        catch { }
    }

    private void ShipmentFilter_SelectionChangedOrder(object sender, SelectionChangedEventArgs e)
    {
        UpdateOrderList();
    }


    private void UpdateOrderList()
    {
        try
        {
            var managerId = s_bl.Admin.GetConfig().ManagerId;

            OrderList = SelectedShipment == null
                ? s_bl.Order.GetOrdersList(managerId)
                : s_bl.Order.GetOrdersList(managerId, BO.OrderListFilterBy.ShipmentType,SelectedShipment.Value);           
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error updating orders: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
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
        new OrderWindow().Show();
    }

    private void DeleteOrder_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not Button btn || btn.CommandParameter is not int orderId)
                return;

            var confirm = MessageBox.Show(
                "Are you sure you want to delete the selected order?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            var managerId = s_bl.Admin.GetConfig().ManagerId;
            s_bl.Order.DeleteOrder(managerId, orderId);

            UpdateOrderList();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message,
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
