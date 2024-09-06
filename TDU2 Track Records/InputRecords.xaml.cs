using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TDU2_Track_Records.Properties;

namespace TDU2_Track_Records
{
    /// <summary>
    /// Interaction logic for InputRecords.xaml
    /// </summary>
    public partial class InputRecords : Window
    {
        public int RecordsOn = 0;
        public int Popup = 0;
        readonly string connectionString = Settings.Default.connectionString;
        public string distance = Settings.Default.distance;
        public string speed = Settings.Default.speed;
        readonly string SI = Settings.Default.system;
        private static double lastTracksLeft;
        private static double lastTracksTop;
        private static readonly Regex _regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
        //SQLiteDataReader reader;
        ////SQLiteConnection dbConn; // Declare the SQLiteConnection-Object
        //SQLiteCommand dbCmd;
        public double onemile = 0.621371192;

        public InputRecords()
        {
            InitializeComponent();
            List<ComboBoxItem> items = new List<ComboBoxItem>
            {
                new ComboBoxItem { ImagePath = "Images/ico/events/ico_cup_eliminator.png", Description = "Eliminator Cup", Value = "Eliminator SP" },
                new ComboBoxItem { ImagePath = "Images/ico/events/ico_cup_race.png", Description = "Race Cup", Value = "Race SP" },
                new ComboBoxItem { ImagePath = "Images/ico/events/ico_cup_speed.png", Description = "Speed Cup", Value = "Speed SP" },
                new ComboBoxItem { ImagePath = "Images/ico/events/ico_cup_speedtrap.png", Description = "Speed Trap Cup", Value = "Speed Trap SP" },
                new ComboBoxItem { ImagePath = "Images/ico/events/ico_cup_timeattack.png", Description = "Time Attack Cup", Value = "Time Attack SP" },
                new ComboBoxItem { ImagePath = "Images/ico/events/ico_mp_ftl.png", Description = "Follow The Leader", Value = "Follow The Leader MP" },
                new ComboBoxItem { ImagePath = "Images/ico/events/ico_mp_kyd.png", Description = "Keep Your Distance", Value = "Keep Your Distance MP" },
                new ComboBoxItem { ImagePath = "Images/ico/events/ico_mp_race.png", Description = "Race Multiplayer", Value = "Race MP" },
                new ComboBoxItem { ImagePath = "Images/ico/events/ico_mp_speed.png", Description = "Speed Multiplayer", Value = "Speed MP" },
                new ComboBoxItem { ImagePath = "Images/ico/events/ico_mp_speedtrap.png", Description = "Speed Trap Multiplayer", Value = "Speed Trap MP" },
            };

            combo_Type.ItemsSource = items;

        }


        private void combo_Track_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void combo_Type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FillComboBoxWithTracks(combo_Track);
        }

        private void Btn_TrackInfo_Click(object sender, MouseButtonEventArgs e)
        {

        }

        private void cb_conditions_StateChanged(object sender, RoutedEventArgs e)
        {

        }
        private void cb_weather_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void cb_weather_Click(object sender, RoutedEventArgs e)
        {

        }

        private void cb_orientation_StateChanged(object sender, RoutedEventArgs e)
        {

        }

        private void toggle_orientation_Click(object sender, RoutedEventArgs e)
        {

        }

        private void toggle_orientation_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void VehicleManager_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Tracks_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var tracks = new tracks();

            // Set the position to the stored values, or default to the main window position if not set
            tracks.Left = lastTracksLeft != 0 ? lastTracksLeft : this.Left + this.Width + 10;
            tracks.Top = lastTracksTop != 0 ? lastTracksTop : this.Top + (this.Height - tracks.Height) / 2;

            tracks.Closed += tracks_Closed;
            tracks.ShowDialog();
        }

        private void tracks_Closed(object sender, EventArgs e)
        {
            var tracks = sender as Window;
            if (tracks != null)
            {
                lastTracksLeft = tracks.Left;
                lastTracksTop = tracks.Top;
            }
            FillComboBoxWithTracks(combo_Track);
        }
        private void FillComboBoxWithTracks(ComboBox comboBox)
        {
            ComboBoxItem selectedItem = (ComboBoxItem)combo_Type.SelectedItem;
            if (!string.IsNullOrEmpty(combo_Type.Text))
            { 
            string query = $"SELECT * FROM tracks WHERE RaceType = '{selectedItem.Value}'";
            ExecuteQuery(query, "tracks", dataSet =>
            {
                SetComboBoxSource(comboBox, dataSet, "Name", "id");
            },
            ex => MessageBox.Show($"An error occurred while loading tracks:\n{ex.Message}"));
            }
        }

        private void BindVehicleComboBox(ComboBox comboBox)
        {
            ComboBoxItem selectedItem = (ComboBoxItem)combo_Class.SelectedItem;
            string query = "SELECT * FROM vehicles WHERE _is_active = 'true' AND _is_owned = '1'";

            if (!string.IsNullOrEmpty(combo_Class.Text))
            {
                string selectedValue = selectedItem.Value;
                if (selectedValue != "All")
                {
                    query += $" AND Class = '{selectedValue}' ORDER BY Name ASC;";
                }
            }
            else
            {
                query += " ORDER BY Name ASC;";
            }

            ExecuteQuery(query, "vehicles", dataSet =>
            {
                if (dataSet.Tables[0].Rows.Count > 0)
                {
                    SetComboBoxSource(comboBox, dataSet, "_vehicle_name", "id");
                    //ClearVehicleDetails();
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
            ("@Class", combo_Class.Text));
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
                comboBox.DisplayMemberPath = displayMemberPath;
                comboBox.SelectedValuePath = selectedValuePath;
            }
            else
            {
                MessageBox.Show($"No data found in the {displayMemberPath} table.");
            }
        }
        private void Objectives_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Houses_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Dealership_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void PowerLaps_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var VehicleWindow = new Vehicle();
            VehicleWindow.Show();
        }

        private void Settings_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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

                settingsWindow.Left = this.Left + this.Width + 10;
                settingsWindow.Top = this.Top;
                settingsWindow.ShowDialog();
            }
            else
            {
                // User canceled the operation, do nothing
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

        private void combo_Class_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void combo_Vehicle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Btn_reset_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void keep_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
