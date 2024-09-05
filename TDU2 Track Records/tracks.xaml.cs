using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TDU2_Track_Records.Properties;
using static System.Net.Mime.MediaTypeNames;

namespace TDU2_Track_Records
{
    public partial class tracks : Window
    {
        public int Popup = 0;
        readonly string connectionString = Settings.Default.connectionString;
        public string distance = Settings.Default.distance;
        public string speed = Settings.Default.speed;
        readonly string SI = Settings.Default.system;
        public int keep = 0;

        public tracks()
        {
            InitializeComponent();
            LapsGroupBox.Visibility = Visibility.Collapsed;
            DistanceGroupBox.Visibility = Visibility.Collapsed;
            SpeedGroupBox.Visibility = Visibility.Collapsed;
            CheckpointsGroupBox.Visibility = Visibility.Collapsed;
            RestrictedClassGroupBox.Visibility = Visibility.Collapsed;
            RestrictedCarGroupBox.Visibility = Visibility.Collapsed;


            List<ComboBoxItem> items = new List<ComboBoxItem>
            {
                new ComboBoxItem { ImagePath = "Images/ico/events/ico_cup_race.png", Description = "Race Cup", Value = "Race SP" },
                new ComboBoxItem { ImagePath = "Images/ico/events/ico_cup_speed.png", Description = "Speed Cup", Value = "Speed SP" },
                new ComboBoxItem { ImagePath = "Images/ico/events/ico_cup_eliminator.png", Description = "Eliminator Cup", Value = "Eliminator SP" },
                new ComboBoxItem { ImagePath = "Images/ico/events/ico_cup_speedtrap.png", Description = "Speed Trap Cup", Value = "Speed Trap SP" },
                new ComboBoxItem { ImagePath = "Images/ico/events/ico_cup_timeattack.png", Description = "Time Attack Cup", Value = "Time Attack SP" },
                new ComboBoxItem { ImagePath = "Images/ico/events/ico_mp_race.png", Description = "Race Multiplayer", Value = "Race MP" },
                new ComboBoxItem { ImagePath = "Images/ico/events/ico_mp_race.png", Description = "Race Ranked Multiplayer", Value = "Race RMP" },
                new ComboBoxItem { ImagePath = "Images/ico/events/ico_mp_speed.png", Description = "Speed Multiplayer", Value = "Speed MP" },
                new ComboBoxItem { ImagePath = "Images/ico/events/ico_mp_ftl.png", Description = "Follow The Leader", Value = "Follow The Leader MP" },
                new ComboBoxItem { ImagePath = "Images/ico/events/ico_mp_kyd.png", Description = "Keep Your Distance", Value = "Keep Your Distance MP" },
                new ComboBoxItem { ImagePath = "Images/ico/events/ico_mp_speedtrap.png", Description = "Speed Trap Multiplayer", Value = "Speed Trap MP" }
            };

            RaceTypeComboBox.ItemsSource = items;
            PopulateClasses();
            ShowLastEntry();
        }

        private void PopulateClasses()
        {
            List<ComboBoxItem> items = new List<ComboBoxItem>
            {
                new ComboBoxItem { ImagePath = "Images/carClasses/SC.png", Description = "None", Value = "" },
                new ComboBoxItem { ImagePath = "Images/carClasses/A1.png", Description = "Asphalt 1", Value = "A1" },
                new ComboBoxItem { ImagePath = "Images/carClasses/A2.png", Description = "Asphalt 2", Value = "A2" },
                new ComboBoxItem { ImagePath = "Images/carClasses/A3.png", Description = "Asphalt 3", Value = "A3" },
                new ComboBoxItem { ImagePath = "Images/carClasses/A4.png", Description = "Asphalt 4", Value = "A4" },
                new ComboBoxItem { ImagePath = "Images/carClasses/A5.png", Description = "Asphalt 5", Value = "A5" },
                new ComboBoxItem { ImagePath = "Images/carClasses/A6.png", Description = "Asphalt 6", Value = "A6" },
                new ComboBoxItem { ImagePath = "Images/carClasses/A7.png", Description = "Asphalt 7", Value = "A7" },
                new ComboBoxItem { ImagePath = "Images/carClasses/B1.png", Description = "Rally 1", Value = "B1" },
                new ComboBoxItem { ImagePath = "Images/carClasses/B2.png", Description = "Rally 2", Value = "B2" },
                new ComboBoxItem { ImagePath = "Images/carClasses/B3.png", Description = "Rally 3", Value = "B3" },
                new ComboBoxItem { ImagePath = "Images/carClasses/B4.png", Description = "Rally 4", Value = "B4" },
                new ComboBoxItem { ImagePath = "Images/carClasses/C1.png", Description = "Classic 1", Value = "C1" },
                new ComboBoxItem { ImagePath = "Images/carClasses/C2.png", Description = "Classic 2", Value = "C2" },
                new ComboBoxItem { ImagePath = "Images/carClasses/C3.png", Description = "Classic 3", Value = "C3" },
                new ComboBoxItem { ImagePath = "Images/carClasses/C4.png", Description = "Classic 4", Value = "C4" },
                new ComboBoxItem { ImagePath = "Images/carClasses/MA1.png", Description = "Motorcycles 1", Value = "mA1" },
                new ComboBoxItem { ImagePath = "Images/carClasses/MA2.png", Description = "Motorcycles 2", Value = "mA2" }
            };

            RestrictedClassComboBox.ItemsSource = items;
            //RestrictedCarComboBox.ItemsSource = "";
        }
        private void ShowLastEntry()
        {
            try
            {
                // Define the query to get the last entry
                string query = "SELECT Name, RaceType, Length, RestrictedClass FROM tracks ORDER BY id DESC LIMIT 1";

                // Execute the query and retrieve the data
                using (var conn = new SQLiteConnection(Settings.Default.connectionString))
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read()) // If a record is found
                        {
                            // Retrieve the values from the last entry
                            string trackName = reader["Name"].ToString();
                            string raceType = reader["RaceType"].ToString();
                            string length = reader["Length"].ToString();
                            string RestictClass = reader["RestrictedClass"].ToString(); 
                            // Update the TextBlock with the information
                            LastEntry.Text = $"Last Entry: {trackName},     Class: {RestictClass},     Type: {raceType},     Length: {length}";
                        }
                        else
                        {
                            // If no record is found, display a message
                            LastEntry.Text = "No entries found.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any errors that may have occurred
                MessageBox.Show("An error occurred while retrieving the last entry: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BindVehicleComboBox(ComboBox comboBox)
        {
            ComboBoxItem selectedItem = (ComboBoxItem)RestrictedClassComboBox.SelectedItem;
            string query = "SELECT * FROM vehicles WHERE _is_available = 'true'";

            if (RestrictedClassComboBox.SelectedIndex > 0)
            {
                string selectedValue = selectedItem.Value;
                if (selectedValue != "None")
                {
                    query += $" AND _vehiclecategory_name = '{selectedValue}' ORDER BY _vehicle_name ASC;";
                }
            }
            else
            {
                query += " ORDER BY _vehicle_name ASC;";
            }

            ExecuteQuery(query, "vehicles", dataSet =>
            {
                if (dataSet.Tables[0].Rows.Count > 0)
                {
                    SetComboBoxSource(comboBox, dataSet, "_vehicle_name", "id");
                }
                else
                {
                    Popup = Popup + 1;
                    if (Popup == 1)
                    {
                        MessageBox.Show("No vehicles found.\nPlease go to Vehicle Management to add vehicles.\n\nTip: Vehicle management is the garage icon on top right.",
                                    "Vehicle Management Required",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Exclamation);

                        var VehicleWindow = new Vehicle();
                        VehicleWindow.Show();
                    }
                }
            },
            ex => MessageBox.Show($"An error occurred while loading vehicles:\n{ex.Message}"),
            ("@Class", RestrictedClassComboBox.Text));
        }
        private void ExecuteQuery(string query, string tableName, Action<DataSet> onSuccess, Action<Exception> onError, params (string, object)[] parameters)
        {
            try
            {
                using (var dbConn = new SQLiteConnection(connectionString))
                using (var dbCmd = new SQLiteCommand(query, dbConn))
                using (var dbAdapter = new SQLiteDataAdapter(dbCmd))
                {
                    foreach (var param in parameters)
                    {
                        dbCmd.Parameters.AddWithValue(param.Item1, param.Item2);
                    }

                    dbConn.Open();
                    var dataSet = new DataSet();
                    dbAdapter.Fill(dataSet, tableName);
                    onSuccess(dataSet);
                }
            }
            catch (Exception ex)
            {
                onError(ex);
            }
        }
        private void SetComboBoxSource(ComboBox comboBox, DataSet dataSet, string displayMemberPath, string selectedValuePath)
        {
            if (dataSet.Tables.Count > 0)
            {
                comboBox.ItemsSource = dataSet.Tables[0].DefaultView;
                if (comboBox.Name == "RestrictedCarComboBox") {
                    comboBox.DisplayMemberPath = "_vehicle_name";  // or whichever column is displayed
                    comboBox.SelectedValuePath = "id";    // Ensure this is the ID column
                }
                else
                {
                    comboBox.DisplayMemberPath = displayMemberPath;
                    comboBox.SelectedValuePath = selectedValuePath;

                }
            }
            else
            {
                MessageBox.Show($"No data found in the {displayMemberPath} table.");
            }
        }

        private void AddTrackButton_Click(object sender, RoutedEventArgs e)
        {
            if (RaceTypeComboBox.SelectedIndex == -1) {
                LastEntry.Text = "Fail";
                LastEntry.Foreground = new SolidColorBrush(Colors.Red);
                RaceTypeComboBox.Focus();
                return;
            }
            else
            {
                LastEntry.Text = null;
                LastEntry.Foreground = new SolidColorBrush(Colors.White);
            }

            double totalDistanceText = 0;
            string lapsText = "";
            string restrictedClass = "";
            string restrictedCar = "";
            // Retrieve and trim input values
            var selectedItemType = RaceTypeComboBox.SelectedItem as ComboBoxItem;
            string raceType = selectedItemType.Value;
            string trackName = TrackNameTextBox.Text.Trim();

            if (LapsGroupBox.Visibility == Visibility.Visible) { 
                    lapsText = LapsTextBox.Text.Trim();
            } else {
                    lapsText = "1";
            }
            if (DistanceGroupBox.Visibility == Visibility.Visible) { 
            totalDistanceText = Convert.ToDouble(LapLengthTextBox.Text);
            } else {
                totalDistanceText = 0;
            }
            string minSpeedText = MinSpeedTextBox.Text.Trim();
            string checkpointsText = CheckpointsTextBox.Text.Trim();
            
            if (RestrictedClassGroupBox.Visibility == Visibility.Visible) { 
                    var selectedItemClass = RestrictedClassComboBox.SelectedItem as ComboBoxItem;
                    restrictedClass = selectedItemClass?.Value;
            } else { 
                restrictedClass = "";
            }
            //if (RestrictedCarGroupBox.Visibility == Visibility.Visible) {
            //    var selectedItemCar = RestrictedCarComboBox.SelectedItem as ComboBoxItem;
            //    restrictedCar = selectedItemCar?.Value;
            //} else {
            //    restrictedCar = "";
            //}
            if (RestrictedCarGroupBox.Visibility == Visibility.Visible)
            {
                if (RestrictedCarComboBox.SelectedItem is DataRowView selectedItemCar)
                {
                    restrictedCar = selectedItemCar["id"].ToString(); // Assuming "id" is the value you need
                }
                else
                {
                    restrictedCar = "";
                }
            }
            else
            {
                restrictedCar = "";
            }
            //MessageBox.Show($"Track Name: {trackName}, Race Type: {raceType},\n" +
            //    $" Length: {totalDistanceText}, Laps: {lapsText},\n" +
            //    $" Lap Length: {totalDistanceText / Convert.ToInt32(lapsText)},\n" +
            //    $" Minimum Speed: {minSpeedText}, Checkpoints: {checkpointsText} \n" +
            //    $" Restricted to Class: {restrictedClass}, Restricted to Vehicle: {restrictedCar}");
            // Validate inputs and show appropriate error messages
            //if (!ValidateInputs(trackName, raceType, totalDistanceText, lapsText, minSpeedText, checkpointsText))
            //    return;

            // Parse numeric inputs
            double totalDistance = totalDistanceText;
            int laps = ParseInt(lapsText, "Number of Laps", LapsTextBox);
            int minSpeed = ParseInt(minSpeedText, "Minimum Speed", MinSpeedTextBox);
            int checkpoints = ParseInt(checkpointsText, "Checkpoints", CheckpointsTextBox);

            if (totalDistance == -1 || laps == -1 || minSpeed == -1 || checkpoints == -1)
                return;

            // Calculate lap length if applicable
            double lapLength = CalculateLapLength(raceType, totalDistance, laps);

            // Insert the event into the database
            InsertTrackIntoDatabase(trackName, raceType, totalDistance, lapLength, laps, minSpeed, checkpoints, restrictedClass, restrictedCar);
            LastEntry.Text = $"{trackName}, {raceType}, {totalDistance}";
            // Inform the user of success and clear input fields
            LastEntry.Text = $" Success: " + LastEntry.Text;
            //MessageBox.Show("Event added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            RaceTypeComboBox.Focus();
        }

        // Updated InsertTrackIntoDatabase to include the restricted class and car
        private void InsertTrackIntoDatabase(string trackName, string raceType, double totalDistance, double lapLength, int laps, int minSpeed, int checkpoints, string restrictedClass, string restrictedCar)
        {
            try
            {
                using (var conn = new SQLiteConnection(Settings.Default.connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO tracks (Name, Length, RaceType, Laps, MinSpeed, Checkpoints, RestrictedClass, RestrictedCar, Runs) VALUES (@name, @length, @type, @laps, @minSpeed, @checkpoints, @restrictedClass, @restrictedCar, @runs)";

                    using (var cmd = new SQLiteCommand(query, conn))
                    {
                        // Prepare parameters for the insert statement
                        cmd.Parameters.Add("@name", DbType.String).Value = trackName;
                        cmd.Parameters.Add("@type", DbType.String).Value = raceType;
                        cmd.Parameters.Add("@length", DbType.String).Value = Math.Round(totalDistance / laps,2);
                        cmd.Parameters.Add("@laps", DbType.Int32).Value = laps;
                        cmd.Parameters.Add("@minSpeed", DbType.Int32).Value = minSpeed;
                        cmd.Parameters.Add("@checkpoints", DbType.Int32).Value = checkpoints;
                        cmd.Parameters.Add("@restrictedClass", DbType.String).Value = !string.IsNullOrEmpty(restrictedClass) ? restrictedClass : (object)DBNull.Value;
                        cmd.Parameters.Add("@restrictedCar", DbType.String).Value = !string.IsNullOrEmpty(restrictedCar) ? restrictedCar : (object)DBNull.Value;
                        cmd.Parameters.Add("@runs", DbType.Int32).Value = 0;

                        // Execute the insert statement
                        cmd.ExecuteNonQuery();
                        ClearInputFields();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while adding the event: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);                
            }
        }


        private bool ValidateInputs(string trackName, string raceType, string totalDistanceText, string lapsText, string minSpeedText, string checkpointsText)
        {
            if (string.IsNullOrWhiteSpace(trackName))
            {
                ShowValidationError("Track Name must not be empty.", TrackNameTextBox);
                return false;
            }


            if (string.IsNullOrWhiteSpace(raceType))
            {
                ShowValidationError("Race Type must not be empty.", RaceTypeComboBox);
                return false;
            }


            if (DistanceGroupBox.Visibility == Visibility.Visible && string.IsNullOrWhiteSpace(totalDistanceText))
            {
                ShowValidationError("Total Race Distance must not be empty for this event type.", LapLengthTextBox);
                return false;
            }


            if (LapsGroupBox.Visibility == Visibility.Visible && string.IsNullOrWhiteSpace(lapsText))
            {
                ShowValidationError("Number of Laps must be specified for this event type.", LapsTextBox);
                return false;
            }


            if (SpeedGroupBox.Visibility == Visibility.Visible && string.IsNullOrWhiteSpace(minSpeedText))
            {
                ShowValidationError("Minimum Speed must be specified for Speed events.", MinSpeedTextBox);
                return false;
            }


            if (CheckpointsGroupBox.Visibility == Visibility.Visible && string.IsNullOrWhiteSpace(checkpointsText))
            {
                ShowValidationError("Checkpoints must be specified for Speed Trap events.", CheckpointsTextBox);
                return false;
            }


            return true;
        }


        private void ShowValidationError(string message, Control controlToFocus)
        {
            MessageBox.Show(message, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            controlToFocus.Focus();
        }

        private int ParseInt(string input, string fieldName, Control controlToFocus)
        {
            if (int.TryParse(input, out int result))
                return result;

            ShowValidationError($"{fieldName} must be a valid integer.", controlToFocus);
            return -1;
        }

        private double CalculateLapLength(string raceType, double totalDistance, int laps)
        {
            if (IsRaceOrEliminator(raceType) && laps > 0)
                return totalDistance / laps;
                
            return 0;
        }

        private bool RequiresDistance(string raceType)
        {
            return IsRaceOrEliminator(raceType) || raceType.Equals("Time Attack", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsRaceOrEliminator(string raceType)
        {
            return raceType.Equals("Race", StringComparison.OrdinalIgnoreCase) || raceType.Equals("Eliminator", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsSpeedEvent(string raceType)
        {
            return raceType.Equals("Speed", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsSpeedTrap(string raceType)
        {
            return raceType.Equals("Speed Trap", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Clears the input fields after successful addition.
        /// </summary>
        private void ClearInputFields()
        {
            TrackNameTextBox.Clear();
            LapLengthTextBox.Clear();
            LapsTextBox.Text = "1";
            MinSpeedTextBox.Text = "0";
            CheckpointsTextBox.Text = "0";
            RaceTypeComboBox.SelectedIndex = -1; // Reset selection
            RestrictedCarComboBox.SelectedIndex = -1;
            if (keep == 0) { 
            RestrictedClassComboBox.SelectedIndex = -1;
            }
            LapsGroupBox.Visibility = Visibility.Collapsed;
            DistanceGroupBox.Visibility = Visibility.Collapsed;
            SpeedGroupBox.Visibility = Visibility.Collapsed;
            CheckpointsGroupBox.Visibility = Visibility.Collapsed;
            RestrictedCarGroupBox.Visibility = Visibility.Collapsed;
            RestrictedClassGroupBox.Visibility = Visibility.Collapsed;
        }
        private void RaceTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RaceTypeComboBox.SelectedIndex == -1) return;
            ComboBoxItem selectedItem = (ComboBoxItem)RaceTypeComboBox.SelectedItem;
            string selectedRaceType = selectedItem.Value;

            // Adjust visibility based on the selected race type
            bool isRaceOrEliminator = selectedRaceType == "Race SP" || selectedRaceType == "Eliminator SP" || selectedRaceType == "Race MP";
            bool requiresDistance = isRaceOrEliminator || selectedRaceType == "Time Attack SP";
            bool isSpeedEvent = selectedRaceType == "Speed SP" || selectedRaceType == "Speed MP";
            bool isSpeedTrap = selectedRaceType == "Speed Trap SP" || selectedRaceType == "Speed Trap MP";

            // Define conditions for restricted class and vehicle
            bool needsRestrictedClass = selectedRaceType.EndsWith("SP") || selectedRaceType.EndsWith("MP"); 
            bool needsRestrictedVehicle = selectedRaceType.EndsWith("MP");

            // Show or hide group boxes based on the event type
            DistanceGroupBox.Visibility = requiresDistance ? Visibility.Visible : Visibility.Collapsed;
            LapsGroupBox.Visibility = isRaceOrEliminator ? Visibility.Visible : Visibility.Collapsed;
            SpeedGroupBox.Visibility = isSpeedEvent ? Visibility.Visible : Visibility.Collapsed;
            CheckpointsGroupBox.Visibility = isSpeedTrap ? Visibility.Visible : Visibility.Collapsed;

            // Show or hide the restricted class and car group boxes
            RestrictedClassGroupBox.Visibility = needsRestrictedClass ? Visibility.Visible : Visibility.Collapsed;
            RestrictedCarGroupBox.Visibility = needsRestrictedVehicle ? Visibility.Visible : Visibility.Collapsed;

            if (selectedRaceType.StartsWith("Elim")) { LapsTextBox.Text = "7"; } else { LapsTextBox.Text = "1"; }

            // Bind vehicles to the RestrictedCarComboBox if needed
            if (needsRestrictedClass)
            {
                BindVehicleComboBox(RestrictedCarComboBox);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            // Any cleanup or additional actions when the window is closed
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e is null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void RestrictedClassComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BindVehicleComboBox(RestrictedCarComboBox);
        }

        private void KeepCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (KeepCheckBox.IsChecked == false)
            {
                keep = 0;
            }
            else
            {
                keep = 1;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var eventArgs = new RoutedEventArgs();
                // Call the AddTrackButton_Click method
                AddTrackButton_Click(sender, eventArgs);
                e.Handled = true; // Prevent further processing if needed
            }
        }
    }
}
