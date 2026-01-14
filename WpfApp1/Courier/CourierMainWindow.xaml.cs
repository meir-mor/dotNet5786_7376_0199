using System;
using System.Windows;
using BO; // Assuming the BL uses this namespace
using PL.Helpers;

namespace PL.Courier
{
    public partial class CourierMainWindow : Window
    {
        // Access to the BL layer
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
        private readonly int _courierId;
        private readonly UserRole _userRole;

        // Variable to save the Observer for automatic update
        private Action? _observer;

        // Stage 7: Mutex for thread-safe observer updates
        private readonly ObserverMutex _courierMutex = new();

        // Auxiliary variable to track the original shipment type (to prevent change during an order)
        private BO.ShipmentType? _originalType;

        // Dependency Property for Data Binding in XAML
        public BO.Courier? CurrentCourier
        {
            get => (BO.Courier?)GetValue(CurrentCourierProperty);
            set => SetValue(CurrentCourierProperty, value);
        }

        public static readonly DependencyProperty CurrentCourierProperty =
            DependencyProperty.Register(nameof(CurrentCourier), typeof(BO.Courier), typeof(CourierMainWindow));

        public CourierMainWindow(int courierId, UserRole userRole)
        {
            _courierId = courierId;
            _userRole = userRole;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadCourier();

                // Setting Observer to refresh the screen when data changes
                _observer = CourierObserver;
                s_bl.Courier.AddObserver(_courierId, _observer);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading courier: {ex.Message}");
                Close();
            }
        }

        /// <summary>
        /// Stage 7: Observer method with mutex pattern for thread-safe UI updates
        /// </summary>
        private void CourierObserver()
        {
            #region Stage 7 (for multithreading)
            if (_courierMutex.CheckAndSetLoadInProgressOrRestartRequired())
                return;

            _ = Dispatcher.BeginInvoke(async () =>
            {
                // Loading current courier details from BL
                CurrentCourier = s_bl.Courier.GetCourierDetails(_courierId);
                // Saving current shipment type for validation checks during update
                _originalType = CurrentCourier?.ShipmentType;

                if (await _courierMutex.UnsetLoadInProgressAndCheckRestartRequested())
                    CourierObserver();
            });
            #endregion Stage 7 (for multithreading)
        }

        public void LoadCourier()
        {
            // Loading current courier details from BL
            CurrentCourier = s_bl.Courier.GetCourierDetails(_courierId);
            // Saving current shipment type for validation checks during update
            _originalType = CurrentCourier?.ShipmentType;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // Removing the Observer on exit to prevent errors and memory leaks
            if (_observer != null)
                s_bl.Courier.RemoveObserver(_courierId, _observer);
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentCourier == null) return;

                // Performing the update
                s_bl.Courier.UpdateCourier(_courierId, CurrentCourier);
                MessageBox.Show("Courier details updated successfully.", "Success", MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Refreshing data
                LoadCourier();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Update failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnPickOrder_Click(object sender, RoutedEventArgs e)
        {
            // Opening order selection screen (the button will be active only if XAML conditions are met)
            new OpenOrdersWindow(_courierId) { Owner = this }.Show();
        }

        private void BtnFinishDelivery_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentCourier?.OrderInProgress == null) return;

            // Reporting completion (simulation of selecting completion type)
            var res = MessageBox.Show("Has the delivery been completed successfully?", "Finish Delivery",
                MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            if (res == MessageBoxResult.Cancel) return;

            try
            {
                // Here the BL call to finish handling will be performed
                // Assume Yes indicates success. (Must match your BL method if it receives status)
                s_bl.Order.CompleteDelivery(_courierId, _courierId, CurrentCourier.OrderInProgress.DeliveryId);

                MessageBox.Show("Delivery completed.", "Success");
                LoadCourier();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        public bool IsManager => _userRole == UserRole.Manager;
        public bool IsCourier => _userRole == UserRole.Courier;

        private void BtnHistory_Click(object sender, RoutedEventArgs e)
        {
            new CourierHistoryWindow(_courierId) { Owner = this }.Show();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete this courier?", "Delete Courier",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    s_bl.Courier.DeleteCourier(s_bl.Admin.GetConfig().ManagerId, _courierId);
                    MessageBox.Show("Courier deleted successfully.", "Success");
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting courier: {ex.Message}", "Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
    }
}