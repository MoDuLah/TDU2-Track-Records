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
            VehicleLengthTextBlock.Text = vehicle.VehicleLength;
            VehicleWidthTextBlock.Text = vehicle.VehicleWidth;
            //VehicleBrandTextBlock.Text = vehicle.VehicleBrand;
            //VehicleModelTextBlock.Text = vehicle.VehicleModel;
            //VehiclePriceTextBlock.Text = vehicle.VehiclePrice;
            //VehicleClassTextBlock.Text = vehicle.VehicleClass;
            //Blah.text = vehicle.VehicleAccelerationTime;
            UpdateDisplay(vehicle.VehicleEnginePosition, vehicle.VehicleWheelDrive);

            //// Set the vehicle image
            //VehicleImage.Source = LoadImage(vehicle.VehicleImage);
        }

        private void UpdateDisplay(string chassisType, string wheeldrive)
        {
            // Example logic to show/hide chassis components
            stackpanelFWD.Visibility = wheeldrive == "FWD" ? Visibility.Visible : Visibility.Hidden;
            stackpanelAWD.Visibility = wheeldrive == "AWD" ? Visibility.Visible : Visibility.Hidden;
            stackpanelRWD.Visibility = wheeldrive == "RWD" ? Visibility.Visible : Visibility.Hidden;
            stackpanelFENG.Visibility = chassisType == "FENG" ? Visibility.Visible : Visibility.Hidden;
            stackpanelMENG.Visibility = chassisType == "MENG" ? Visibility.Visible : Visibility.Hidden;
            stackpanelRENG.Visibility = chassisType == "RENG" ? Visibility.Visible : Visibility.Hidden;
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
