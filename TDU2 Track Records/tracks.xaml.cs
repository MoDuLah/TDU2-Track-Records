using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TDU2_Track_Records.Properties;

namespace TDU2_Track_Records
{
    /// <summary>
    /// Interaction logic for tracks.xaml
    /// </summary>
    public partial class tracks : Window
    {
        public tracks()
        {
            InitializeComponent();
            List<ComboBoxItem> items = new List<ComboBoxItem>
            {
                new ComboBoxItem { ImagePath = "Images/ico/ta.png", Description = "Time Attack Cup", Value = "Time Attack" },
                new ComboBoxItem { ImagePath = "Images/ico/race2.png", Description = "Race Cup", Value = "Race" },
                new ComboBoxItem { ImagePath = "Images/ico/speed2.png", Description = "Speed Cup", Value = "Speed" },
                new ComboBoxItem { ImagePath = "Images/ico/st2.png", Description = "Speed Trap Cup", Value = "Speed Trap" },
                new ComboBoxItem { ImagePath = "Images/ico/eliminator.png", Description = "Eliminator Cup", Value = "Eliminator" },
                new ComboBoxItem { ImagePath = "Images/ico/ftl.png", Description = "Follow The Leader", Value = "Follow The Leader" },
                new ComboBoxItem { ImagePath = "Images/ico/kyd.png", Description = "Keep Your Distance", Value = "Keep Your Distance" },
                new ComboBoxItem { ImagePath = "Images/ico/race.png", Description = "Race", Value = "Race" },
                new ComboBoxItem { ImagePath = "Images/ico/speed.png", Description = "Speed", Value = "Speed" },
                new ComboBoxItem { ImagePath = "Images/ico/st.png", Description = "Speed Trap", Value = "Speed Trap" }
               // new ComboBoxItem { ImagePath = "Images/carClasses/MA2.png", Description = "Motorcycles 2", Value = "MA2" }
            };

            RaceTypeComboBox.ItemsSource = items;
        }
        private void AddTrackButton_Click(object sender, RoutedEventArgs e)
        {
            string trackName = TrackNameTextBox.Text;
            string lapLength = LapLengthTextBox.Text;
            string raceType = RaceTypeComboBox.SelectedValue as string;
            string laps = LapsTextBox.Text;

            if (string.IsNullOrWhiteSpace(trackName) || string.IsNullOrWhiteSpace(lapLength) || string.IsNullOrWhiteSpace(raceType))
            {
                MessageBox.Show("Track Name, Lap Length, and Race Type must not be empty.");
                return;
            }

            // Validate laps only if the race type is 'Race'
            if (raceType == "Race" && string.IsNullOrWhiteSpace(laps))
            {
                MessageBox.Show("Number of Laps must be specified for a Race.");
                return;
            }

            using (var conn = new SQLiteConnection(Settings.Default.connectionString))
            {
                conn.Open();
                string query = "INSERT INTO tracks (Name, Length, RaceType, Laps, Runs) VALUES (@name, @length, @type, @laps, @runs)";
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@name", trackName);
                    cmd.Parameters.AddWithValue("@length", lapLength);
                    cmd.Parameters.AddWithValue("@type", raceType);
                    cmd.Parameters.AddWithValue("@laps", raceType == "Race" ? (object)laps : DBNull.Value);
                    cmd.Parameters.AddWithValue("@runs", 0); // Initialize runs to 0
                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Track Added");
        }

        private void RaceTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selectedItem = (ComboBoxItem)RaceTypeComboBox.SelectedItem;
            string selectedRaceType = selectedItem.Value;

            MessageBox.Show(selectedRaceType);
            if (selectedRaceType == "Race")
            {
                LapsGroupBox.Visibility = Visibility.Visible;
            }
            else
            {
                LapsGroupBox.Visibility = Visibility.Collapsed;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
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
    }
}
