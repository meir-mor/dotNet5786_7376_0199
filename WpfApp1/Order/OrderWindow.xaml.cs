using BO;
using DO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PL.Order
{
    public partial class OrderWindow : Window
    {
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        private volatile DispatcherOperation? _observerOperation = null;

        /// <summary>
        /// Current order displayed in the window
        /// </summary>
        public BO.Order CurrentOrder
        {
            get => (BO.Order)GetValue(CurrentOrderProperty);
            set => SetValue(CurrentOrderProperty, value);
        }

        public static readonly DependencyProperty CurrentOrderProperty =
            DependencyProperty.Register(
                nameof(CurrentOrder),
                typeof(BO.Order),
                typeof(OrderWindow),
                new PropertyMetadata(null)
            );

        private readonly int _managerId;
        private readonly bool _isAddMode;

        /// <summary>
        /// Create new order
        /// </summary>
        public OrderWindow()
        {
            _managerId = s_bl.Admin.GetConfig().ManagerId;
            _isAddMode = true;

            InitializeComponent();

            CurrentOrder = new BO.Order
            {
                OrderOpenTime = DateTime.Now,
                OrderStatus = OrderStatus.Open,
                Deliveries = new System.Collections.Generic.List<DeliveryPerOrderInList>()
            };
        }

        /// <summary>
        /// Open existing order
        /// </summary>
        public OrderWindow(int orderId)
        {
            _managerId = s_bl.Admin.GetConfig().ManagerId;
            _isAddMode = false;

            InitializeComponent();

            try
            {
                CurrentOrder = s_bl.Order.GetOrderDetails(_managerId, orderId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load order: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void OrderObserver()
        {
            if (_observerOperation is null ||
                _observerOperation.Status == DispatcherOperationStatus.Completed)
            {
                _observerOperation = Dispatcher.BeginInvoke(() =>
                {
                    try
                    {
                        var updated = s_bl.Order.GetOrderDetails(_managerId, CurrentOrder.Id);
                        if (updated == null)
                        {
                            MessageBox.Show("Order was removed.", "Info");
                            Close();
                        }
                        else
                        {
                            CurrentOrder = updated;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Observer Error");
                    }
                });
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isAddMode)
            {
                s_bl.Order.AddObserver(OrderObserver);
                s_bl.Admin.AddClockObserver(OrderObserver);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (!_isAddMode)
            {
                s_bl.Order.RemoveObserver(OrderObserver);
                s_bl.Admin.RemoveClockObserver(OrderObserver);
            }
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                s_bl.Order.UpdateOrder(_managerId, CurrentOrder);
                MessageBox.Show("Order updated successfully", "Success");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Update Error");
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                s_bl.Order.CancelOrder(_managerId, CurrentOrder.Id);
                MessageBox.Show("Order cancelled", "Success");
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Cancel Error");
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}