using System.Data.SQLite;
using System;
using System.Windows;
using System.Windows.Input;
using TDU2_Track_Records.Properties;

namespace TDU2_Track_Records
{
    /// <summary>
    /// Interaction logic for vehicleCard.xaml
    /// </summary>
    public partial class vehicleCard : Window
    {
        readonly string connectionString = Settings.Default.connectionString;
        public string distance, speed;
        readonly string SI = Settings.Default.system;

        public vehicleCard(VehicleManagement vehicle)
        {
            InitializeComponent();

            GetVehicleDetails(vehicle.id);
        }
        private VehicleManagement GetVehicleDetails(int vehicleId)
        {
            VehicleManagement vehicle = null;
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = $"SELECT * FROM vehicles WHERE id = '{vehicleId}'";
                MessageBox.Show(query);
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", vehicleId);
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            vehicle = new VehicleManagement
                            {
                                VehicleName = reader["Name"].ToString(),
                            };
                        }
                    }
                }
            }
            return vehicle;
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
    }
}
