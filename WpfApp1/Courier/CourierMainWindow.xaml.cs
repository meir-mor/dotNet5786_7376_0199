using System;
using System.Windows;
using System.Windows.Threading;

namespace PL.Courier;

public partial class CourierMainWindow : Window
{
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
    private readonly int _courierId;
    private Action? _observer;

    public BO.Courier? CurrentCourier
    {
        get => (BO.Courier?)GetValue(CurrentCourierProperty);
        set => SetValue(CurrentCourierProperty, value);
    }
    public static readonly DependencyProperty CurrentCourierProperty =
        DependencyProperty.Register(nameof(CurrentCourier), typeof(BO.Courier), typeof(CourierMainWindow), new PropertyMetadata(null));

    public CourierMainWindow(int courierId)
    {
        _courierId = courierId;
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            LoadCourier();

            // register observer for this courier so UI updates when BL notifies
            _observer = () =>
            {
                Dispatcher.Invoke(() =>
                {
                    try { LoadCourier(); } catch { /* ignore */ }
                });
            };

            try { s_bl.Courier.AddObserver(_courierId, _observer); } catch { }
            try { s_bl.Order.AddObserver(() => LoadCourier()); } catch { }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to initialize courier view: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
        }
    }

    public void LoadCourier()
    {
        CurrentCourier = s_bl.Courier.GetCourierDetails(_courierId);
    }

    private void Window_Closed(object? sender, EventArgs e)
    {
        try
        {
            if (_observer != null) s_bl.Courier.RemoveObserver(_courierId, _observer);
        }
        catch { }
    }

    private void BtnUpdate_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // simple client-side rule: MaxDistance cannot exceed company MaxDistance (if company has one)
            var companyMax = s_bl.Admin.GetConfig().MaxDistance;
            if (CurrentCourier?.MaxDistance != null && companyMax != null && CurrentCourier.MaxDistance > companyMax)
            {
                MessageBox.Show("Max distance cannot exceed company maximum.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // requesterId should be courier id for self-updates
            s_bl.Courier.UpdateCourier(_courierId, CurrentCourier!);
            MessageBox.Show("Courier updated.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (BO.BlException ex)
        {
            MessageBox.Show($"Update failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Unexpected error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnPickOrder_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (CurrentCourier == null || !CurrentCourier.IsActive)
            {
                MessageBox.Show("You must be an active courier to pick orders.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var wnd = new OpenOrdersWindow(_courierId);
            wnd.Owner = this;
            wnd.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Cannot open orders list: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnFinishDelivery_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var inProg = CurrentCourier?.OrderInProgress;
            if (inProg == null)
            {
                MessageBox.Show("No order in progress.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Ask user for end type (simple choice)
            var res = MessageBox.Show("Mark delivery as supplied (Yes) or cancelled (No)?\nYes = Supplied, No = Cancelled", "Finish Delivery", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            if (res == MessageBoxResult.Cancel) return;

            if (res == MessageBoxResult.Yes)
            {
                s_bl.Order.CompleteDelivery(_courierId, _courierId, inProg.DeliveryId);
                MessageBox.Show("Delivery marked as supplied.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // For simplicity use Cancelled end type by calling CompleteDelivery (BL handles mapping) - if BL has a separate method use it
                s_bl.Order.CompleteDelivery(_courierId, _courierId, inProg.DeliveryId);
                MessageBox.Show("Delivery marked as completed (non-supplied).", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            // refresh
            LoadCourier();
        }
        catch (BO.BlException ex)
        {
            MessageBox.Show($"Finish failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Unexpected error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnHistory_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var wnd = new CourierHistoryWindow(_courierId);
            wnd.Owner = this;
            wnd.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Cannot open history: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
}