using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using TDU2_Track_Records.Classes;
using TDU2_Track_Records.ViewModels;

namespace TDU2_Track_Records
{
    /// <summary>
    /// Interaction logic for vehicleCard.xaml
    /// </summary>
    public partial class vehicleCard : Window
    {
        //private vehicleCardViewModel viewModel;

        public vehicleCard()
        {
            InitializeComponent();
            //viewModel = new vehicleCardViewModel();
            //DataContext = new vehicleCardViewModel();
            //DataContext = viewModel;
        }

        // Example properties for binding
 
        // Example of loading an image from a file path
        private BitmapImage LoadImage(string filePath)
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(filePath, UriKind.RelativeOrAbsolute);
            bitmap.EndInit();
            return bitmap;
        }

        // If you have specific actions, you can define event handlers here
        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Minimize_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Window.GetWindow(this).WindowState = WindowState.Minimized;
        }

        private void Close_Button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //int vehicleId = 192; // or get this ID dynamically as per your requirement
            ////viewModel.LoadVehicleById(vehicleId);
        }
    }
}
