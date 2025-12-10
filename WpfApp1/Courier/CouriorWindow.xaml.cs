using System;
using System.Text.RegularExpressions;
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
        private static readonly Regex s_digitsOnly = new(@"^\d+$");

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

        /// <summary>
        /// Controls whether Id textbox is read-only. True when updating existing courier.
        /// </summary>
        public bool IdIsReadOnly
        {
            get => (bool)GetValue(IdIsReadOnlyProperty);
            set => SetValue(IdIsReadOnlyProperty, value);
        }
        public static readonly DependencyProperty IdIsReadOnlyProperty =
            DependencyProperty.Register(nameof(IdIsReadOnly), typeof(bool), typeof(CourierWindow), new PropertyMetadata(true));

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

            // Id editable only in Add mode
            IdIsReadOnly = id != 0;

            InitializeComponent();

            // load or create current courier
            if (id == 0)
            {
                // creation mode: allow user to enter Id and set initial metrics
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
                    if (CurrentCourier.Id == 0)
                    {
                        MessageBox.Show("Please enter a valid ID for the new courier.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // check uniqueness using manager id
                    try
                    {
                        var managerId = s_bl.Admin.GetConfig().ManagerId;
                        var exists = s_bl.Courier.GetCouriersList(managerId).Any(c => c.Id == CurrentCourier.Id);
                        if (exists)
                        {
                            MessageBox.Show("A courier with this ID already exists. Choose a different ID.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        s_bl.Courier.AddCourier(CurrentCourier);
                        MessageBox.Show("Courier added.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        Close();
                    }
                    catch (BO.BlException ex)
                    {
                        MessageBox.Show($"Add failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
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

        /// <summary>
        /// Delete button - only enabled/visible in update mode (converter handles visibility).
        /// </summary>
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentCourier == null || CurrentCourier.Id == 0) return;

            var confirm = MessageBox.Show("Are you sure you want to delete this courier?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                var managerId = s_bl.Admin.GetConfig().ManagerId;
                s_bl.Courier.DeleteCourier(managerId, CurrentCourier.Id);
                MessageBox.Show("Courier deleted.", "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            catch (BO.BlException ex)
            {
                MessageBox.Show($"Delete failed: {ex.Message}", "Delete Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception)
            {
                MessageBox.Show("Unexpected error while deleting courier. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Id_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // allow only digits
            e.Handled = !s_digitsOnly.IsMatch(e.Text);
        }
    }
}