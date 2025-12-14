using BO;
using DO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PL.Order
{
    public partial class OrderListWindow : Window
    {
        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        public BO.ShipmentType? SelectedShipment { get; set; } = null;

        public IEnumerable<OrderInList> OrderList
        {
            get => (IEnumerable<OrderInList>)GetValue(OrderListProperty);
            set => SetValue(OrderListProperty, value);
        }

        public static readonly DependencyProperty OrderListProperty =
            DependencyProperty.Register(nameof(OrderList), typeof(IEnumerable<OrderInList>), typeof(OrderListWindow), new PropertyMetadata(null));

        public OrderListWindow()
        {
            InitializeComponent();
        }

        private void OrderListObserver()
        {
            UpdateOrderList();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                s_bl.Order.AddObserver(OrderListObserver);
                UpdateOrderList();
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
                s_bl.Order.RemoveObserver(OrderListObserver);
            }
            catch { }
        }

        private void ShipmentFilter_SelectionChangedOrder(object sender, SelectionChangedEventArgs e)
        {
            UpdateOrderList();
        }

        /// <summary>
        /// Updates OrderList using BL, applies ShipmentType filter if selected.
        /// </summary>
        private void UpdateOrderList()
        {
            try
            {
                var managerId = s_bl.Admin.GetConfig().ManagerId;

                OrderList = SelectedShipment == null
                    ? s_bl.Order.GetOrdersList(managerId)
                    : s_bl.Order.GetOrdersList(managerId, 
                    BO.OrderListFilterBy.ShipmentType, SelectedShipment.Value);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating orders: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

                MessageBox.Show("Order deleted.", "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (BO.BlException ex)
            {
                MessageBox.Show($"Delete failed: {ex.Message}", "Delete Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception)
            {
                MessageBox.Show("Unexpected error while deleting order. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}

