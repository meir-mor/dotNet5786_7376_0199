using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Reflection;
using Microsoft.Win32;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Windows.Shapes;
using System.Windows.Media;

namespace PL.Courier;

public partial class OpenOrdersWindow : Window
{
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
    private readonly int _courierId;
    
    // Observers
    private Action _listObserver;

    public IEnumerable<BO.OpenOrderInList>? OpenOrders
    {
        get => (IEnumerable<BO.OpenOrderInList>?)GetValue(OpenOrdersProperty);
        set => SetValue(OpenOrdersProperty, value);
    }
    public static readonly DependencyProperty OpenOrdersProperty =
        DependencyProperty.Register(nameof(OpenOrders), typeof(IEnumerable<BO.OpenOrderInList>), typeof(OpenOrdersWindow), new PropertyMetadata(null));

    public OpenOrdersWindow(int courierId)
    {
        SetBrowserFeatureControl();
        _courierId = courierId;
        InitializeComponent();
        
        // Define observer action (thread-safe dispatch)
        _listObserver = () => Dispatcher.Invoke(LoadOpenOrders);
        
        // Handle closing to unsubscribe
        Closing += Window_Closing;
    }

    private void SetBrowserFeatureControl()
    {
        try
        {
            var fileName = System.IO.Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
            var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", true);
            if (key != null)
            {
                // 11001 (0x2AF9) = Internet Explorer 11. Webpages are displayed in IE11 Standards mode, regardingles of !DOCTYPE directive.
                key.SetValue(fileName, 11001, RegistryValueKind.DWord);
                key.Close();
            }
        }
        catch { }
    }

    private BO.ShipmentType _courierShipmentType;
    private double _companyLat, _companyLon;

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try 
        {
            var courier = s_bl.Courier.GetCourierDetails(_courierId);
            _courierShipmentType = courier.ShipmentType;
        }
        catch { _courierShipmentType = BO.ShipmentType.Motorcycle; } 

        var types = Enum.GetValues(typeof(BO.OrderType)).Cast<BO.OrderType>().ToList();
        CbFilterType.ItemsSource = new List<object> { "All" }.Concat(types.Cast<object>());
        CbFilterType.SelectedIndex = 0;

        CbSortBy.ItemsSource = Enum.GetValues(typeof(BO.OpenOrderSortBy));
        CbSortBy.SelectedItem = BO.OpenOrderSortBy.ScheduleStatus;

        // Initial Load
        LoadOpenOrders();

        // Register Observer
        try
        {
            s_bl.Order.AddObserver(_listObserver);
        }
        catch (Exception ex) 
        { 
            // Logging or silent fail if observer not supported yet 
        }
    }

    private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        try
        {
            s_bl.Order.RemoveObserver(_listObserver);
        }
        catch { }
    }

    private void LoadOpenOrders()
    {
        try
        {
            // Get Filter
            BO.OrderType? typeFilter = null;
            if (CbFilterType.SelectedItem is BO.OrderType t) typeFilter = t;

            // Get Sort
            BO.OpenOrderSortBy? sortBy = null;
            if (CbSortBy.SelectedItem is BO.OpenOrderSortBy s) sortBy = s;

            // Fetch
            OpenOrders = s_bl.Order.GetOpenOrdersForCourier(_courierId, _courierId, typeFilter, sortBy);
        }
        catch (Exception ex)
        {
            // If window is closing, we might get exceptions on Dispatcher, can ignore or log
        }
    }

    private void CbFilterType_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        LoadOpenOrders();
    }

    private void CbSortBy_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        LoadOpenOrders();
    }

    private void BtnRefresh_Click(object sender, RoutedEventArgs e)
    {
        LoadOpenOrders();
    }

    private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is DataGrid dg && dg.SelectedItem is BO.OpenOrderInList order)
        {
            UpdateMap(order);
        }
    }

    private void CnvMap_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (_selectedForMap != null) UpdateMap(_selectedForMap);
    }

    private BO.OpenOrderInList? _selectedForMap;

    private void UpdateMap(BO.OpenOrderInList order)
    {
        _selectedForMap = order;

        CnvMap.Children.Clear();
        TxtMapPlaceholder.Visibility = Visibility.Collapsed; // Remove placeholder

        var w = CnvMap.ActualWidth;
        var h = CnvMap.ActualHeight;

        if (w == 0 || h == 0) return;

        // Coordinates
        var cLat = s_bl.Admin.GetConfig().Latitude ?? 0;
        var cLon = s_bl.Admin.GetConfig().Longitude ?? 0;
        var oLat = order.Latitude ?? 0;
        var oLon = order.Longitude ?? 0;
        
        const double minLat = 29.3; 
        const double maxLat = 33.5;
        const double minLon = 34.0; 
        const double maxLon = 35.9;

        const double latSpan = maxLat - minLat;
        const double lonSpan = maxLon - minLon;

        // Draw Connector Line
        var line = new Line
        {
            X1 = GetX(cLon),
            Y1 = GetY(cLat),
            X2 = GetX(oLon),
            Y2 = GetY(oLat),
            Stroke = Brushes.Black,
            StrokeThickness = 2,
            StrokeDashArray = [4, 1]
        };
        CnvMap.Children.Add(line);

        // Draw Company (Blue square)
        var companyShape = new Rectangle { Width = 12, Height = 12, Fill = Brushes.Blue, Stroke=Brushes.White, StrokeThickness=1, ToolTip = "Company" };
        Canvas.SetLeft(companyShape, GetX(cLon) - 6);
        Canvas.SetTop(companyShape, GetY(cLat) - 6);
        CnvMap.Children.Add(companyShape);

        // Draw Order (Red circle)
        var orderShape = new Ellipse { Width = 12, Height = 12, Fill = Brushes.Red, Stroke = Brushes.White, StrokeThickness = 1, ToolTip = order.FullAddress };
        Canvas.SetLeft(orderShape, GetX(oLon) - 6);
        Canvas.SetTop(orderShape, GetY(oLat) - 6);
        CnvMap.Children.Add(orderShape);
        return;

        double GetY(double lat) => (maxLat - lat) / latSpan * h;
        double GetX(double lon) => (lon - minLon) / lonSpan * w;
    }

    private void BtnPick_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not Button btn || btn.CommandParameter is not int orderId) return;

            var confirm = MessageBox.Show("Pick this order for delivery?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            var order = OpenOrders?.FirstOrDefault(o => o.OrderId == orderId);
            s_bl.Order.AssignOrderToCourier(_courierId, _courierId, orderId);

            MessageBox.Show("Order assigned to you.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);

            // Send Email
            if (order != null) 
            {
                SendEmail(order);
            }

            // refresh lists (observer will likely also trigger, but explicit refresh ensures immediate feedback)
            LoadOpenOrders();
            
            // notify owner to refresh (courier main window)
            if (Owner is CourierMainWindow c) c.LoadCourier();
        }
        catch (BO.BlException ex)
        {
            MessageBox.Show($"Assign failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Unexpected error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private void SendEmail(BO.OpenOrderInList order)
    {
        try
        {
            string toEmail = "arioren100@gmail.com";   // אליך

            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("arioren100@gmail.com");
            mail.To.Add(toEmail);
            mail.Subject = $"New Order Assigned #{order.OrderId}";
            mail.Body = $@"
Hello Aharon,

You have successfully assigned Order #{order.OrderId}.

Details:
Type: {order.OrderType}
Address: {order.FullAddress}
Max Delivery Time: {order.MaxDeliveryTime}

Safe Travels!
";

            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
            smtp.EnableSsl = true;

            smtp.Credentials = new NetworkCredential(
                "arioren100@gmail.com",
                "xxxxxxxxx"
            );

            smtp.Send(mail);  

            MessageBox.Show("Email sent successfully!", "Email", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Email failed: " + ex.Message);
        }
    }
}