using BO;
using DO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using PL.Helpers;

namespace PL.Order
{
    public partial class OrderWindow : Window
    {
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        private readonly int _managerId;

        public BO.Order CurrentOrder
        {
            get => (BO.Order)GetValue(CurrentOrderProperty);
            set => SetValue(CurrentOrderProperty, value);
        }

        public static readonly DependencyProperty CurrentOrderProperty =
            DependencyProperty.Register(nameof(CurrentOrder), typeof(BO.Order), typeof(OrderWindow),
                new PropertyMetadata(null));

        public IEnumerable<BO.OrderType> OrderTypes => Enum.GetValues(typeof(BO.OrderType)).Cast<BO.OrderType>();

        // Stage 7: Mutex for thread-safe observer updates
        private readonly ObserverMutex _orderMutex = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderWindow"/> class with the specified order ID.
        /// </summary>
        /// <remarks>This constructor retrieves the manager ID from the application configuration and uses
        /// it to fetch the details of the specified order. The order details are then set as the data context for the
        /// window.</remarks>
        /// <param name="orderId">The unique identifier of the order to be displayed in the window.</param>
        public OrderWindow(int orderId)
        {
            InitializeComponent();

            _managerId = s_bl.Admin.GetConfig().ManagerId;

            try
            {
                CurrentOrder = s_bl.Order.GetOrderDetails(_managerId, orderId);
                DataContext = this;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load order: {ex.Message}", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Close();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            s_bl.Order.AddObserver(CurrentOrder.Id, RefreshOrderObserver);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            s_bl.Order.RemoveObserver(CurrentOrder.Id, RefreshOrderObserver);
        }

        /// <summary>
        /// Stage 7: Observer method with mutex pattern for thread-safe UI updates
        /// </summary>
        private void RefreshOrderObserver()
        {
            #region Stage 7 (for multithreading)
            if (_orderMutex.CheckAndSetLoadInProgressOrRestartRequired())
                return;

            _ = Dispatcher.BeginInvoke(async () =>
            {
                try
                {
                    CurrentOrder = s_bl.Order.GetOrderDetails(_managerId, CurrentOrder.Id);
                }
                catch
                {
                    MessageBox.Show("Order was removed", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    Close();
                }

                if (await _orderMutex.UnsetLoadInProgressAndCheckRestartRequested())
                    RefreshOrderObserver();
            });
            #endregion Stage 7 (for multithreading)
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                s_bl.Order.UpdateOrder(_managerId, CurrentOrder);
                MessageBox.Show("Order updated successfully", "Success", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                s_bl.Order.CancelOrder(_managerId, CurrentOrder.Id);
                MessageBox.Show("Order cancelled", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Cancel Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}