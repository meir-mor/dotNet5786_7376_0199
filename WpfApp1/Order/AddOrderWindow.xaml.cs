using System;
using System.Windows;

namespace PL.Order
{
    public partial class AddOrderWindow : Window
    {
        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        public BO.Order CurrentOrder
        {
            get => (BO.Order)GetValue(CurrentOrderProperty);
            set => SetValue(CurrentOrderProperty, value);
        }

        public static readonly DependencyProperty CurrentOrderProperty =
            DependencyProperty.Register(nameof(CurrentOrder), typeof(BO.Order), typeof(AddOrderWindow),
                new PropertyMetadata(null));

        public AddOrderWindow()
        {
            InitializeComponent();
            CurrentOrder = new BO.Order();
            DataContext = this;
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var managerId = s_bl.Admin.GetConfig().ManagerId;
                s_bl.Order.AddOrder(managerId, CurrentOrder);
                MessageBox.Show("Order added successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
