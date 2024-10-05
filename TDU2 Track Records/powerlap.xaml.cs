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
            viewModel = new PowerLapViewModel();
            DataContext = viewModel;
            Fill(combo_Track);
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
            List<ComboBoxItem> sorts = new List<ComboBoxItem>
            {
                new ComboBoxItem { ImagePath = "Images/carClasses/SC.png", Description = "Car Name", Value = "carName" },
                new ComboBoxItem { ImagePath = "Images/carClasses/A1.png", Description = "Fastest Lap", Value = "Total_Time" },
                new ComboBoxItem { ImagePath = "Images/carClasses/A2.png", Description = "Average Lap", Value = "Average_Lap" }
            };

            combo_Sort.ItemsSource = sorts;

            combo_TextBlock.Visibility = Visibility.Collapsed;
            //combo_Track.SelectionChanged += Combo_Track_SelectionChanged;
        }

        private void Combo_Class_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selectedItem = combo_Class.SelectedItem as ComboBoxItem;
            if (selectedItem != null)
            {
                MessageBox.Show("Selected Value: " + selectedItem.Value);
            }
        }

        private void LoadFastestLaps()
        {
            if (combo_Track.SelectedIndex == -1) return;
            if (combo_Track.SelectedValue != null)
            {
                // Get the selected track ID
                int trackId = Convert.ToInt32(combo_Track.SelectedValue);

                // Determine the conditions and orientation based on the checkboxes
                int conditions = cb_conditions.IsChecked == true ? 1 : 0;
                int orientation = cb_orientation.IsChecked == true ? 1 : 0;

                // Get the selected car class, if any
                string carClass;
                if (combo_Class.SelectedIndex >= 0)
                {
                    var selectedItem = combo_Class.SelectedItem as ComboBoxItem;
                    carClass = selectedItem?.Value.ToString() ?? "*";
                }
                else
                {
                    carClass = "*";
                }

                LapDataAccess lapDataAccess = new LapDataAccess(connectionString);

                // Get the fastest lap for each car
                List<Lap> fastestLaps = lapDataAccess.GetFastestLapForEachCar(trackId, conditions, orientation, carClass);

                LapsListBox.ItemsSource = fastestLaps;

                // Update ViewModel based on conditions
                foreach (var lap in fastestLaps)
                {
                    string wc, lo;

                    if (lap.WeatherConditions == "0")
                    {
                        viewModel.WeatherImageSource = "/Images/sun.png"; // Example path
                        viewModel.IsWeatherImageVisible = true;
                        wc = viewModel.WeatherImageSource.ToString();
                    }
                    else
                    {
                        viewModel.WeatherImageSource = "/Images/rain.png"; // Hide weather image
                        viewModel.IsWeatherImageVisible = true;
                        wc = viewModel.WeatherImageSource.ToString();
                    }
                    if (lap.Orientation == "0")
                    {
                        viewModel.OrientationImageSource = "/Images/clockwise.png";
                        viewModel.IsOrientationImageVisible = true;
                        lo = viewModel.OrientationImageSource.ToString();
                    }
                    else
                    {
                        viewModel.OrientationImageSource = "/Images/anticlockwise.png";
                        viewModel.IsOrientationImageVisible = true;
                        lo = viewModel.OrientationImageSource.ToString();
                    }
                }
            }
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
            if(combo_Class.SelectedIndex == -1) { return; }
            //// Load the fastest laps based on the selected track
            LoadFastestLaps();

            // Get the selected item from the ComboBox
            var selectedTrack = combo_Track.SelectedItem;
            var selectedItem = combo_Class.SelectedItem as ComboBoxItem;
            string selectedClassText = "";
            if (selectedTrack != null)
            {
                // Assuming the selected item is a DataRowView (or similar) and "Name" is the column you want
                var selectedTrackText = (selectedTrack as DataRowView)?["Name"].ToString();
                if (combo_Class.SelectedIndex >= 0)
                {
                    selectedClassText = selectedItem?.Value.ToString() ?? "All";
                }
                else
                {
                    selectedClassText = "";
                }

                // Create an Image control to display the selected image
                var imageControl = new Image
                {
                    Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(selectedItem.ImagePath, UriKind.Relative)),
                    Width = 40,
                    Height = 40
                };

                // Update the ContentControl with the selected image
                trackName.Text = "@" + selectedTrackText;
                combo_TextBlock.Content = imageControl;

                cb_weather.IsEnabled = false;
                txt_orientation.IsEnabled = false;
                combo_TextBlock.Visibility = Visibility.Visible;
                grp.Visibility = Visibility.Collapsed;
            }
            else
            {
                // Handle case where no item is selected, if necessary
                combo_TextBlock.Visibility = Visibility.Collapsed;
                cb_weather.IsEnabled = true;
                txt_orientation.IsEnabled = true;
            }

            // Make the TextBlock visible and collapse the group box

        }

        private void Fill(ComboBox comboBox)
        {
            using (SQLiteConnection dbConn = new SQLiteConnection(connectionString))
            {
                dbConn.Open();
                using (SQLiteCommand dbCmd = new SQLiteCommand("SELECT id, Name FROM tracks", dbConn))
                {
                    try
                    {
                        using (SQLiteDataAdapter dbAdapter = new SQLiteDataAdapter(dbCmd))
                        {
                            DataSet ds = new DataSet();
                            dbAdapter.Fill(ds, "tracks");

                            comboBox.ItemsSource = ds.Tables[0].DefaultView;
                            comboBox.DisplayMemberPath = "Name"; // Display the 'Name' column
                            comboBox.SelectedValuePath = "id"; // Use the 'id' column as the value
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An error occurred while loading tracks:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void LapsListBox_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            grp.Visibility = Visibility.Visible;
            combo_TextBlock.Visibility = Visibility.Collapsed;
            LapsListBox.ItemsSource = "";
            trackName.Text = "";
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

        public LapDataAccess(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<Lap> GetFastestLapForEachCar(int trackId, int conditions, int orientation, string carClass)
        {
            List<Lap> laps = new List<Lap>();
            try
            {
                string query;

                if (carClass == "All")
                {
                    query = $"SELECT carName, MIN(Total_Time) AS Fastest_Time, carClass, conditions, orientation " +
                            $"FROM records WHERE trackId = @TrackId AND conditions = @Conditions AND orientation = @Orientation " +
                            $"GROUP BY carName, carClass, conditions, orientation ORDER BY Fastest_Time ASC";
                }
                else
                {
                    query = $"SELECT carName, MIN(Total_Time) AS Fastest_Time, carClass, conditions, orientation " +
                            $"FROM records WHERE trackId = @TrackId AND carClass = @CarClass AND conditions = @Conditions AND orientation = @Orientation " +
                            $"GROUP BY carName, carClass, conditions, orientation ORDER BY Fastest_Time ASC";
                }

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