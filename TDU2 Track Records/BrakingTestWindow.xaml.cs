using System;
using System.Collections.Generic;
using System.Data.SQLite; // Make sure to install System.Data.SQLite NuGet package
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using TDU2_Track_Records.Properties;

namespace TDU2_Track_Records
{
    public partial class BrakingTestWindow : Window
    {
        private string connectionString = Settings.Default.connectionString;
        private Dictionary<string, int> vehicleDictionary = new Dictionary<string, int>();
        private Dictionary<string, int> originalMaxLengths = new Dictionary<string, int>();
        private DispatcherTimer messageClearTimer; // Timer to clear the message
        int vehicleId;

        public BrakingTestWindow()
        {
            InitializeComponent();
            LoadVehicleData(); // Load vehicle data into the dictionary
            LoadVehicleNames(); // Populate ComboBox
        }
        private void TextBox_Loaded(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                originalMaxLengths[textBox.Name] = textBox.MaxLength; // Store the original max length using the TextBox name as the key
            }
        }
        private void LoadVehicleData()
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // Base query
                string query = "SELECT id, _vehicle_name FROM vehicles";

                // List to store conditions
                List<string> conditions = new List<string>();

                // Check the state of each checkbox and add conditions as needed
                if (VehicleOwnedCheckBox.IsChecked == true) conditions.Add("_is_owned = 'true'");
                if (VehicleActiveCheckBox.IsChecked == true) conditions.Add("_is_active = 'true'");
                if (VehiclePurchasableCheckBox.IsChecked == true) conditions.Add("_is_purchasable = 'true'");
                if (VehicleRewardCheckBox.IsChecked == true) conditions.Add("_is_reward = 'true'");

                // If there are any conditions, append them to the query
                if (conditions.Count > 0)
                {
                    query += " WHERE " + string.Join(" AND ", conditions);
                }

                // Add ordering to the query
                query += " ORDER BY _vehicle_name ASC;";

                // Execute the query
                using (var cmd = new SQLiteCommand(query, connection))
                {
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        vehicleDictionary.Clear(); // Clear the dictionary before adding new entries
                        while (reader.Read())
                        {
                            string vehicleName = reader["_vehicle_name"].ToString();
                            vehicleId = Convert.ToInt32(reader["id"]);
                            vehicleDictionary[vehicleName] = vehicleId; // Store vehicle name with its ID
                        }
                    }
                }
            }
        }
        // Event handler for LostFocus to automatically add the decimal point
        private void BrakingDistance_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            // Validate numeric input
            if (double.TryParse(textBox.Text, out double value))
            {
                if (textBox.Text.Length > 2 && !textBox.Text.Contains("."))
                {
                    // Insert decimal point after the second digit
                    textBox.Text = textBox.Text.Insert(textBox.Text.Length - 2, ".");
                }
            }
            else
            {
                // Clear invalid input
                textBox.Text = string.Empty;
            }
        }

        // Reset Button Click Handler
        private void ResetData_Click(object sender, RoutedEventArgs e)
        {
            Braking100KphDistanceMBox.Text = "";
            Braking80MphDistanceMBox.Text = "";
            Braking70MphDistanceMBox.Text = "";
            Braking60MphDistanceMBox.Text = "";
        }
        private void LoadVehicleNames()
        {
            VehicleComboBox.Items.Clear(); // Clear existing items
            foreach (var vehicle in vehicleDictionary)
            {
                VehicleComboBox.Items.Add(vehicle.Key); // Add vehicle name to ComboBox
            }
        }
        // Save Button Click Handler
        private void SaveData_Click(object sender, RoutedEventArgs e)
        {
            if (VehicleComboBox.SelectedItem == null)
            {
                ShowMessageForLimitedTime(YourMessageTextBlock, "Please select a vehicle from the list.",3);
                return;
            }

            string selectedVehicle = VehicleComboBox.SelectedItem.ToString();

            if (string.IsNullOrWhiteSpace(Braking100KphDistanceMBox.Text) ||
                string.IsNullOrWhiteSpace(Braking80MphDistanceMBox.Text) ||
                string.IsNullOrWhiteSpace(Braking70MphDistanceMBox.Text) ||
                string.IsNullOrWhiteSpace(Braking60MphDistanceMBox.Text) ||
                string.IsNullOrEmpty(selectedVehicle))
            {
                ShowMessageForLimitedTime(YourMessageTextBlock, "Please fill in all fields before submitting!", 3);
                return; // Exit the method if validation fails
            }

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO braking (vID, vehicle, distance_100kph, distance_80mph, distance_70mph, distance_60mph) VALUES (@vID, @vehicle, @dist100, @dist80, @dist70, @dist60)";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@vID", vehicleId);
                        cmd.Parameters.AddWithValue("@vehicle", selectedVehicle);
                        cmd.Parameters.AddWithValue("@dist100", Convert.ToDouble(Braking100KphDistanceMBox.Text));
                        cmd.Parameters.AddWithValue("@dist80", Convert.ToDouble(Braking80MphDistanceMBox.Text));
                        cmd.Parameters.AddWithValue("@dist70", Convert.ToDouble(Braking70MphDistanceMBox.Text));
                        cmd.Parameters.AddWithValue("@dist60", Convert.ToDouble(Braking60MphDistanceMBox.Text));

                        cmd.ExecuteNonQuery();
                    }
                    ShowMessageForLimitedTime(YourMessageTextBlock, "Form submitted successfully!", 3);
                    ClearTextBoxes(this);
                    Braking100KphDistanceMBox.Focus(); // Focus on the next field
                }
            }
            catch (Exception ex)
            {
                ShowMessageForLimitedTime(YourMessageTextBlock, "Error saving data: " + ex.Message, 3);
            }
        }
        private void FocusNextControl(TextBox currentTextBox)
        {
            var request = new TraversalRequest(FocusNavigationDirection.Next);
            currentTextBox.MoveFocus(request);
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
        private void ShowMessageForLimitedTime(TextBlock textBlock, string message, int durationInSeconds)
        {
            // Stop the timer if it's already running
            if (messageClearTimer != null && messageClearTimer.IsEnabled)
            {
                messageClearTimer.Stop();
            }

            // Set the message in the TextBlock
            textBlock.Text = message;
            textBlock.Visibility = Visibility.Visible;

            // Create and start the DispatcherTimer
            if (messageClearTimer == null)
            {
                messageClearTimer = new DispatcherTimer();
                messageClearTimer.Tick += (s, e) =>
                {
                    textBlock.Text = string.Empty;
                    messageClearTimer.Stop();
                    textBlock.Visibility = Visibility.Collapsed;
                };
            }

            messageClearTimer.Interval = TimeSpan.FromSeconds(durationInSeconds);
            messageClearTimer.Start();
        }


        private void ClearTextBoxes(DependencyObject parent)
        {
            // Iterate through all child elements
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                // Check the type of control and reset accordingly
                if (child is TextBox textBox)
                {
                    textBox.Text = string.Empty;
                }
                if (child is ComboBox comboBox)
                {
                    // Check the attempts for the selected vehicle
                    string selectedVehicle = VehicleComboBox.SelectedItem?.ToString();
                    if (!string.IsNullOrEmpty(selectedVehicle))
                    {
                        using (var connection = new SQLiteConnection(connectionString))
                        {
                            connection.Open();

                            // Prepare the query to count the entries for the selected vehicle
                            string query = "SELECT COUNT(*) FROM braking WHERE vehicle = @vehicle";
                            using (var cmd = new SQLiteCommand(query, connection))
                            {
                                cmd.Parameters.AddWithValue("@vehicle", selectedVehicle);
                                int attemptsCount = Convert.ToInt32(cmd.ExecuteScalar());

                                // Update the Attempts TextBlock
                                Attempts.Text = $"Attempts: {attemptsCount}";

                                // Check if attemptsCount equals 5
                                if (attemptsCount == 5)
                                {
                                    ShowAverageBrakingData();
                                    // Increment the ComboBox SelectedIndex if possible
                                    if (VehicleComboBox.SelectedIndex < VehicleComboBox.Items.Count - 1)
                                    {
                                        VehicleComboBox.SelectedIndex++;
                                    }
                                }
                            }
                        }
                    }
            }
                ClearTextBoxes(child);
            }
        }
        private void ShowAverageBrakingData()
        {
            if (VehicleComboBox.SelectedItem == null)
            {
                ShowMessageForLimitedTime(YourMessageTextBlock, "Please select a vehicle from the list.", 3);
                return;
            }

            string selectedVehicle = VehicleComboBox.SelectedItem.ToString();

            // Retrieve the vehicle ID

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // Query to get the averages
                string query = @"
            SELECT 
                AVG(distance_100kph) AS AvgDistance100Kph, 
                AVG(distance_80mph) AS AvgDistance80Mph, 
                AVG(distance_70mph) AS AvgDistance70Mph, 
                AVG(distance_60mph) AS AvgDistance60Mph 
            FROM 
                braking 
            WHERE 
                vehicle = @vehicleId";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@vehicleId", selectedVehicle);

                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            double avg100Kph = reader.IsDBNull(0) ? 0 : reader.GetDouble(0);
                            double avg80Mph = reader.IsDBNull(1) ? 0 : reader.GetDouble(1);
                            double avg70Mph = reader.IsDBNull(2) ? 0 : reader.GetDouble(2);
                            double avg60Mph = reader.IsDBNull(3) ? 0 : reader.GetDouble(3);

                            // Create the message
                            string message = $"Average Braking Distances\n for {selectedVehicle}:\n" +
                                             $"100 kph: {avg100Kph:F2} m\n" +
                                             $"80 mph: {avg80Mph:F2} m\n" +
                                             $"70 mph: {avg70Mph:F2} m\n" +
                                             $"60 mph: {avg60Mph:F2} m";

                            // Show the message box
                            MessageBox.Show(message, "Average Braking Distances", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            ShowMessageForLimitedTime(YourMessageTextBlock, "No data found for the selected vehicle.", 3);
                        }
                    }
                }
            }
        }
        private void ReloadButton_Click(object sender, RoutedEventArgs e)
        {
            // Clear the Items collection before setting ItemsSource
            // Load the vehicle data (this will refresh the vehicleDictionary)
            LoadVehicleData();
            VehicleComboBox.Items.Clear(); // Ensure no conflict between Items and ItemsSource
            LoadVehicleNames();
        }

        private void VehiclePurchasableCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (VehicleRewardCheckBox.IsChecked == true)
            {
                VehicleRewardCheckBox.IsChecked = false;
            }
        }

        private void VehicleRewardCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (VehiclePurchasableCheckBox.IsChecked == true)
            {
                VehiclePurchasableCheckBox.IsChecked = false;
            }
        }

        private void Braking_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;

            if (textBox != null) { 
                // If the length reaches the max allowed length, move focus to the next control
                if (textBox.Text.Length >= textBox.MaxLength)
            {
                FocusNextControl(textBox);
                // Revert back to the original max length after moving focus
                textBox.MaxLength = originalMaxLengths[textBox.Name];
            }
            }
        }
        private void VehicleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Get the selected vehicle name
            string selectedVehicle = VehicleComboBox.SelectedItem?.ToString();

            if (!string.IsNullOrEmpty(selectedVehicle))
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    // Prepare the query to count the entries for the selected vehicle
                    string query = "SELECT COUNT(*) FROM braking WHERE vehicle = @vehicle";
                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@vehicle", selectedVehicle);
                        int attemptsCount = Convert.ToInt32(cmd.ExecuteScalar());

                        // Update the Attempts TextBlock
                        Attempts.Text = $"Attempts: {attemptsCount}";
                    }
                }
            }
            else
            {
                // If no vehicle is selected, reset the Attempts TextBlock
                Attempts.Text = "Attempts: 0";
            }
        }
    }
}