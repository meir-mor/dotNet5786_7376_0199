using PL.Order;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace WpfApp1;

/// <summary>
/// Main administration window used as temporary management entry point for a manager user.
/// The window binds to its own dependency properties so controls are data-bound instead of named.
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// Static readonly BL facade used to access logical services (Courier, Order, Admin).
    /// Initialized via the BL factory to obtain the concrete BL implementation.
    /// </summary>
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// Calls <see cref="InitializeComponent"/> and prepares the window for use.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
    }

    #region Dependency Properties

    /// <summary>
    /// Gets or sets the current business clock displayed on the UI.
    /// Bound to the clock Label in XAML via DataContext binding.
    /// </summary>
    public DateTime CurrentTime
    {
        get => (DateTime)GetValue(CurrentTimeProperty);
        set => SetValue(CurrentTimeProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="CurrentTime"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty CurrentTimeProperty =
        DependencyProperty.Register(nameof(CurrentTime), typeof(DateTime), typeof(MainWindow));

    /// <summary>
    /// Gets or sets the configuration object exposed to the UI.
    /// The object maps to <see cref="BO.Config"/> and is updated/loaded via <see cref="s_bl.Admin"/>.
    /// </summary>
    public BO.Config Configuration
    {
        get => (BO.Config)GetValue(ConfigurationProperty);
        set => SetValue(ConfigurationProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="Configuration"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ConfigurationProperty =
        DependencyProperty.Register(nameof(Configuration), typeof(BO.Config), typeof(MainWindow));

    #endregion

    #region Observers

    /// <summary>
    /// Observer method invoked by BL when the clock updates.
    /// This method marshals the update to the UI thread and assigns the new clock value to <see cref="CurrentTime"/>.
    /// </summary>
    private void ClockObserver()
    {
        // BL might call observers on a background thread — marshal to UI thread.
        Dispatcher.Invoke(() =>
        {
            try
            {
                CurrentTime = s_bl.Admin.GetClock();
            }
            catch (Exception)
            {
                // Swallow exceptions here; keep UI stable.
            }
        });
    }

    /// <summary>
    /// Observer method invoked by BL when configuration values change.
    /// Marshals to the UI thread and assigns the new configuration instance to <see cref="Configuration"/>.
    /// </summary>
    private void ConfigObserver()
    {
        Dispatcher.Invoke(() =>
        {
            try
            {
                Configuration = s_bl.Admin.GetConfig();
            }
            catch (Exception)
            {
                // Ignore observer errors to avoid UI disruption.
            }
        });
    }

    #endregion

    #region Window events

    /// <summary>
    /// Handles the window Loaded event.
    /// Loads initial values from BL into dependency properties and registers observers with BL Admin.
    /// </summary>
    /// <param name="sender">Event source (Window).</param>
    /// <param name="e">Event arguments.</param>
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            // Initialize UI values from BL
            CurrentTime = s_bl.Admin.GetClock();
            Configuration = s_bl.Admin.GetConfig();

            // Register observers
            s_bl.Admin.AddClockObserver(ClockObserver);
            s_bl.Admin.AddConfigObserver(ConfigObserver);
        }
        catch (BO.BlException ex)
        {
            MessageBox.Show($"BL Exception: {ex.GetType().Name} - {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error during load: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Handles the window Closed event.
    /// Unregisters previously registered observers from BL to avoid further callbacks after window closes.
    /// </summary>
    /// <param name="sender">Event source (Window).</param>
    /// <param name="e">Event arguments.</param>
    private void Window_Closed(object? sender, EventArgs e)
    {
        try
        {
            // Unregister observers
            s_bl.Admin.RemoveClockObserver(ClockObserver);
            s_bl.Admin.RemoveConfigObserver(ConfigObserver);
        }
        catch
        {
            // Ignore errors during shutdown/unregistration.
        }
    }

    #endregion

    #region Clock advance button handlers

    /// <summary>
    /// Advances the BL clock by one minute using <see cref="IAdmin.ForwardClock"/>.
    /// </summary>
    private void BtnAdvanceMinute_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            s_bl.Admin.ForwardClock(BO.TimeUnit.Minute);
        }
        catch (BO.BlException ex)
        {
            MessageBox.Show($"{ex.GetType().Name}: {ex.Message}");
        }
    }

    /// <summary>
    /// Advances the BL clock by one hour using <see cref="IAdmin.ForwardClock"/>.
    /// </summary>
    private void BtnAdvanceHour_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            s_bl.Admin.ForwardClock(BO.TimeUnit.Hour);
        }
        catch (BO.BlException ex)
        {
            MessageBox.Show($"{ex.GetType().Name}: {ex.Message}");
        }
    }

    /// <summary>
    /// Advances the BL clock by one day using <see cref="IAdmin.ForwardClock"/>.
    /// </summary>
    private void BtnAdvanceDay_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            s_bl.Admin.ForwardClock(BO.TimeUnit.Day);
        }
        catch (BO.BlException ex)
        {
            MessageBox.Show($"{ex.GetType().Name}: {ex.Message}");
        }
    }

    /// <summary>
    /// Advances the BL clock by one month using <see cref="IAdmin.ForwardClock"/>.
    /// </summary>
    private void BtnAdvanceMonth_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            s_bl.Admin.ForwardClock(BO.TimeUnit.Month);
        }
        catch (BO.BlException ex)
        {
            MessageBox.Show($"{ex.GetType().Name}: {ex.Message}");
        }
    }

    /// <summary>
    /// Advances the BL clock by one year using <see cref="IAdmin.ForwardClock"/>.
    /// </summary>
    private void BtnAdvanceYear_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            s_bl.Admin.ForwardClock(BO.TimeUnit.Year);
        }
        catch (BO.BlException ex)
        {
            MessageBox.Show($"{ex.GetType().Name}: {ex.Message}");
        }
    }

    #endregion

    #region Configuration actions

    /// <summary>
    /// Pushes the current <see cref="Configuration"/> dependency property to BL via <see cref="IAdmin.SetConfig"/>.
    /// Displays a message on success or error.
    /// </summary>
    /// <param name="sender">Event source (Button).</param>
    /// <param name="e">Event arguments.</param>
    private void BtnUpdateConfig_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Push the current Configuration DP to BL
            if (Configuration is null)
            {
                MessageBox.Show("Configuration is not available.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            s_bl.Admin.SetConfig(Configuration);
            MessageBox.Show("Configuration updated.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (BO.BlException ex)
        {
            MessageBox.Show($"BL Exception: {ex.GetType().Name} - {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error updating configuration: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Attempts to open a list-view window (example: CourierListWindow) using reflection.
    /// Uses <see cref="Window.Show"/> so the main window remains accessible.
    /// </summary>
    /// <param name="sender">Event source (Button).</param>
    /// <param name="e">Event arguments.</param>
    private void BtnOpenListView_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Open a list window. Replace CourierListWindow with your actual list-window if exists.
            // Using Show() to keep main window accessible.
            var wnd = new PL.Courier.CourierListWindow();
            wnd.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening list view: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion

    #region DB actions (init / reset)

    /// <summary>
    /// Initializes the database by calling <see cref="IAdmin.InitializeDB"/> on a background task.
    /// Prompts the user for confirmation, closes other windows, sets the mouse cursor to Wait during the operation,
    /// and refreshes local configuration and clock after completion.
    /// </summary>
    /// <param name="sender">Event source (Button).</param>
    /// <param name="e">Event arguments.</param>
    private void BtnInitializeDb_Click(object sender, RoutedEventArgs e)
    {
        var res = MessageBox.Show("Initialize database with sample data? This will overwrite existing data.", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (res != MessageBoxResult.Yes) return;

        try
        {
            Mouse.OverrideCursor = Cursors.Wait;
            // Close other windows except this main window
            CloseOtherWindows();
            
            Task.Run(() => s_bl.Admin.InitializeDB());
            // Refresh local view
            Configuration = s_bl.Admin.GetConfig();
            CurrentTime = s_bl.Admin.GetClock();

            MessageBox.Show("Database initialized.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (BO.BlException ex)
        {
            MessageBox.Show($"BL Exception: {ex.GetType().Name} - {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error initializing DB: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            Mouse.OverrideCursor = null;
        }
    }

    /// <summary>
    /// Resets (clears) the database by calling <see cref="IAdmin.ResetDB"/> on a background task.
    /// Prompts the user for confirmation, closes other windows, sets the mouse cursor to Wait during the operation,
    /// and refreshes local configuration and clock after completion.
    /// </summary>
    /// <param name="sender">Event source (Button).</param>
    /// <param name="e">Event arguments.</param>
    private async void BtnResetDb_Click(object sender, RoutedEventArgs e)
    {
        var res = MessageBox.Show("Reset (clear) database? This operation cannot be undone.", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (res != MessageBoxResult.Yes) return;

        try
        {
            Mouse.OverrideCursor = Cursors.Wait;
            CloseOtherWindows();

            await Task.Run(() => s_bl.Admin.ResetDB());
            Configuration = s_bl.Admin.GetConfig();
            CurrentTime = s_bl.Admin.GetClock();

            MessageBox.Show("Database reset.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (BO.BlException ex)
        {
            MessageBox.Show($"BL Exception: {ex.GetType().Name} - {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error resetting DB: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            Mouse.OverrideCursor = null;
        }
    }

    /// <summary>
    /// Closes all open application windows except this main window.
    /// Used prior to database initialize/reset operations to ensure a consistent UI state.
    /// </summary>
    private void CloseOtherWindows()
    {
        var windows = Application.Current.Windows.Cast<Window>().ToList();
        foreach (var w in windows)
        {
            if (w != this)
            {
                try { w.Close(); }
                catch { /* ignore errors closing other windows */ }
            }
        }
    }

    #endregion

    private void BtnOpenOrderInList_Click(object sender, RoutedEventArgs e)
    {
        var wnd = new OrderListWindow();
        wnd.Show();
    }
}