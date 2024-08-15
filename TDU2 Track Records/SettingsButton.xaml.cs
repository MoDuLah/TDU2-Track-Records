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
    /// Interaction logic for SettingsButton.xaml
    /// </summary>
    public partial class SettingsButton : UserControl
    {
        public SettingsButton()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, MouseButtonEventArgs e)
        {
            // Prompt the user before closing the main window
            MessageBoxResult result = MessageBox.Show(
                "Changing the settings will reload the main window, and any unsaved data will be lost. Do you want to continue?",
                "Warning",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                var settingsWindow = new SettingsWindow();
                settingsWindow.ShowDialog();
            }
            else
            {
                // User canceled the operation, do nothing
            }

        }
    }
}
