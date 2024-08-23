using System.Windows.Controls;
using System.Windows.Input;

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
            VehicleWindow.Show();
        }
    }
}
