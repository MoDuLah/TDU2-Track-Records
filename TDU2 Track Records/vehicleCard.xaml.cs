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

        public vehicleCard(VehicleManagement vehicle)
        {
            InitializeComponent();

            VehicleNameTextBlock.Text = vehicle.VehicleName;
            //VehicleBrandTextBlock.Text = vehicle.VehicleBrand;
            //VehicleModelTextBlock.Text = vehicle.VehicleModel;
            //VehiclePriceTextBlock.Text = vehicle.VehiclePrice;
            //VehicleClassTextBlock.Text = vehicle.VehicleClass;
            //Blah.text = vehicle.VehicleAccelerationTime;

            //// Set the vehicle image
            //VehicleImage.Source = LoadImage(vehicle.VehicleImage);
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
