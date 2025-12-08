using System;
using System.Windows;
using BO;

namespace PL.Courier
{
    /// <summary>
    /// Window to display and edit a single courier (add or update).
    /// Uses full DataBinding: the window DataContext is itself and the UI binds to the dependency property CurrentCourier.
    /// </summary>
    public partial class CourierWindow : Window
    {
        // BL facade
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        /// <summary>
        /// Dependency property that holds the current courier instance shown in the UI.
        /// Bind UI controls to members of this BO.Courier.
        /// </summary>
        public BO.Courier? CurrentCourier
        {
            get => (BO.Courier?)GetValue(CurrentCourierProperty);
            set => SetValue(CurrentCourierProperty, value);
        }
        public static readonly DependencyProperty CurrentCourierProperty =
            DependencyProperty.Register(nameof(CurrentCourier), typeof(BO.Courier), typeof(CourierWindow), new PropertyMetadata(null));

        /// <summary>
        /// Text shown on the Add/Update button. Set in the constructor depending on id.
        /// </summary>
        public string ButtonText
        {
            get => (string)GetValue(ButtonTextProperty);
            set => SetValue(ButtonTextProperty, value);
        }
        public static readonly DependencyProperty ButtonTextProperty =
            DependencyProperty.Register(nameof(ButtonText), typeof(string), typeof(CourierWindow), new PropertyMetadata("Add"));

        // Observer callback for this courier id
        private Action? _courierObserver;

        /// <summary>
        /// Ctor. If id == 0 -> Add mode; otherwise Update mode and load courier from BL.
        /// ButtonText is set according to mode before InitializeComponent to reflect in UI.
        /// </summary>
        /// <param name="id">Courier id to edit; 0 for create</param>
        public CourierWindow(int id = 0)
        {
            // set button label according to mode
            ButtonText = id == 0 ? "Add" : "Update";

            InitializeComponent();

            // load or create current courier
            if (id == 0)
            {
                CurrentCourier = new BO.Courier
                {
                    Id = 0,
                    FullName = string.Empty,
                    PhoneNumber = string.Empty,
                    Email = string.Empty,
                    Password = "Default1A!",
                    IsActive = true,
                    MaxDistance = null,
                    ShipmentType = ShipmentType.Motorcycle,
                    StartWorkTime = s_bl.Admin.GetClock(),
                    TotalDeliveredLate = 0,
                    TotalDeliveredOnTime = 0,
                    OrderInProgress = null
                };
            }
            else
            {
                try
                {
                    CurrentCourier = s_bl.Courier.GetCourierDetails(id);
                }
                catch (BO.BlException ex)
                {
                    MessageBox.Show($"Unable to load courier: {ex.Message}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    CurrentCourier = null;
                }
                catch (Exception)
                {
                    MessageBox.Show("Unexpected error while loading courier.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    CurrentCourier = null;
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Register as observer for this courier if editing existing courier
            if (CurrentCourier?.Id != 0)
            {
                _courierObserver = () =>
                {
                    try
                    {
                        // refresh current courier on UI thread
                        Dispatcher.Invoke(() =>
                        {
                            try
                            {
                                var id = CurrentCourier!.Id;
                                CurrentCourier = s_bl.Courier.GetCourierDetails(id);
                            }
                            catch { /* ignore reload errors */ }
                        });
                    }
                    catch { }
                };

                try { s_bl.Courier.AddObserver(CurrentCourier!.Id, _courierObserver); } catch { }
            }
        }

        private void Window_Closed(object? sender, EventArgs e)
        {
            // unregister observer
            if (_courierObserver != null && CurrentCourier?.Id != 0)
            {
                try { s_bl.Courier.RemoveObserver(CurrentCourier.Id, _courierObserver); } catch { }
            }
        }

        /// <summary>
        /// Click handler for Add / Update button.
        /// Uses manager id as requester for update (manager permissions).
        /// </summary>
        private void BtnAddUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentCourier == null)
            {
                MessageBox.Show("No courier data to save.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (ButtonText == "Add")
                {
                    s_bl.Courier.AddCourier(CurrentCourier);
                    MessageBox.Show("Courier added.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    Close();
                }
                else // Update
                {
                    var managerId = s_bl.Admin.GetConfig().ManagerId;
                    s_bl.Courier.UpdateCourier(managerId, CurrentCourier);
                    MessageBox.Show("Courier updated.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    Close();
                }
            }
            catch (BO.BlException ex)
            {
                MessageBox.Show($"Operation failed: {ex.Message}", "Operation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception)
            {
                MessageBox.Show("Unexpected error during save. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {

        }
    }
}