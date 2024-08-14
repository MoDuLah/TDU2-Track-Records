using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TDU2_Track_Records
{
    /// <summary>
    /// Interaction logic for CloseButton.xaml
    /// </summary>
    public partial class CloseButton : UserControl
    {
        public CloseButton()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }

        private void closeImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }
    }
}
