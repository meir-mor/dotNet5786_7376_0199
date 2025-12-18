using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PL.Order;

/// <summary>
/// Interaction logic for AddOrderWindow.xaml
/// </summary>
public partial class AddOrderWindow : Window
{
    static readonly BlApi.IBl bl = BlApi.Factory.Get();
    public BO.Order CurrentOrder { get; set; }

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
            bl.Order.AddOrder(CurrentOrder);
            MessageBox.Show("Order added successfully");
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }
}
