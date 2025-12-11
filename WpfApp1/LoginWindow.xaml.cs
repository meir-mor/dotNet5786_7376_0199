using System.Windows;

namespace WpfApp1;

/// <summary>
/// Simple login window using username/password flow.
/// Manager -> opens MainWindow.
/// Courier -> opens CourierWindow with courier id parsed from username (username expected to be Tz integer).
/// All errors shown with friendly MessageBox messages per PL rules.
/// </summary>
public partial class LoginWindow : Window
{
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    public LoginWindow()
    {
        InitializeComponent();
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void BtnLogin_Click(object sender, RoutedEventArgs e)
    {
        var username = TxtUsername.Text?.Trim() ?? string.Empty;
        var password = PwdPassword.Password ?? string.Empty;

        if (string.IsNullOrWhiteSpace(username))
        {
            MessageBox.Show("Please enter username.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            // Attempt authentication via BL
            var role = s_bl.Courier.Login(username, password);

            if (role == BO.UserRole.Manager)
            {
                // Manager: open admin main window
                var wnd = new MainWindow();
                wnd.Show();
                Close();
                return;
            }

            // Courier: username is expected to contain courier id (Tz). parse it.
            if (role == BO.UserRole.Courier)
            {
                if (!int.TryParse(username, out var courierId))
                {
                    MessageBox.Show("Courier login requires numeric username (Tz). Please enter your Tz as username.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    // Validate courier exists and open courier window
                    var courierDetails = s_bl.Courier.GetCourierDetails(courierId);
                    var wnd = new PL.Courier.CourierWindow(courierId);
                    wnd.Show();
                    Close();
                    return;
                }
                catch (BO.BlException ex)
                {
                    MessageBox.Show($"Cannot open courier view: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            // Unknown role - treat as error
            MessageBox.Show("Login succeeded but role is not supported by this UI.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (BO.BlException ex)
        {
            // Friendly message for BL validation/auth errors
            MessageBox.Show($"Login failed: {ex.Message}", "Login Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show("An unexpected error occurred during login. Please try again or contact support.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}