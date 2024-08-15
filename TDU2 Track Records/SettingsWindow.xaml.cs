using System;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using TDU2_Track_Records.Properties;

namespace TDU2_Track_Records
{
    public partial class SettingsWindow : Window
    {
        public delegate void MeasurementSystemChangedEventHandler(object sender, EventArgs e);
        public event MeasurementSystemChangedEventHandler MeasurementSystemChanged;
        public SettingsWindow()
        {
            InitializeComponent();
            LoadSettings();
            UnitSlider.ValueChanged += UnitSlider_ValueChanged;
        }

        private void LoadSettings()
        {
            OpacitySlider.Value = Settings.Default.MainWindowOpacity;
            UnitSlider.Value = Settings.Default.system == "Imperial" ? 1 : 0;
        }

        private void UnitSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int sliderValue = (int)UnitSlider.Value;
            if (e.NewValue == 1)
            {
                Settings.Default.system = "Imperial";
                Settings.Default.speed = "mph";
                Settings.Default.weight = "lbs";
                Settings.Default.distance = "mi";
                Settings.Default.torque = "lb⋅ft";
            }
            else
            {
                Settings.Default.system = "Metric";
                Settings.Default.speed = "km/h";
                Settings.Default.weight = "kg";
                Settings.Default.distance = "km";
                Settings.Default.torque = "N⋅m";
            }
            Settings.Default.Save();
            // Close the main window and reopen it
            Application.Current.MainWindow.Close();
            MainWindow mainWindow = new MainWindow();
            Application.Current.MainWindow = mainWindow;
            mainWindow.Show();
        }

        private void AddTrackButton_Click(object sender, RoutedEventArgs e)
        {
            string trackName = TrackNameTextBox.Text;
            string lapLength = LapLengthTextBox.Text;

            if (string.IsNullOrWhiteSpace(trackName) || string.IsNullOrWhiteSpace(lapLength))
            {
                MessageBox.Show("Track Name and Lap Length must not be empty.");
                return;
            }

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

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void ChangeMeasurementSystem()
        {
            // Assuming 'MainWindow' is the owner or a reference to the main window
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.UpdateMeasurementSystem();
            MeasurementSystemChanged?.Invoke(this, EventArgs.Empty);

            // Or if you passed the main window as a reference to the settings window
            // mainWindowReference.UpdateMeasurementSystem();
        }
    }
}
