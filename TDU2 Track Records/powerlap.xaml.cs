using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TDU2_Track_Records.Properties;
using System.ComponentModel;

namespace TDU2_Track_Records
{
    public partial class powerlap : Window
    {
        private readonly string connectionString = Settings.Default.connectionString;
        private PowerLapViewModel viewModel;


        public powerlap()
        {
            InitializeComponent();
            ThemeManager.ApplyScopedControlStyles(this);
            viewModel = new PowerLapViewModel();
            DataContext = viewModel;
            List<ComboBoxItem> items = new List<ComboBoxItem>
            {
                new ComboBoxItem { ImagePath = "Images/carClasses/SC.png", Description = "All", Value = "All" },
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

            combo_Class.ItemsSource = items;
            combo_Class.SelectedIndex = 0;
            List<ComboBoxItem> sorts = new List<ComboBoxItem>
            {
                new ComboBoxItem { ImagePath = "Images/carClasses/SC.png", Description = "Car Name", Value = "carName" },
                new ComboBoxItem { ImagePath = "Images/carClasses/A1.png", Description = "Fastest Lap", Value = "Total_Time" },
                new ComboBoxItem { ImagePath = "Images/carClasses/A2.png", Description = "Average Lap", Value = "Average_Lap" }
            };

            combo_Sort.ItemsSource = sorts;

            combo_TextBlock.Visibility = Visibility.Collapsed;
            RefreshRecordedTracks();
        }

        private void Combo_Class_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selectedItem = combo_Class.SelectedItem as ComboBoxItem;
            if (selectedItem != null)
            {
                MessageBox.Show("Selected Value: " + selectedItem.Value);
            }
        }

        private bool LoadFastestLaps()
        {
            if (combo_Track.SelectedValue == null)
                return false;

            int trackId = Convert.ToInt32(combo_Track.SelectedValue);
            int conditions = cb_conditions.IsChecked == true ? 1 : 0;
            int orientation = cb_orientation.IsChecked == true ? 1 : 0;
            string carClass = (combo_Class.SelectedItem as ComboBoxItem)?.Value ?? "All";

            var lapDataAccess = new LapDataAccess(connectionString);
            List<Lap> fastestLaps = lapDataAccess.GetFastestLapForEachCar(trackId, conditions, orientation, carClass);
            LapsListBox.ItemsSource = fastestLaps;
            if (fastestLaps.Count == 0)
            {
                PowerLapStatus.Text = "No lap times for this class, weather and direction.";
                return false;
            }

            PowerLapStatus.Text = string.Empty;
            viewModel.WeatherImageSource = conditions == 0 ? "/Images/sun.png" : "/Images/rain.png";
            viewModel.IsWeatherImageVisible = true;
            viewModel.OrientationImageSource = orientation == 0 ? "/Images/clockwise.png" : "/Images/anticlockwise.png";
            viewModel.IsOrientationImageVisible = true;
            return true;
        }

        private void cb_conditions_StateChanged(object sender, RoutedEventArgs e)
        {
            if (cb_conditions.IsChecked == false)
            {
                cb_weather.IsChecked = false;
            }
            else
            {
                cb_weather.IsChecked = true;
            }
        }
        private void cb_orientation_StateChanged(object sender, RoutedEventArgs e)
        {
            if (cb_orientation.IsChecked == false)
            {
                txt_orientation.IsChecked = false;
            }
            else
            {
                txt_orientation.IsChecked = true;
            }
        }

        private void cb_weather_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (cb_conditions.IsChecked == false)
            {
                cb_conditions.IsChecked = true;
            }
            else
            {
                cb_conditions.IsChecked = false;
            }

        }

        private void txt_orientation_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (cb_orientation.IsChecked == false)
            {
                cb_orientation.IsChecked = true;
            }
            else
            {
                cb_orientation.IsChecked = false;
            }

        }
        private void cb_weather_Click(object sender, RoutedEventArgs e)
        {
            if (cb_conditions.IsChecked == false)
            {

                cb_conditions.IsChecked = true;
            }
            else
            {
                cb_conditions.IsChecked = false;
            }
        }

        private void txt_orientation_Click(object sender, RoutedEventArgs e)
        {
            if (cb_orientation.IsChecked == false)
            {

                cb_orientation.IsChecked = true;
            }
            else
            {
                cb_orientation.IsChecked = false;
            }
        }

        private void combo_Button_Click(object sender, RoutedEventArgs e)
        {
            if (!(combo_Track.SelectedItem is DataRowView selectedTrack) ||
                !(combo_Class.SelectedItem is ComboBoxItem selectedClass) ||
                !LoadFastestLaps())
                return;

            combo_TextBlock.Content = new Image
            {
                Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(selectedClass.ImagePath, UriKind.Relative)),
                Width = 40,
                Height = 40
            };
            trackName.Text = "@" + selectedTrack["Name"];
            combo_TextBlock.Visibility = Visibility.Visible;
            cb_weather.IsEnabled = false;
            txt_orientation.IsEnabled = false;
            grp.Visibility = Visibility.Collapsed;
        }

        private void TrackComboBox_DropDownOpened(object sender, EventArgs e)
        {
            RefreshRecordedTracks();
        }

        private void RefreshRecordedTracks()
        {
            const string query = @"
                SELECT t.id, t.Name
                FROM tracks t
                WHERE EXISTS (
                    SELECT 1 FROM records r
                    WHERE r.trackId = t.id AND " + LapDataAccess.TimedRecordFilter + @"
                )
                ORDER BY t.Name COLLATE NOCASE, t.id;";

            try
            {
                object selectedTrackId = combo_Track.SelectedValue;
                using (var connection = new SQLiteConnection(connectionString))
                using (var command = new SQLiteCommand(query, connection))
                using (var adapter = new SQLiteDataAdapter(command))
                {
                    connection.Open();
                    var tracks = new DataTable();
                    adapter.Fill(tracks);
                    combo_Track.DisplayMemberPath = "Name";
                    combo_Track.SelectedValuePath = "id";
                    combo_Track.ItemsSource = tracks.DefaultView;
                    combo_Track.SelectedValue = selectedTrackId;
                    combo_Button.IsEnabled = tracks.Rows.Count > 0;
                    PowerLapStatus.Text = tracks.Rows.Count == 0 ? "No timed laps saved yet." : string.Empty;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading tracks:\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LapsListBox_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            grp.Visibility = Visibility.Visible;
            combo_TextBlock.Visibility = Visibility.Collapsed;
            LapsListBox.ItemsSource = null;
            trackName.Text = "";
            PowerLapStatus.Text = combo_Track.Items.Count == 0 ? "No timed laps saved yet." : string.Empty;
            cb_weather.IsEnabled = true;
            txt_orientation.IsEnabled = true;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

    }

    public class ComboBoxItem
    {
        public string ImagePath { get; set; }
        public string Description { get; set; }
        public string Value { get; set; }
    }
    public class Lap
    {
        public string carName { get; set; }
        public string LapTime { get; set; }
        public string carClass { get; set; }
        public string WeatherConditions { get; set; }
        public string Orientation { get; set; }
 
    }

    public class LapDataAccess
    {
        private readonly string _connectionString;
        internal const string TimedRecordFilter = "TRIM(r.Total_Time) COLLATE NOCASE NOT IN ('', 'N/A')";

        public LapDataAccess(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<Lap> GetFastestLapForEachCar(int trackId, int conditions, int orientation, string carClass)
        {
            List<Lap> laps = new List<Lap>();
            try
            {
                const string query = @"
                    SELECT r.carName, MIN(r.Total_Time) AS Fastest_Time, r.carClass, r.conditions, r.orientation
                    FROM records r
                    WHERE r.trackId = @TrackId AND r.conditions = @Conditions AND r.orientation = @Orientation
                      AND (@CarClass = 'All' OR r.carClass = @CarClass)
                      AND " + TimedRecordFilter + @"
                    GROUP BY r.carName, r.carClass, r.conditions, r.orientation
                    ORDER BY Fastest_Time ASC;";

                using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@TrackId", trackId);
                        command.Parameters.AddWithValue("@CarClass", carClass);
                        command.Parameters.AddWithValue("@Conditions", conditions);
                        command.Parameters.AddWithValue("@Orientation", orientation);

                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Lap lap = new Lap
                                {
                                    carName = reader["carName"].ToString(),
                                    carClass = reader["carClass"].ToString(),
                                    LapTime = reader["Fastest_Time"].ToString(),
                                    WeatherConditions = reader["conditions"].ToString(),
                                    Orientation = reader["orientation"].ToString()
                                };
                                laps.Add(lap);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return laps;
        }

    }
    public class PowerLapViewModel : INotifyPropertyChanged
    {
        private string _weatherImageSource;
        private string _orientationImageSource;
        private bool _isWeatherImageVisible;
        private bool _isOrientationImageVisible;

        public string WeatherImageSource
        {
            get => _weatherImageSource;
            set
            {
                _weatherImageSource = value;
                OnPropertyChanged(nameof(WeatherImageSource));
            }
        }

        public string OrientationImageSource
        {
            get => _orientationImageSource;
            set
            {
                _orientationImageSource = value;
                OnPropertyChanged(nameof(OrientationImageSource));
            }
        }

        public bool IsWeatherImageVisible
        {
            get => _isWeatherImageVisible;
            set
            {
                _isWeatherImageVisible = value;
                OnPropertyChanged(nameof(IsWeatherImageVisible));
            }
        }

        public bool IsOrientationImageVisible
        {
            get => _isOrientationImageVisible;
            set
            {
                _isOrientationImageVisible = value;
                OnPropertyChanged(nameof(IsOrientationImageVisible));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
