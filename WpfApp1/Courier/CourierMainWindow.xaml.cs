using System;
using System.Windows;
using BO; // הנחה שה-BL משתמש ב-namespace הזה

namespace PL.Courier
{
    public partial class CourierMainWindow : Window
    {
        // גישה לשכבת ה-BL
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
        private readonly int _courierId;
        
        // משתנה לשמירת ה-Observer לעדכון אוטומטי
        private Action? _observer;
        
        // משתנה עזר למעקב אחר סוג המשלוח המקורי (למניעת שינוי באמצע הזמנה)
        private BO.ShipmentType? _originalType;

        // Dependency Property לקישור הנתונים (Data Binding) ב-XAML
        public BO.Courier? CurrentCourier
        {
            get => (BO.Courier?)GetValue(CurrentCourierProperty);
            set => SetValue(CurrentCourierProperty, value);
        }

        public static readonly DependencyProperty CurrentCourierProperty =
            DependencyProperty.Register(nameof(CurrentCourier), typeof(BO.Courier), typeof(CourierMainWindow));

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
                
                // הגדרת Observer לרענון המסך כשמתבצע שינוי בנתונים
                _observer = () => Dispatcher.Invoke(LoadCourier);
                s_bl.Courier.AddObserver(_courierId, _observer);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading courier: {ex.Message}");
                Close();
            }
        }

        public void LoadCourier()
        {
            // טעינת פרטי השליח העדכניים מה-BL
            CurrentCourier = s_bl.Courier.GetCourierDetails(_courierId);
            // שמירת סוג המשלוח הנוכחי לבדיקות אימות בעדכון
            _originalType = CurrentCourier?.ShipmentType;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // הסרת ה-Observer ביציאה למניעת שגיאות וזליגת זיכרון
            if (_observer != null) 
                s_bl.Courier.RemoveObserver(_courierId, _observer);
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentCourier == null) return;

                // 2. בדיקת שינוי סוג משלוח בזמן הזמנה פעילה
                if (CurrentCourier.OrderInProgress != null && CurrentCourier.ShipmentType != _originalType)
                {
                    MessageBox.Show("Cannot change shipment type while an order is in progress.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    // שחזור הערך המקורי בתצוגה
                    CurrentCourier.ShipmentType = _originalType ?? ShipmentType.Bicycle; 
                    return;
                }

                // ביצוע העדכון
                s_bl.Courier.UpdateCourier(_courierId, CurrentCourier);
                MessageBox.Show("Courier details updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // רענון הנתונים
                LoadCourier();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Update failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnPickOrder_Click(object sender, RoutedEventArgs e)
        {
            // פתיחת מסך בחירת הזמנה (הכפתור יהיה פעיל רק אם התנאים ב-XAML מתקיימים)
            new OpenOrdersWindow(_courierId) { Owner = this }.Show();
        }

        private void BtnFinishDelivery_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentCourier?.OrderInProgress == null) return;

            // דיווח על סיום (סימולציה של בחירת סוג סיום)
            var res = MessageBox.Show("Has the delivery been completed successfully?", "Finish Delivery", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            
            if (res == MessageBoxResult.Cancel) return;

            try
            {
                // כאן תתבצע הקריאה ל-BL לסיום הטיפול
                // נניח ש-Yes מסמן הצלחה. (יש להתאים למתודת ה-BL שלך אם היא מקבלת סטטוס)
                s_bl.Order.CompleteDelivery(_courierId, _courierId, CurrentCourier.OrderInProgress.DeliveryId);
                
                MessageBox.Show("Delivery completed.", "Success");
                LoadCourier();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        private void BtnHistory_Click(object sender, RoutedEventArgs e)
        {
            new CourierHistoryWindow(_courierId) { Owner = this }.Show();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
    }
}