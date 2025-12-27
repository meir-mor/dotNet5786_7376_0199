using System.Windows;
using BO;

namespace PL;

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
                var wnd = new MainWindow();
                wnd.Show();
                return;
            }

            if (role == BO.UserRole.Courier)
            {
                if (!int.TryParse(username, out var courierId))
                {
                    MessageBox.Show("Courier login requires numeric username (Tz). Please enter your Tz as username.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    var wnd = new PL.Courier.CourierMainWindow(courierId, UserRole.Courier);
                    wnd.Show();
                    return;
                }
                catch (BO.BlException ex)
                {
                    MessageBox.Show($"Cannot open courier view: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            MessageBox.Show("Login succeeded but role is not supported by this UI.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (BO.BlException ex)
        {
            MessageBox.Show($"Login failed: {ex.Message}", "Login Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            MessageBox.Show("An unexpected error occurred during login. Please try again or contact support.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}