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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TDU2_Track_Records
{
    /// <summary>
    /// Interaction logic for VehicleButton.xaml
    /// </summary>
    public partial class VehicleButton : UserControl
    {
        public VehicleButton()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, MouseButtonEventArgs e)
        {
            var VehicleWindow = new Vehicle();
            VehicleWindow.ShowDialog();
        }
    }
}
