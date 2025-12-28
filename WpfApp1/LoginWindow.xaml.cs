using System.Windows;
using BO;

namespace PL;

/// <summary>
/// Simple login window with role-based navigation and "one manager" concurrency rule.
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

    private void ShowError(string message)
    {
        TxtError.Text = message;
        TxtError.Visibility = Visibility.Visible;
    }

    private void BtnLogin_Click(object sender, RoutedEventArgs e)
    {
        var username = TxtUsername.Text?.Trim() ?? string.Empty;
        var password = PwdPassword.Password ?? string.Empty;

        if (string.IsNullOrWhiteSpace(username))
        {
            ShowError("Please enter username.");
            return;
        }

        try
        {
            TxtError.Visibility = Visibility.Collapsed;
            var role = s_bl.Courier.Login(username, password);

            if (role == UserRole.Manager)
            {
                OpenManagerWindow();
            }
            else if (role == UserRole.Courier)
            {
                if (int.TryParse(username, out var id))
                {
                    OpenCourierWindow(id, role);
                }
                else
                {
                    ShowError("Courier login requires numeric ID.");
                }
            }
        }
        catch (BlException ex)
        {
            ShowError(ex.Message);
        }
        catch (Exception)
        {
            ShowError("An unexpected error occurred.");
        }
    }

    private void OpenManagerWindow()
    {
        // Enforce one manager window at a time
        var existing = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
        if (existing != null)
        {
            existing.Activate();
            if (existing.WindowState == WindowState.Minimized)
                existing.WindowState = WindowState.Normal;
            
            MessageBox.Show("Administrator is already logged in.", "Notice", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        new MainWindow().Show();
    }

    private void OpenCourierWindow(int id, UserRole role)
    {
        try
        {
            var wnd = new PL.Courier.CourierMainWindow(id, role);
            wnd.Show();
        }
        catch (BlException ex)
        {
            ShowError($"Could not open courier view: {ex.Message}");
        }
    }
}
