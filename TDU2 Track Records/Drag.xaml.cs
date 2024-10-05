using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TDU2_Track_Records.Properties;
using System.Windows.Threading;

namespace TDU2_Track_Records
{
    public partial class Drag : Window
    {
        private string connectionString = Settings.Default.connectionString;
        private Dictionary<string, int> vehicleDictionary = new Dictionary<string, int>();
        private Dictionary<string, int> originalMaxLengths = new Dictionary<string, int>();
        private DispatcherTimer messageClearTimer; // Timer to clear the message


        public Drag()
        {
            InitializeComponent();
            LoadVehicleData(); // Load vehicle data into the dictionary
            LoadVehicleNames(); // Populate ComboBox
        }

        // Load vehicle data into a dictionary
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
                            int vehicleId = Convert.ToInt32(reader["id"]);
                            vehicleDictionary[vehicleName] = vehicleId; // Store vehicle name with its ID
                        }
                    }
                }
            }
        }

        // Load vehicle names into the ComboBox from the dictionary
        private void LoadVehicleNames()
        {
            VehicleComboBox.Items.Clear(); // Clear existing items
            foreach (var vehicle in vehicleDictionary)
            {
                VehicleComboBox.Items.Add(vehicle.Key); // Add vehicle name to ComboBox
            }
        }

        // Load details for the selected vehicle when "Load From DB" is clicked
        private void LoadFromDBButton_Click(object sender, RoutedEventArgs e)
        {
            if (VehicleComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a vehicle from the list.");
                return;
            }

            string selectedVehicle = VehicleComboBox.SelectedItem.ToString();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = @"
            SELECT 
                _acceleration_0_60_mph,
                _acceleration_0_100_kph,
                _acceleration_0_100_mph,
                _acceleration_0_200_kph,
                _acceleration_0_400m,
                _quartermile_sec,
                _quartermile_speed_kph,
                _1000m_time,
                _1_mi_time,
                _braking_100_kph_0_meters,
                _braking_80_mph_0_meters,
                _braking_70_mph_0_meters,
                _braking_60_mph_0_meters
            FROM vehicles 
            WHERE _vehicle_name = @vehicleName";
                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@vehicleName", selectedVehicle);
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Load the data into the corresponding text boxes
                            ZeroToSixtyTextBox.Text = reader["_acceleration_0_60_mph"] != DBNull.Value ? ((double)reader["_acceleration_0_60_mph"]).ToString("F2") : string.Empty;
                            ZeroToHundredKphTextBox.Text = reader["_acceleration_0_100_kph"] != DBNull.Value ? ((double)reader["_acceleration_0_100_kph"]).ToString("F2") : string.Empty;
                            ZeroToHundredMphTextBox.Text = reader["_acceleration_0_100_mph"] != DBNull.Value ? ((double)reader["_acceleration_0_100_mph"]).ToString("F2") : string.Empty;
                            ZeroToTwoHundredKphTextBox.Text = reader["_acceleration_0_200_kph"] != DBNull.Value ? ((double)reader["_acceleration_0_200_kph"]).ToString("F2") : string.Empty;
                            ZeroToFourHundredMetersTextBox.Text = reader["_acceleration_0_400m"] != DBNull.Value ? ((double)reader["_acceleration_0_400m"]).ToString("F2") : string.Empty;
                            QuarterMileTimeTextBox.Text = reader["_quartermile_sec"] != DBNull.Value ? ((double)reader["_quartermile_sec"]).ToString("F2") : string.Empty;

                            // Convert the quarter mile speed from kph to mph and set it
                            if (reader["_quartermile_speed_kph"] != DBNull.Value)
                            {
                                double quarterMileSpeedKph = (double)reader["_quartermile_speed_kph"];
                                double quarterMileSpeedMph = ConvertKphToMph(quarterMileSpeedKph);
                                QuarterMileSpeedMphTextBox.Text = quarterMileSpeedMph.ToString("F2");
                            }
                            else
                            {
                                QuarterMileSpeedMphTextBox.Text = string.Empty; // Clear if input is invalid
                            }

                            QuarterMileSpeedKphTextBox.Text = reader["_quartermile_speed_kph"] != DBNull.Value ? ((double)reader["_quartermile_speed_kph"]).ToString("F2") : string.Empty;

                            // Convert and load braking distances
                            if (reader["_braking_100_kph_0_meters"] != DBNull.Value)
                            {
                                double braking100KphMeters = (double)reader["_braking_100_kph_0_meters"];
                                Braking100KphDistanceMBox.Text = braking100KphMeters.ToString("F2");
                                Braking100KphDistanceFtBox.Text = ConvertMetersToFeet(braking100KphMeters).ToString("F2"); // Convert to feet
                            }

                            if (reader["_braking_80_mph_0_meters"] != DBNull.Value)
                            {
                                double braking80MphMeters = (double)reader["_braking_80_mph_0_meters"];
                                Braking80MphDistanceMBox.Text = braking80MphMeters.ToString("F2");
                                Braking80MphDistanceFtBox.Text = ConvertMetersToFeet(braking80MphMeters).ToString("F2"); // Convert to feet
                            }

                            if (reader["_braking_70_mph_0_meters"] != DBNull.Value)
                            {
                                double braking70MphMeters = (double)reader["_braking_70_mph_0_meters"];
                                Braking70MphDistanceMBox.Text = braking70MphMeters.ToString("F2");
                                Braking70MphDistanceFtBox.Text = ConvertMetersToFeet(braking70MphMeters).ToString("F2"); // Convert to feet
                            }

                            if (reader["_braking_60_mph_0_meters"] != DBNull.Value)
                            {
                                double braking60MphMeters = (double)reader["_braking_60_mph_0_meters"];
                                Braking60MphDistanceMBox.Text = braking60MphMeters.ToString("F2");
                                Braking60MphDistanceFtBox.Text = ConvertMetersToFeet(braking60MphMeters).ToString("F2"); // Convert to feet
                            }

                            ThousandMetersTextBox.Text = reader["_1000m_time"] != DBNull.Value ? ((double)reader["_1000m_time"]).ToString("F2") : string.Empty;
                            OneMileTextBox.Text = reader["_1_mi_time"] != DBNull.Value ? ((double)reader["_1_mi_time"]).ToString("F2") : string.Empty;
                        }
                        else
                        {
                            MessageBox.Show("No data found for the selected vehicle.");
                        }
                    }
                }
            }
        }

        // Function to convert kph to mph
        private double ConvertKphToMph(double kph)
        {
            return kph * 0.621371; // Conversion factor
        }

        // Function to convert meters to feet
        private double ConvertMetersToFeet(double meters)
        {
            return meters * 3.280839895; // More precise conversion factor
        }

        // Save updated values to the database when "Save to DB" is clicked
        private void SaveToDBButton_Click(object sender, RoutedEventArgs e)
        {
            if (VehicleComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a vehicle from the list.");
                return;
            }

            string selectedVehicle = VehicleComboBox.SelectedItem.ToString();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = @"
                    UPDATE vehicles SET
                        _acceleration_0_60_mph = @zeroToSixty,
                        _acceleration_0_100_kph = @zeroToHundredKph,
                        _acceleration_0_100_mph = @zeroToHundredMph,
                        _acceleration_0_200_kph = @zeroToTwoHundredKph,
                        _quartermile_sec = @quarterMile,
                        _quartermile_speed_kph = @quarterMileSpeedKph,
                        _1000m_time = @thousandMetersTime,
                        _acceleration_0_400m = @acceleration_0_400m,
                        _1_mi_time = @acceleration_1_mi,
                        _braking_100_kph_0_meters = @braking100KphDistanceM,
                        _braking_80_mph_0_meters = @braking80MphDistanceM,
                        _braking_70_mph_0_meters = @braking70MphDistanceM,
                        _braking_60_mph_0_meters = @braking60MphDistanceM
                    WHERE _vehicle_name = @vehicleName";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@zeroToSixty", ZeroToSixtyTextBox.Text);
                    cmd.Parameters.AddWithValue("@zeroToHundredKph", ZeroToHundredKphTextBox.Text);
                    cmd.Parameters.AddWithValue("@zeroToHundredMph", ZeroToHundredMphTextBox.Text);
                    cmd.Parameters.AddWithValue("@zeroToTwoHundredKph", ZeroToTwoHundredKphTextBox.Text);
                    cmd.Parameters.AddWithValue("@acceleration_0_400m", ZeroToFourHundredMetersTextBox.Text);
                    cmd.Parameters.AddWithValue("@quarterMile", QuarterMileTimeTextBox.Text);
                    cmd.Parameters.AddWithValue("@quarterMileSpeedKph", QuarterMileSpeedKphTextBox.Text);
                    cmd.Parameters.AddWithValue("@thousandMetersTime", ThousandMetersTextBox.Text);
                    cmd.Parameters.AddWithValue("@acceleration_1_mi", OneMileTextBox.Text);
                    cmd.Parameters.AddWithValue("@braking100KphDistanceM", Braking100KphDistanceMBox.Text);
                    cmd.Parameters.AddWithValue("@braking80MphDistanceM", Braking80MphDistanceMBox.Text);
                    cmd.Parameters.AddWithValue("@braking70MphDistanceM", Braking70MphDistanceMBox.Text);
                    cmd.Parameters.AddWithValue("@braking60MphDistanceM", Braking60MphDistanceMBox.Text);
                    cmd.Parameters.AddWithValue("@vehicleName", selectedVehicle);

                    cmd.ExecuteNonQuery();
                }
            }

            ShowMessageForLimitedTime(YourMessageTextBlock, "Form submitted successfully!", 3);
            ClearTextBoxes(this);
            // Focus on the 0-60 mph textbox after submission
            ZeroToSixtyTextBox.Focus();
        }
        private void ShowMessageForLimitedTime(TextBlock textBlock, string message, int durationInSeconds)
        {
            // Set the message in the TextBlock
            textBlock.Text = message;
            textBlock.Visibility = Visibility.Visible;
            // Create and start a DispatcherTimer to clear the message
            if (messageClearTimer == null)
            {
                messageClearTimer = new DispatcherTimer();
                messageClearTimer.Tick += (s, e) =>
                {
                    // Clear the message and stop the timer
                    textBlock.Text = string.Empty;
                    messageClearTimer.Stop();
                    textBlock.Visibility = Visibility.Collapsed;
                };
            }

            // Set the timer interval and start it
            messageClearTimer.Interval = TimeSpan.FromSeconds(durationInSeconds);
            messageClearTimer.Start();
        }

        // Check if the text is a valid decimal with up to two decimal places
        private bool IsTextValidDecimal(string text)
        {
            if (string.IsNullOrEmpty(text))
                return true;

            if (double.TryParse(text, out double result))
            {
                // Ensure the text has at most one decimal point
                if (text.Count(c => c == '.') <= 1)
                {
                    int decimalIndex = text.IndexOf('.');
                    if (decimalIndex >= 0)
                    {
                        // Limit to two decimal places
                        string decimalPart = text.Substring(decimalIndex + 1);
                        return decimalPart.Length <= 2;
                    }
                    return true; // No decimal point
                }
            }
            return false; // Invalid decimal format
        }

        // Move focus to the next control
        private void FocusNextControl(TextBox currentTextBox)
        {
            var request = new TraversalRequest(FocusNavigationDirection.Next);
            currentTextBox.MoveFocus(request);
        }
        private void TextBox_Loaded(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                originalMaxLengths[textBox.Name] = textBox.MaxLength; // Store the original max length using the TextBox name as the key
            }
        }

        private void DecimalTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;

            if (textBox != null)
            {
                // Store the original max length if it's not already stored
                if (!originalMaxLengths.ContainsKey(textBox.Name))
                {
                    originalMaxLengths[textBox.Name] = textBox.MaxLength;
                }

                // List of text boxes to ignore the first digit rule
                var ignoreFirstDigitTextBoxes = new HashSet<string>
                 {
                     "ZeroToTwoHundredKphTextBox",
                     "ZeroToFourHundredMetersTextBox",
                     "ThousandMetersTextBox",
                     "OneMileTextBox"
                 };

                // If the current TextBox is one of the specified, skip the first digit check
                if (!ignoreFirstDigitTextBoxes.Contains(textBox.Name))
                {
                    // Validate the decimal text
                    if (IsTextValidDecimal(textBox.Text))
                    {
                        // Check if the first character is a digit and greater than 1
                        if (textBox.Text.Length == 1 && char.IsDigit(textBox.Text[0]) && textBox.Text[0] > '1')
                        {
                            // Decrease max length by 1 if the first digit is greater than 1
                            textBox.MaxLength = Math.Max(0, originalMaxLengths[textBox.Name] - 1);
                        }

                        // If the length reaches the max allowed length, move focus to the next control
                        if (textBox.Text.Length >= textBox.MaxLength)
                        {
                            FocusNextControl(textBox);
                            // Revert back to the original max length after moving focus
                            textBox.MaxLength = originalMaxLengths[textBox.Name];
                        }

                        // Convert feet to meters if this is a braking distance TextBox
                        if (textBox.Name.StartsWith("Braking") && textBox.Name.Contains("DistanceFtBox"))
                        {
                            if (double.TryParse(textBox.Text, out double feet))
                            {
                                double meters = ConvertFeetToMeters(feet);
                                UpdateMetersTextBox(textBox, meters);
                            }
                        }
                    }
                    else
                    {
                        // Clear if the input is invalid
                        textBox.Text = string.Empty;
                    }
                }
                else
                {
                    // For the specified text boxes, simply validate the decimal and handle meter-to-feet conversion
                    if (IsTextValidDecimal(textBox.Text))
                    {
                        // If the length reaches the max allowed length, move focus to the next control
                        if (textBox.Text.Length >= textBox.MaxLength)
                        {
                            FocusNextControl(textBox);
                        }
                    }
                    else
                    {
                        // Clear if the input is invalid
                        textBox.Text = string.Empty;
                    }
                }
            }
        }


        private void DecimalTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                string currentText = textBox.Text;
                string newText = currentText.Insert(textBox.SelectionStart, e.Text);

                // Validate if the new text is a valid decimal number
                if (!IsTextValidDecimal(newText))
                {
                    e.Handled = true; // Prevent invalid input
                }
            }
        }

        // Convert feet to meters (used for braking textboxes)
        private double ConvertFeetToMeters(double feet)
        {
            // More precise conversion factor
            double meters = feet * 0.3048; // 1 foot = 0.3048 meters
            return Math.Round(meters, 2); // Round to 2 decimal places
        }

        // Update the corresponding meter TextBox for braking distances
        private void UpdateMetersTextBox(TextBox feetTextBox, double meters)
        {
            TextBox metersTextBox = null;

            switch (feetTextBox.Name)
            {
                case "Braking100KphDistanceFtBox":
                    metersTextBox = Braking100KphDistanceMBox;
                    break;
                case "Braking80MphDistanceFtBox":
                    metersTextBox = Braking80MphDistanceMBox;
                    break;
                case "Braking70MphDistanceFtBox":
                    metersTextBox = Braking70MphDistanceMBox;
                    break;
                case "Braking60MphDistanceFtBox":
                    metersTextBox = Braking60MphDistanceMBox;
                    break;
            }

            if (metersTextBox != null)
            {
                metersTextBox.Text = meters.ToString("F2"); // Ensure two decimal places in the meters TextBox
            }
        }

        // Convert mph to kph
        private double ConvertMphToKph(double mph)
        {
            return Math.Floor(mph * 1.609344 * 100) / 100; // Convert to kph and round down to 2 decimal places
        }

        // Update kph text box when mph changes
        private void QuarterMileSpeedMphTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;

            if (textBox != null && IsTextValidDecimal(textBox.Text))
            {
                // Convert mph to kph using the updated conversion
                if (double.TryParse(textBox.Text, out double speedMph))
                {
                    // Convert mph to kph using the updated conversion
                    double speedKph = ConvertMphToKph(speedMph);

                    // Set the text box with formatted value
                    QuarterMileSpeedKphTextBox.Text = speedKph.ToString("F2"); // Format to 2 decimal places
                }
                else
                {
                    QuarterMileSpeedKphTextBox.Text = string.Empty; // Clear if input is invalid
                }

                // Check if the length of the input has reached the max allowed length
                if (textBox.Text.Length >= textBox.MaxLength)
                {
                    // Move focus to the next control
                    FocusNextControl(textBox);
                }
            }
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
                    comboBox.SelectedIndex++;
                }
                ClearTextBoxes(child);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ClearTextBoxes(this);
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
    }
}
