using System;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using TDU2_Track_Records.Properties;

namespace TDU2_Track_Records
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {

        public SettingsWindow()
        {
            InitializeComponent();

            LoadSettings();
            if (Settings.Default.Language == "English")
            {
                LanguageComboBox.SelectedIndex = 0;
            }
            else
            {
                LanguageComboBox.SelectedIndex = 1;
            }
            if (Settings.Default.system == "Metric")
            {
                UnitComboBox.SelectedIndex = 0;
            }
            else
            {
                UnitComboBox.SelectedIndex = 1;
            }
        }

        private void LoadSettings()
        {
            // Load existing settings
            OpacitySlider.Value = Settings.Default.MainWindowOpacity;
            LanguageComboBox.SelectedValue = Settings.Default.Language;
            UnitComboBox.SelectedValue = Settings.Default.system;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (UnitComboBox.Text == "Imperial")
                {
                    Settings.Default.distance = "mi";
                    Settings.Default.speed = "mph";
                    Settings.Default.system = "Imperial";
                }
                else
                {
                    Settings.Default.distance = "m";
                    Settings.Default.speed = "km/h";
                    Settings.Default.system = "Metric";
                }
                Settings.Default.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while saving settings: {ex.Message}");
            }
        
            Settings.Default.MainWindowOpacity = OpacitySlider.Value;
            if (LanguageComboBox.SelectedIndex >= 0) { 
            Settings.Default.Language = LanguageComboBox.SelectedValue.ToString();
            }
            Settings.Default.Save();
            MessageBox.Show("Settings Saved");
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void AddCarButton_Click(object sender, RoutedEventArgs e)
        {
            string name = CarNameTextBox.Text;
            string carClass = CarClassTextBox.Text;
            bool isActive = CarActiveCheckBox.IsChecked.GetValueOrDefault();

            using (var conn = new SQLiteConnection(Settings.Default.connectionString))
            {
                conn.Open();
                string query = "INSERT INTO cars (Name, Class, Active) VALUES (@name, @class, @active)";
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@class", carClass);
                    cmd.Parameters.AddWithValue("@active", isActive);
                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Car Added");
        }

        private void AddTrackButton_Click(object sender, RoutedEventArgs e)
        {
            string trackName = TrackNameTextBox.Text;
            string lapLength = LapLengthTextBox.Text;

            using (var conn = new SQLiteConnection(Settings.Default.connectionString))
            {
                conn.Open();
                string query = "INSERT INTO tracks (Name, Length) VALUES (@name, @length)";
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@name", trackName);
                    cmd.Parameters.AddWithValue("@length", lapLength);
                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Track Added");
        }

        private void UnitComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UnitComboBox.Text == "Imperial")
            {
                Settings.Default.distance = "mi";
                Settings.Default.speed = "mph";
                Settings.Default.system = "Imperial";
            }
            else
            {
                Settings.Default.distance = "m";
                Settings.Default.speed = "km/h";
                Settings.Default.system = "Metric";
            }
            Settings.Default.Save();
        }
    }
}
