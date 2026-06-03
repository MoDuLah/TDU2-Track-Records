using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;
using TDU2_Track_Records.Properties;

namespace TDU2_Track_Records
{
    public partial class OdometerWindow : Window
    {
        private List<VehicleManagement> _vehicles = new List<VehicleManagement>();
        readonly string connectionString = Settings.Default.connectionString;
        readonly string SI = Settings.Default.system;
        readonly string distance = Settings.Default.distance;

        public OdometerWindow()
        {
            InitializeComponent();
            LoadVehicles();
            Distance.Text = distance;
        }

        private void LoadVehicles()
        {
            try
            {
                using (var conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT id, _vehicle_name, _odometer_metric, _odometer_imperial FROM vehicles WHERE _is_owned = 'true' ORDER BY _vehicle_name ASC;";
                    using (var cmd = new SQLiteCommand(query, conn))
                    {
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            _vehicles.Clear();

                            while (reader.Read())
                            {
                                var vehicle = new VehicleManagement
                                {
                                    id = reader.GetInt32(0),
                                    VehicleName = reader.GetString(1),
                                    VehicleOdometerMetric = reader.GetDouble(2),
                                    VehicleOdometerImperial = reader.GetDouble(3)
                                };
                                _vehicles.Add(vehicle);
                            }

                            VehicleComboBox.ItemsSource = _vehicles;
                            VehicleComboBox.DisplayMemberPath = "VehicleName";
                            VehicleComboBox.SelectedValuePath = "id";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading vehicles: {ex.Message}");
            }
        }

        private void VehicleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (VehicleComboBox.SelectedItem is VehicleManagement selectedVehicle)
            {
                PopulateVehicleDetails(selectedVehicle);
            }
        }

        private void PopulateVehicleDetails(VehicleManagement selectedVehicle)
        {
            if (SI == "Metric")
            {
                _odometer_metric.Text = selectedVehicle.VehicleOdometerMetric == 0.0 ? string.Empty : selectedVehicle.VehicleOdometerMetric.ToString("F1");
                _odometer_imperial.Visibility = Visibility.Collapsed;
                _odometer_metric.Visibility = Visibility.Visible;
            }
            else if (SI == "Imperial")
            {
                _odometer_imperial.Text = selectedVehicle.VehicleOdometerImperial == 0.0 ? string.Empty : selectedVehicle.VehicleOdometerImperial.ToString("F1");
                _odometer_metric.Visibility = Visibility.Collapsed;
                _odometer_imperial.Visibility = Visibility.Visible;
            }
        }

        private void SaveChangesButton_Click(object sender, RoutedEventArgs e)
        {
            if (VehicleComboBox.SelectedItem is VehicleManagement selectedVehicle)
            {
                double mileage = 0.0;

                if (SI == "Metric" && !string.IsNullOrWhiteSpace(_odometer_metric.Text) && double.TryParse(_odometer_metric.Text, out mileage))
                {
                    selectedVehicle.VehicleOdometerMetric = mileage;
                    selectedVehicle.VehicleOdometerImperial = Math.Round(mileage * 0.621371, 1);
                }
                else if (SI == "Imperial" && !string.IsNullOrWhiteSpace(_odometer_imperial.Text) && double.TryParse(_odometer_imperial.Text, out mileage))
                {
                    selectedVehicle.VehicleOdometerImperial = mileage;
                    selectedVehicle.VehicleOdometerMetric = Math.Round(mileage * 1.60934, 1);
                }

                UpdateVehicleOdometer(selectedVehicle);

                // Focus on the visible odometer textbox
                if (SI == "Metric")
                {
                    _odometer_metric.Focus();
                }
                else
                {
                    _odometer_imperial.Focus();
                }

                // Move to the next item in the VehicleComboBox
                int currentIndex = VehicleComboBox.SelectedIndex;
                if (currentIndex < VehicleComboBox.Items.Count - 1)
                {
                    VehicleComboBox.SelectedIndex = currentIndex + 1;
                }
                else
                {
                    VehicleComboBox.SelectedIndex = 0;
                }
            }
        }


        private void UpdateVehicleOdometer(VehicleManagement vehicle)
        {
            string query = @"
                UPDATE vehicles SET 
                    _odometer_metric = @odometerMetric,
                    _odometer_imperial = @odometerImperial
                WHERE id = @id";

            try
            {
                using (var conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", vehicle.id);
                        cmd.Parameters.AddWithValue("@odometerMetric", vehicle.VehicleOdometerMetric);
                        cmd.Parameters.AddWithValue("@odometerImperial", vehicle.VehicleOdometerImperial);

                        cmd.ExecuteNonQuery();
                    }
                }
               
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating vehicle odometer: {ex.Message}");
            }
        }

        // Text validation: only allow one decimal point and only one digit after the decimal
        private void OneDecimalPointTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            string currentText = textBox.Text.Insert(textBox.SelectionStart, e.Text);

            // Regular expression to match up to one decimal point with a maximum of one digit after it
            var regex = new Regex(@"^\d*\.?\d{0,1}$");

            // Validate the input, ensuring only one decimal and one digit after the decimal
            e.Handled = !regex.IsMatch(currentText);
        }

        private void VehicleManagement_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }
}
