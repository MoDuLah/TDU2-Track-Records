using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Reflection;
using TDU2_Track_Records.Models;
using TDU2_Track_Records.Properties;
using System.Security.AccessControl;


namespace TDU2_Track_Records
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public int RecordsOn = 0;
        public int Popup = 0;
        int isCalculated = 0;
        //int vehicle = 0;
        readonly string connectionString = Settings.Default.connectionString;
        public string distance = Settings.Default.distance;
        public string speed = Settings.Default.speed;
        readonly string SI = Settings.Default.system;
        // Dictionary to store last positions for different windows
        private Dictionary<string, (double Left, double Top)> lastWindowPositions = new Dictionary<string, (double Left, double Top)>();
        private static readonly Regex _regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
        //SQLiteDataReader reader;
        //SQLiteCommand dbCmd;
        public double onemile = 0.621371192;

        public MainWindow()
        {

            InitializeComponent();
            ShowAssemblyVersion(); // Call this after InitializeComponent()

            if (SI == "Metric")
            {
                btn_loadrec_Metric.Visibility = Visibility.Collapsed;
                ViewEntries_Metric.Visibility = Visibility.Collapsed;
                btn_loadrec_Imperial.Visibility = Visibility.Collapsed;
                ViewEntries_Imperial.Visibility = Visibility.Collapsed;
            }
            else
            {
                btn_loadrec_Imperial.Visibility = Visibility.Collapsed;
                ViewEntries_Imperial.Visibility = Visibility.Collapsed;
                btn_loadrec_Metric.Visibility = Visibility.Collapsed;
                ViewEntries_Metric.Visibility = Visibility.Collapsed;
            }
            // Bind the TextBox to the TextBlock
            Binding binding = new Binding
            {
                Source = LapsTextBlock,
                Path = new PropertyPath("Text"),
                Mode = BindingMode.OneWay // TextBox will only receive text from the TextBlock
            };
            LapsTextBox.SetBinding(TextBox.TextProperty, binding);
            // bind ends here

            BindRaceTypeComboBox(combo_Type);
            BindVehicleComboBox(combo_Vehicle);
            calc_Total_Odometer();
            HideBoxes();

        }
        private void BindRaceTypeComboBox(ComboBox comboBox)
        {
            List<ComboBoxItem> raceTypes = new List<ComboBoxItem>
        {
                new ComboBoxItem { ImagePath = "Images/ico/events/ico_cup_race.png", Description = "Race Cup", Value = "Race SP" },
                new ComboBoxItem { ImagePath = "Images/ico/events/ico_cup_speed.png", Description = "Speed Cup", Value = "Speed SP" },
                new ComboBoxItem { ImagePath = "Images/ico/events/ico_cup_eliminator.png", Description = "Eliminator Cup", Value = "Eliminator SP" },
                new ComboBoxItem { ImagePath = "Images/ico/events/ico_cup_speedtrap.png", Description = "Speed Trap Cup", Value = "Speed Trap SP" },
                new ComboBoxItem { ImagePath = "Images/ico/events/ico_cup_timeattack.png", Description = "Time Attack Cup", Value = "Time Attack SP" },
                new ComboBoxItem { ImagePath = "Images/ico/events/ico_mp_race.png", Description = "Race Multiplayer", Value = "Race MP" },
                new ComboBoxItem { ImagePath = "Images/ico/events/ico_mp_race.png", Description = "Ranked Race", Value = "Race RMP" },
                new ComboBoxItem { ImagePath = "Images/ico/events/ico_mp_speed.png", Description = "Speed Multiplayer", Value = "Speed MP" },
                new ComboBoxItem { ImagePath = "Images/ico/events/ico_mp_ftl.png", Description = "Follow The Leader", Value = "Follow The Leader MP" },
                new ComboBoxItem { ImagePath = "Images/ico/events/ico_mp_kyd.png", Description = "Keep Your Distance", Value = "Keep Your Distance MP" },
                new ComboBoxItem { ImagePath = "Images/ico/events/ico_mp_speedtrap.png", Description = "Speed Trap Multiplayer", Value = "Speed Trap MP" }
  
        };

            comboBox.ItemsSource = raceTypes;
            comboBox.SelectedIndex = -1; // No race type selected initially
        }

        private void BindClassComboBox(ComboBox comboBox, string selectedRaceType)
        {
            List<ComboBoxItem> availableClasses = new List<ComboBoxItem>
    {
                new ComboBoxItem { ImagePath = "Images/carClasses/SC.png", Description = "All", Value = "" },
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
            comboBox.ItemsSource = availableClasses;
            comboBox.SelectedIndex = -1; // No class selected initially
        }
        private void combo_Type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            isCalculated = 0;
            HideAll();
            ComboBoxItem selectedType = (ComboBoxItem)combo_Type.SelectedItem;
            if (selectedType != null)
            {
                combo_Class.IsEnabled = true; // Enable class selection after race type is selected
                combo_Vehicle.SelectedIndex = -1;
                combo_Vehicle.IsEnabled = false;
                combo_Track.SelectedIndex = -1;
                combo_Track.IsEnabled = false;
                BindClassComboBox(combo_Class, selectedType.Value);
            }
        }
        private bool _isResetting = false; // Flag to prevent re-entrancy

        private void combo_Class_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isResetting)
                return; // Prevent re-entrancy if we're already resetting

            _isResetting = true;

            try
            {
                HideBoxes();
                //// Reset other controls
                if (combo_Track.SelectedIndex != -1)
                {
                    combo_Track.SelectedIndex = -1; // Only reset if necessary
                }
                if (combo_Vehicle.SelectedIndex != -1)
                {
                    combo_Vehicle.SelectedIndex = -1;
                }
                
                cb_conditions.IsChecked = false;
                keep.IsChecked = false;
                ResetRaceFields();
                // Arrays of TextBoxes for laps and average lap speeds
                lap_Length.Text = "";
                LapLengthGroupBox.Visibility = Visibility.Collapsed;
                race_Length.Text = "";
                RaceLength_Distance_Unit.Text = "";
                RaceLengthGroupBox.Visibility = Visibility.Collapsed;
                txt_odometer.Text = "";
                txt_lrec.Text = "";
                txt_lrecCar.Text = "";
                txt_rRan.Text = "";
                txt_Progress_1.Text = "";
                txt_Progress_2.Text = "";
                txt_carPB.Text = "";
                average_LapTime.Text = "";
                AverageRace_Speed.Text = "";

                ComboBoxItem selectedRaceType = (ComboBoxItem)combo_Type.SelectedItem;
                ComboBoxItem selectedClass = (ComboBoxItem)combo_Class.SelectedItem;

                if (selectedRaceType != null && selectedClass != null)
                {
                    string classValue = selectedClass.Value.ToString();

                    // Bind vehicles and tracks based on the selected class (or "All")
                    BindVehicleComboBox(combo_Vehicle, classValue);
                    BindTrackComboBox(combo_Track, selectedRaceType.Value.ToString(), classValue);

                    combo_Vehicle.IsEnabled = true;
                    combo_Track.IsEnabled = true;
                }
                else
                {
                    combo_Vehicle.IsEnabled = false;
                    combo_Track.IsEnabled = false;
                }
            }
            finally
            {
                _isResetting = false; // Ensure the flag is reset
            }
        }


        private void BindTrackComboBox(ComboBox comboBox, string selectedRaceType, string selectedClass)
        {
            string query;

            if (selectedClass == "All")
            {
                // If "All" is selected, ignore class restrictions and load all tracks for the race type
                query = $@"
                        SELECT * FROM tracks 
                        WHERE RaceType = '{selectedRaceType}';";
            }
            else
            {
                // Filter tracks based on the class restriction
                query = $@"
                        SELECT * FROM tracks 
                        WHERE RaceType = '{selectedRaceType}' 
                        AND (RestrictedClass IS NULL OR RestrictedClass = '' OR RestrictedClass = '{selectedClass}');";
            }

            // Execute the query and populate the ComboBox
            ExecuteQuery(query, "tracks", dataSet =>
            {
                SetComboBoxSource(comboBox, dataSet, "Name", "id");
            },
            ex => MessageBox.Show($"An error occurred while loading tracks:\n{ex.Message}"));
        }
        private void ShowAssemblyVersion()
        {
            try
            {
                // Get the assembly version
                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                string fullVersion = $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";

                // Set the TextBlock's text
                Currentversion.Text = $"TDU2 Track Records | Version: {fullVersion}";
            }
            catch (Exception ex)
            {
                // Handle any exception and display a fallback text
                Currentversion.Text = $"Error retrieving version: {ex.Message}";
            }
        }
        private void BindVehicleComboBox(ComboBox comboBox, string selectedClass = null)
        {
            string query;

            if (selectedClass == "All" || string.IsNullOrEmpty(selectedClass))
            {
                // If "All" is selected, show all vehicles
                query = $@"
            SELECT * FROM vehicles 
            WHERE _is_active = 'true' 
            OR _is_owned = 'true'
            ORDER BY _vehicle_name ASC;";
            }
            else
            {
                // Show only vehicles matching the selected class
                query = $@"
            SELECT * FROM vehicles 
            WHERE _vehiclecategory_name = '{selectedClass}'
            AND _is_active = 'true'
            AND _is_owned = 'true'
            ORDER BY _vehicle_name ASC;";
            }

            ExecuteQuery(query, "vehicles", dataSet =>
            {
                SetComboBoxSource(comboBox, dataSet, "_vehicle_name", "id");
            },
            ex => MessageBox.Show($"An error occurred while loading vehicles:\n{ex.Message}"));
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
        private void ClearVehicleDetails()
        {
            txt_odometer.Text = "";
            txt_carPB.Text = "";
            txt_rRan.Text = "";
        }
        private void calc_Total_Odometer()
        {
            string odo = SI == "Metric" ? "_odometer_metric" : "_odometer_imperial";
            string query = $"SELECT Sum({odo}) FROM [vehicles];";

            try
            {
                using (var dbConn = new SQLiteConnection(connectionString))
                {
                    dbConn.Open();
                    using (var dbCmd = new SQLiteCommand(query, dbConn))
                    {
                        using (var reader = dbCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                double value = reader.IsDBNull(0) ? 0 : Convert.ToDouble(reader[0]);
                                Total_Odometer.Text = $"{value}";
                                total_distance_unit.Visibility = Visibility.Visible;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex);
            }
            finally
            {
                if (keep.IsChecked == true)
                {
                    txt_odometer.Focus();
                }
                else
                {
                    combo_Track.Focus();
                }
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
        private void HideBoxes()
        {
            WeatherGroupBox.Visibility = Visibility.Collapsed;
            InfoGroupBox.Visibility = Visibility.Collapsed;
            TrackProgressGroupBox.Visibility = Visibility.Collapsed;
            RaceTimeGroupBox.Visibility = Visibility.Collapsed;
            avgLapGroupBox.Visibility = Visibility.Collapsed;
            ClassRestictionGroupBox.Visibility = Visibility.Collapsed;
            VehicleRestictionGroupBox.Visibility = Visibility.Collapsed;
            OrientationGroupBox.Visibility = Visibility.Collapsed;
            LapLengthGroupBox.Visibility = Visibility.Collapsed;
            LapsGroupBox.Visibility = Visibility.Collapsed;
            RaceLengthGroupBox.Visibility = Visibility.Collapsed;
            TrackRecordGroupbox.Visibility = Visibility.Collapsed;
            PointsGroupBox.Visibility = Visibility.Collapsed;
            MinimumSpeedGroupBox.Visibility = Visibility.Collapsed;
            CheckPointsGroupBox.Visibility = Visibility.Collapsed;
        }
        private void LapsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //ComboBoxItem selectedItem = (ComboBoxItem)combo_Type.SelectedItem;
            //string selectedRaceType = selectedItem?.Value;

            //switch (selectedRaceType)
            //{
            //    case "Race SP":
            //    case "Time Attack SP":
            //    case "Eliminator SP":

            //        RaceTimeGroupBox.Visibility = Visibility.Visible;
            //        avgLapGroupBox.Visibility = Visibility.Visible;
            //        RaceLengthGroupBox.Visibility = Visibility.Visible;
            //        LapsTextBlock.Visibility = Visibility.Visible;
            //        WeatherGroupBox.Visibility = Visibility.Visible;
            //        OrientationGroupBox.Visibility = Visibility.Visible;
            //        TrackProgressGroupBox.Visibility = Visibility.Visible;
            //        LapsGroupBox.Visibility = Visibility.Collapsed;
            //        LapsTextBox.Visibility = Visibility.Collapsed;
            //        break;

            //    case "Race MP":
            //        avgLapGroupBox.Visibility= Visibility.Visible;
            //        RaceTimeGroupBox.Visibility = Visibility.Visible;
            //        RaceLengthGroupBox.Visibility = Visibility.Visible;
            //        TrackProgressGroupBox.Visibility = Visibility.Visible;
            //        LapsTextBlock.Visibility = Visibility.Collapsed;
            //        LapsTextBox.Visibility = Visibility.Visible;
            //        WeatherGroupBox.Visibility = Visibility.Visible;
            //        OrientationGroupBox.Visibility = Visibility.Visible;
            //        break;

            //    case "Speed SP":
            //    case "Speed MP":
            //    case "Speed Trap SP":
            //    case "Speed Trap MP":
            //    case "Follow The Leader MP":
            //    case "Keep Your Distance MP":
            //        WeatherGroupBox.Visibility = Visibility.Visible;
            //        OrientationGroupBox.Visibility = Visibility.Collapsed;
            //        PointsGroupBox.Visibility = Visibility.Visible;
            //        TrackProgressGroupBox.Visibility = Visibility.Visible;
            //        RaceTimeGroupBox.Visibility = Visibility.Collapsed;
            //        avgLapGroupBox.Visibility = Visibility.Collapsed;
            //        LapsGroupBox.Visibility = Visibility.Collapsed;
            //        LapsTextBox.Visibility = Visibility.Collapsed;
            //        RaceLengthGroupBox.Visibility = Visibility.Collapsed;
            //        break;


            //    default:
            //        RaceTimeGroupBox.Visibility = Visibility.Collapsed;
            //        LapsGroupBox.Visibility = Visibility.Collapsed;
            //        LapsTextBox.Visibility = Visibility.Collapsed;
            //        TrackProgressGroupBox.Visibility = Visibility.Collapsed;
            //        WeatherGroupBox.Visibility = Visibility.Collapsed;
            //        OrientationGroupBox.Visibility = Visibility.Collapsed;
            //        break;
            //}
        }

        // GotFocus event handler
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            FextBox_GotFocus((TextBox)sender);
        }

        private void FextBox_GotFocus(TextBox textBox)
        {
            // Clear the "00" value when the text box gains focus
            if (textBox.Text == "00")
            {
                textBox.Text = "";
            }
        }

        // LostFocus event handler
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            FextBox_LostFocus((TextBox)sender);
        }

        private void FextBox_LostFocus(TextBox textBox)
        {

            if (textBox.Name.EndsWith("_Sec"))
            {
                // Ensure seconds are not greater than 59
                if (int.TryParse(textBox.Text, out int seconds) && seconds > 59)
                {
                    textBox.Text = "59";
                }
            }

            // Ensure the value has at least two digits (for Min/Sec), or three digits (for Ms)
            if (textBox.Name.EndsWith("_Min") || textBox.Name.EndsWith("_Sec"))
            {
                if (textBox.Text.Length == 1)
                {
                    textBox.Text = "0" + textBox.Text;
                }
            }
            else if (textBox.Name.EndsWith("_Ms"))
            {
                if (textBox.Text.Length == 2)
                {
                    textBox.Text = "0" + textBox.Text;
                }
            }
        }


        private void MyTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Auto-tab when maxlength is reached
            if (((TextBox)sender).MaxLength == ((TextBox)sender).Text.Length)
            {
                // move focus
                var ue = e.OriginalSource as FrameworkElement;
                e.Handled = true;
                ue.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }
        private void HandleLapMsPreviewLostKeyboardFocus(TextBox minTextBox, TextBox secTextBox, TextBox msTextBox, TextBlock avgSpeedTextBlock)
        {
            if (string.IsNullOrEmpty(minTextBox.Text) || string.IsNullOrEmpty(secTextBox.Text) || string.IsNullOrEmpty(msTextBox.Text))
                return;

            // Get the race length (numeric value) and trim any extra spaces
            string raceLengthText = race_Length.Text.Trim();
            // Ensure the race length is a valid number using culture-invariant parsing
            if (!double.TryParse(raceLengthText, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double totalRaceDistance))
            {
                // If parsing fails, set avgSpeedTextBlock to indicate an error
                avgSpeedTextBlock.Text = "Invalid distance";
                return;
            }

            // Convert the race time to milliseconds
            int totalMs = ConvertToMilliseconds(minTextBox.Text, secTextBox.Text, msTextBox.Text);

            if (totalMs > 0)
            {
                // Convert milliseconds to hours, then calculate speed (distance / time)
                double hours = totalMs / (1000.0 * 60 * 60); // Convert milliseconds to hours
                double averageSpeed = Math.Round(totalRaceDistance / hours, 2); // Distance divided by time (in hours)
                avgSpeedTextBlock.Text = averageSpeed.ToString();
            }
        }

        // Handler to ensure only numbers are accepted
        private void PreviewNoDecInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private bool IsTextAllowed(string text)
        {
            // Check if the input is numeric
            return Regex.IsMatch(text, "^[0-9]+$");
        }

        private void ApplyRestrictions(string classRestriction, string vehicleRestriction)
        {
            // Handle class restriction
            if (!string.IsNullOrEmpty(classRestriction))
            {
                ClassRestictionGroupBox.Visibility = Visibility.Visible;
                ClassRestrictionTextBlock.Text = classRestriction;
                combo_Class.SelectedValuePath = "Value";
                combo_Class.SelectedValue = classRestriction;
                //combo_Class.IsEnabled = false;
            }
            else
            {
                ClassRestictionGroupBox.Visibility = Visibility.Collapsed;
                combo_Class.IsEnabled = true;
            }

            // Handle vehicle restriction
            if (!string.IsNullOrEmpty(vehicleRestriction))
            {
                VehicleRestictionGroupBox.Visibility = Visibility.Visible;
                VehicleRestrictionTextBlock.Text = vehicleRestriction;
                combo_Vehicle.SelectedValuePath = "Value";
                combo_Vehicle.SelectedValue = vehicleRestriction;
                BindVehicleComboBox(combo_Vehicle, vehicleRestriction);
                combo_Vehicle.IsEnabled = false;
            }
            else
            {
                VehicleRestictionGroupBox.Visibility = Visibility.Collapsed;
                combo_Vehicle.IsEnabled = true;
            }
        }
        private void DisplayTrackDetails(double length, int laps, double minSpeed, int checkpoints, int oriental)
        {
            if (SI == "Imperial")
            {
                length = Math.Round(length * onemile, 2);
                minSpeed = Math.Round(minSpeed * onemile, 2);
            }

            lap_Length.Text = length.ToString();
            MinimumSpeedTextBlock.Text = minSpeed.ToString() + speed;
            CheckpointsTextBlock.Text = checkpoints.ToString();

            if (oriental == 1)
            {
                cb_orientation.IsChecked = true;
            }
            else
            {
                cb_orientation.IsChecked = false;
            }

            if (laps > 0)
            {
                DisplayLapsAndRaceLength(length, laps);
            }
            else
            {
                HideLapsAndRaceLength();
            }
        }
        private void DisplayLapsAndRaceLength(double length, int laps)
        {
            LapsTextBlock.Text = laps.ToString();
            race_Length.Text = (length * laps).ToString();

            RaceLength_Distance_Unit.Visibility = string.IsNullOrEmpty(race_Length.Text) ? Visibility.Collapsed : Visibility.Visible;
            RaceLength_Distance_Unit.Text = distance;

            if (laps == 1)
            {
                lap_Length.Text = "N/A";
                LapsTextBlock.Text = "N/A";
                race_Length.Text = length.ToString();
                Lap_Distance_Unit.Visibility = Visibility.Collapsed;
                avgLapGroupBox.Visibility = Visibility.Collapsed;
            }
            else
            {
                Lap_Distance_Unit.Visibility = Visibility.Visible;
                Lap_Distance_Unit.Text = distance;
                avgLapGroupBox.Visibility = Visibility.Visible;
            }

            LapsGroupBox.Visibility = Visibility.Visible;
            RaceLengthGroupBox.Visibility = Visibility.Visible;
            TrackRecordGroupbox.Visibility = Visibility.Visible;
        }
        private void HideLapsAndRaceLength()
        {
            LapsGroupBox.Visibility = Visibility.Collapsed;
            RaceLengthGroupBox.Visibility = Visibility.Collapsed;
            TrackRecordGroupbox.Visibility = Visibility.Collapsed;
        }
        private int ConvertToMilliseconds(string minutes, string seconds, string milliseconds)
        {
            return (Convert.ToInt32(minutes) * 60 * 1000) +
                   (Convert.ToInt32(seconds) * 1000) +
                   Convert.ToInt32(milliseconds);
        }
        private void calc_Average_Lap_Time()
        {
            if (string.IsNullOrEmpty(LapsTextBox.Text) || LapsTextBox.Text == "N/A") { return; }
            // Collection of tuples representing lap time controls (minutes, seconds, milliseconds)
            var lapTimeControls = new List<(TextBox Min, TextBox Sec, TextBox Ms)>
            {
                (Race_Min, Race_Sec, Race_Ms)
            };

            int totalLapTime = 0;
            int lapCount = Convert.ToInt32(LapsTextBox.Text);

            foreach (var (min, sec, ms) in lapTimeControls)
            {
                if (string.IsNullOrEmpty(min.Text) || string.IsNullOrEmpty(sec.Text) || string.IsNullOrEmpty(ms.Text)) return;
                totalLapTime += ConvertToMilliseconds(min.Text, sec.Text, ms.Text);
            }

            int averageLapTime = totalLapTime / lapCount;

            TimeSpan t = TimeSpan.FromMilliseconds(averageLapTime);
            string answer = string.Format("{0:D2}:{1:D2}.{2:D3}", t.Minutes, t.Seconds, t.Milliseconds);

            if (answer != "00:00.000")
            {
                average_LapTime.Text = answer;
            }

        }


        // Call this method to reset all controls
        private void ResetControls(DependencyObject parent)
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
                else if (child is ComboBox comboBox)
                {
                    comboBox.SelectedIndex = -1;
                }
                else if (child is CheckBox checkBox)
                {
                    checkBox.IsChecked = false;
                }
                else if (child is RadioButton radioButton)
                {
                    radioButton.IsChecked = false;
                }
                else if (child is ListBox listBox)
                {
                    listBox.SelectedIndex = -1;
                }
                else if (child is DatePicker datePicker)
                {
                    datePicker.SelectedDate = null;
                }
                else if (child is PasswordBox passwordBox)
                {
                    passwordBox.Clear();
                }
                //else if(child is TextBlock textBlock)
                //{
                //    textBlock.Text = string.Empty;
                //}
                // Add more control types as needed

                // Recurse into child elements
                ResetControls(child);
            }
        }
        private void Btn_reset_Click(object sender, RoutedEventArgs e)
        {


            //// Reset other controls
            combo_Track.SelectedIndex = -1;
            combo_Class.SelectedIndex = -1;
            cb_conditions.IsChecked = false;
            combo_Vehicle.SelectedIndex = -1;
            keep.IsChecked = false;

            ResetControls(this);
            ResetRaceFields();
            // Arrays of TextBoxes for laps and average lap speeds
            isCalculated = 0;
            combo_Type.SelectedIndex = -1;
            lap_Length.Text = "";
            LapLengthGroupBox.Visibility = Visibility.Collapsed;
            race_Length.Text = "";
            RaceLength_Distance_Unit.Text = "";
            RaceLengthGroupBox.Visibility = Visibility.Collapsed;
            txt_odometer.Text = "";
            txt_lrec.Text = "";
            txt_lrecCar.Text = "";
            txt_rRan.Text = "";
            txt_Progress_1.Text = "";
            txt_Progress_2.Text = "";
            txt_carPB.Text = "";
            average_LapTime.Text = "";
            AverageRace_Speed.Text = "";

            // Set visibility and checked state based on measurement system
            if (SI == "Metric")
            {
                ViewEntries_Metric.Visibility = Visibility.Collapsed;
                btn_loadrec_Metric.IsChecked = false;
            }
            else
            {
                ViewEntries_Imperial.Visibility = Visibility.Collapsed;
                btn_loadrec_Imperial.IsChecked = false;
            }
        }
        private void Btn_submit_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateSelections())
                return;

            // Prepare units and other variables
            PrepareSI(out string odo, out string otherSiName, out double calcOtherSIOdometer,
                      out double calcOtherSIAVG, out string otherSIAVG, out string avg, out string epidia);

            if (string.IsNullOrEmpty(combo_Track.Text)) { return; }

            int trackId = Convert.ToInt32(combo_Track.SelectedValue);
            string raceType = ((ComboBoxItem)combo_Type.SelectedItem).Value; // Get race type
            string carClass = ((ComboBoxItem)combo_Class.SelectedItem).Value; // Get car class value
            double raceDistance = Convert.ToDouble(race_Length.Text); // Distance

            bool isLaps = LapsTextBlock.Text != "N/A";  // Check if lap count is visible
            double avgSpeed = Convert.ToDouble(AverageRace_Speed.Text);

            // Check if carClass is "All" and retrieve the actual car class from the database
            if (string.IsNullOrEmpty(carClass))
            {
                string vehicleName = combo_Vehicle.Text.Replace("'", "''");
                using (var dbConn = new SQLiteConnection(connectionString))
                {
                    dbConn.Open();
                    string classQuery = $"SELECT _vehiclecategory_name FROM vehicles WHERE _vehicle_name = '{vehicleName}'";
                    using (var dbCmd = new SQLiteCommand(classQuery, dbConn))
                    {
                        var result = dbCmd.ExecuteScalar();
                        carClass = result != null ? result.ToString() : "Unknown"; // Default to "Unknown" if not found
                    }
                }
            }

            // Set up the query to insert into the database
            string query = $@"
                            INSERT INTO [records] 
                            (trackId, carName, carClass, conditions, orientation, raceType, score, Total_Time, Average_Lap, {avg}, timestamp) 
                            VALUES 
                            ('{trackId}', '{combo_Vehicle.Text.Replace("'", "''")}', '{carClass}',
                             '{(cb_conditions.IsChecked == true ? 1 : 0)}', '{(cb_orientation.IsChecked == true ? 1 : 0)}',
                             '{raceType}', {GetScoreOrTime(raceType)}, 
                             '{Race_Min.Text}:{Race_Sec.Text}.{Race_Ms.Text}', '{(isLaps ? average_LapTime.Text : "N/A")}', 
                             '{avgSpeed}', CURRENT_TIMESTAMP);
                            
                            UPDATE [records] SET {otherSIAVG} = Round({avg} {epidia} {onemile}, 2) WHERE carName = '{combo_Vehicle.Text.Replace("'", "''")}' AND trackId = '{trackId}';
                            UPDATE [tracks] SET Runs = Runs + 1 WHERE id = '{trackId}';
                            UPDATE [vehicles] SET {odo} = '{Convert.ToDouble(txt_odometer.Text)}', _races_ran = _races_ran + 1 WHERE _vehicle_name = '{combo_Vehicle.Text.Replace("'", "''")}';
                            UPDATE [vehicles] SET {otherSiName} = '{calcOtherSIOdometer}' WHERE _vehicle_name = '{combo_Vehicle.Text.Replace("'", "''")}'; ";

            using (var dbConn = new SQLiteConnection(connectionString))
            {
                dbConn.Open();
                using (var dbCmd = new SQLiteCommand(query, dbConn))
                {
                    dbCmd.ExecuteNonQuery();
                }
            }

            ResetRaceFields();
            if (keep.IsChecked == false && retry.IsChecked == false) { ResetAllFields(); }
            FinalizeUI();
            calc_Total_Odometer();
            if (keep.IsChecked == true) { LoadNextVehicleData(odo); }
            UpdateViewEntries();
            isCalculated = 0;
        }



        // Helper method to return the correct value (time or score) based on race type
        private string GetScoreOrTime(string raceType)
        {
            if (raceType.Contains("Speed") || raceType.Contains("Trap") || raceType.Contains("Keep") || raceType.Contains("Follow"))
            {
                // Score-based races
                return $"'{PointsTextBlock.Text}'"; // Assuming you have a TextBox for score
            }
            else
            {
                // Time-based races
                return "NULL"; // Since score is not applicable for time-based races
            }
        }

        private bool ValidateSelections()
        {
            return combo_Track.SelectedIndex >= 0 && combo_Class.SelectedIndex >= 0 && combo_Vehicle.SelectedIndex >= 0;
        }

        private void PrepareSI(out string odo, out string otherSiName, out double calcOtherSIOdometer, out double calcOtherSIAVG,
                               out string otherSIAVG, out string avg, out string epidia)
        {
            string answer = AverageRace_Speed.Text.Substring(0, AverageRace_Speed.Text.Length - speed.Length);
            if (SI == "Metric")
            {
                odo = "_odometer_metric";
                otherSiName = "_odometer_imperial";
                calcOtherSIOdometer = Math.Round(Convert.ToDouble(txt_odometer.Text) * onemile, 2);
                avg = "Average_Speed_Metric";
                otherSIAVG = "Average_Speed_Imperial";
                epidia = "*";
                calcOtherSIAVG = Math.Round(Convert.ToDouble(answer) * onemile, 2);
            }
            else
            {
                odo = "_odometer_imperial";
                otherSiName = "_odometer_metric";
                calcOtherSIOdometer = Math.Round(Convert.ToDouble(txt_odometer.Text) / onemile, 2);
                avg = "Average_Speed_Imperial";
                otherSIAVG = "Average_Speed_Metric";
                epidia = "/";
                calcOtherSIAVG = Math.Round(Convert.ToDouble(answer) / onemile, 2);
            }
        }

        private void ResetRaceFields()
        {
            Race_Min.Text = "";
            Race_Sec.Text = "";
            Race_Ms.Text = "";
            AverageRace_Speed.Text = "0.0";
        }
        private void ResetAllFields()
        {
            Race_Min.Text = "";
            Race_Sec.Text = "";
            Race_Ms.Text = "";
            RaceDataGroupBox.Visibility = Visibility.Collapsed;
            AverageRace_Speed.Text = "0.0";
            Speed_Text.Text = speed;
            combo_Type.SelectedIndex = -1;
            combo_Track.SelectedIndex = -1;
            combo_Class.SelectedIndex = -1;
            LapLengthGroupBox.Visibility = Visibility.Collapsed;
            RaceLengthGroupBox.Visibility = Visibility.Collapsed;
            race_Length.Text = string.Empty;
            RaceLength_Distance_Unit.Text = string.Empty;
            lap_Length.Text = string.Empty;
            cb_conditions.IsChecked = false;
            combo_Vehicle.SelectedIndex = -1;
            txt_odometer.Text = string.Empty;
            txt_lrec.Text = string.Empty;
            txt_lrecCar.Text = string.Empty;
            txt_rRan.Text = string.Empty;
            txt_Progress_1.Text = string.Empty;
            txt_Progress_2.Text = string.Empty;
            txt_carPB.Text = string.Empty;
            average_LapTime.Text = string.Empty;
            keep.IsChecked = false;
        }

        private void LoadNextVehicleData(string odo)
        {
            combo_Vehicle.SelectedIndex += 1;
            if (string.IsNullOrEmpty(combo_Vehicle.Text))
                return;

            string veh = combo_Vehicle.Text.Replace("'", "''");

            using (var dbConn = new SQLiteConnection(connectionString))
            {
                dbConn.Open();
                try
                {
                    using (var dbCmd = new SQLiteCommand($"SELECT {odo} FROM [vehicles] WHERE _vehicle_name = '{veh}'", dbConn))
                    using (var reader = dbCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            txt_odometer.Text = reader[0].ToString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex);
                }
                finally
                {
                    txt_odometer.Focus();
                    calc_Total_Odometer();
                }
            }
        }

        private void UpdateViewEntries()
        {
            if (btn_loadrec_Metric.IsChecked == true || btn_loadrec_Imperial.IsChecked == true)
            {
                if (Application.Current.MainWindow.Height < 650)
                {
                    Application.Current.MainWindow.Height += 300;
                }

                if (SI == "Metric")
                {
                    ViewEntries_Metric.Visibility = Visibility.Visible;
                }
                else if (SI == "Imperial")
                {
                    ViewEntries_Imperial.Visibility = Visibility.Visible;
                }

                RecordsOn = 1;
                FillDataGrid();
                RecordsOn = 0;
            }
            else
            {
                RecordsOn = 0;
                if (SI == "Metric")
                {
                    ViewEntries_Metric.Visibility = Visibility.Collapsed;
                }
                else if (SI == "Imperial")
                {
                    ViewEntries_Imperial.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void UpdateUIForSI(bool isClassSelected)
        {
            btn_loadrec_Metric.Content = isClassSelected ? "Load Class Records" : "Load Track Records";
            TrackRecordGroupbox.Header = isClassSelected ? "Class Record" : "Track Record";
            if (SI == "Metric")
            {
                ViewEntries_Metric.Visibility = Visibility.Collapsed;
                btn_loadrec_Metric.IsChecked = false;
            }
            else
            {
                ViewEntries_Imperial.Visibility = Visibility.Collapsed;
                btn_loadrec_Imperial.IsChecked = false;
            }
        }

        private void calc_Lap_Length_And_LoadLapRecord()
        {
            HideAll();
            string trackName = combo_Track.Text.Replace("'", "''");

            if (string.IsNullOrEmpty(trackName)) return;

            using (var dbConn = new SQLiteConnection(connectionString))
            {
                SQLiteTransaction transaction = null;
                int retryCount = 3;
                int delayMilliseconds = 100;

                try
                {
                    dbConn.Open();
                    transaction = dbConn.BeginTransaction();

                    // First Query: Retrieve track details
                    string trackQuery = $"SELECT Length, Laps, Orientation, RestrictedClass, RestrictedCar, MinSpeed, Checkpoints FROM [tracks] WHERE Name ='{trackName}'";
                    using (var trackCmd = new SQLiteCommand(trackQuery, dbConn, transaction))
                    using (var reader = trackCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            double length = Convert.ToDouble(reader[0]);
                            int laps = Convert.ToInt32(reader[1]);
                            int oriental = Convert.ToInt32(reader[2]);
                            string classRestriction = reader[3].ToString();
                            string vehicleRestriction = reader[4].ToString();
                            double minSpeed = Convert.ToDouble(reader[5]);
                            int checkpoints = Convert.ToInt32(reader[6]);

                            // Apply restrictions and display track details
                            ApplyRestrictions(classRestriction, vehicleRestriction);
                            DisplayTrackDetails(length, laps, minSpeed, checkpoints, oriental);
                        }
                    }

                    // Second Query: Retrieve lap record details
                    ComboBoxItem selectedClassItem = (ComboBoxItem)combo_Class.SelectedItem;
                    string selectedClass = selectedClassItem?.Value;
                    int conditions = cb_conditions.IsChecked == true ? 1 : 0;
                    int orientation = cb_orientation.IsChecked == true ? 1 : 0;

                    string lapRecordQuery = !string.IsNullOrEmpty(selectedClass)
                        ? $"SELECT carName, Min(Total_Time), conditions, orientation FROM [records] WHERE carClass = '{selectedClass}' AND trackId = (SELECT id FROM [tracks] WHERE Name ='{trackName}');"
                        : $"SELECT carName, Min(Total_Time), conditions, orientation FROM [records] WHERE trackId = (SELECT id FROM [tracks] WHERE Name ='{trackName}');";

                    using (var lapCmd = new SQLiteCommand(lapRecordQuery, dbConn, transaction))
                    {
                        // Retry mechanism for lap record query
                        for (int attempt = 0; attempt < retryCount; attempt++)
                        {
                            try
                            {
                                using (var lapReader = lapCmd.ExecuteReader())
                                {
                                    if (lapReader.Read())
                                    {
                                        string carName = lapReader.IsDBNull(0) ? "No Record" : lapReader.GetString(0);
                                        string value = lapReader.IsDBNull(1) ? "No Record" : lapReader.GetString(1);
                                        int weather = lapReader.IsDBNull(2) ? 0 : lapReader.GetInt32(2);
                                        int orient = lapReader.IsDBNull(3) ? 0 : lapReader.GetInt32(3);

                                        // Display lap record details
                                        txt_lrec.Text = value;
                                        txt_lrecCar.Text = carName;
                                        weatherS_lrec.Visibility = weather == 0 ? Visibility.Visible : Visibility.Collapsed;
                                        weatherR_lrec.Visibility = weather == 1 ? Visibility.Visible : Visibility.Collapsed;
                                        orientationC_lrec.Visibility = orient == 0 ? Visibility.Visible : Visibility.Collapsed;
                                        orientationA_lrec.Visibility = orient == 1 ? Visibility.Visible : Visibility.Collapsed;

                                        // Update track record in DB
                                        string recordType = DetermineRecordType(selectedClass);
                                        if (!string.IsNullOrEmpty(carName) && !string.IsNullOrEmpty(value))
                                        {
                                            string updateQuery = $"UPDATE [tracks] SET {recordType} = '{carName} - {txt_lrec.Text}' WHERE Name = '{trackName}'";
                                            using (var updateCmd = new SQLiteCommand(updateQuery, dbConn, transaction))
                                            {
                                                updateCmd.ExecuteNonQuery();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        txt_lrec.Text = "No Record";
                                        txt_lrecCar.Text = "No Record";
                                    }
                                }
                                transaction.Commit();
                                break; // Exit retry loop on success
                            }
                            catch (SQLiteException ex) when (ex.ResultCode == SQLiteErrorCode.Busy)
                            {
                                if (attempt == retryCount - 1)
                                {
                                    throw; // If it's the last attempt, rethrow the exception
                                }
                                System.Threading.Thread.Sleep(delayMilliseconds); // Wait before retrying
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    transaction?.Rollback();
                    MessageBox.Show("Error: " + ex.Message);
                }
                finally
                {
                    dbConn.Close();
                }
            }
        }



        // Helper function to determine the record type
        private string DetermineRecordType(object selectedValue)
        {
            // Check if selectedValue is null or not convertible to int
            if (selectedValue == null)
            {
                return "Track_Lap_Record"; // Default to Track_Lap_Record if no class is selected
            }

            // Try to convert selectedValue to string first, then parse it as an integer
            if (int.TryParse(selectedValue.ToString(), out int classValue))
            {
                switch (classValue)
                {
                    case 1:
                        return "Record_A1";
                    case 2:
                        return "Record_A2";
                    case 3:
                        return "Record_A3";
                    case 4:
                        return "Record_A4";
                    case 5:
                        return "Record_A5";
                    case 6:
                        return "Record_A6";
                    case 7:
                        return "Record_A7";
                    case 8:
                        return "Record_B1";
                    case 9:
                        return "Record_B2";
                    case 10:
                        return "Record_B3";
                    case 11:
                        return "Record_B4";
                    case 12:
                        return "Record_C1";
                    case 13:
                        return "Record_C2";
                    case 14:
                        return "Record_C3";
                    case 15:
                        return "Record_C4";
                    case 16:
                        return "Record_MA1";
                    case 17:
                        return "Record_MA2";
                    default:
                        return "Track_Lap_Record"; // Fallback for unknown values
                }
            }
            else
            {
                // If selectedValue is not an integer, return a default value
                return "Track_Lap_Record";
            }
        }

        private void Btn_submit_GotFocus(object sender, RoutedEventArgs e)
        {
            calc_Average_Lap_Time();
            //calc_Average_Speed();
        }

        private void Btn_loadrec_Click(object sender, RoutedEventArgs e)
        {
            if (combo_Track.SelectedIndex == -1) { return; }
            if (btn_loadrec_Metric.IsChecked == true || btn_loadrec_Imperial.IsChecked == true)
            {
                if (SI == "Metric")
                {
                    ViewEntries_Metric.Visibility = Visibility.Visible;
                }
                else
                {
                    ViewEntries_Imperial.Visibility = Visibility.Visible;
                }
                FillDataGrid();
            }
            else
            {
                if (SI == "Metric")
                {
                    ViewEntries_Metric.Visibility = Visibility.Collapsed;
                    ViewEntries_Metric.ItemsSource = "";
                }
                else
                {
                    ViewEntries_Imperial.Visibility = Visibility.Collapsed;
                    ViewEntries_Imperial.ItemsSource = "";
                }

            }
        }

        private void Combo_Vehicle_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(combo_Vehicle.Text)) return;

            btn_loadrec_Metric.Content = "Load Vehicle Records";
            btn_loadrec_Imperial.Content = "Load Vehicle Records";

            FetchAndDisplayVehicleData();
        }


        private void Combo_Track_LostFocus(object sender, RoutedEventArgs e)
        {
            // Call HandleTrackChange to update the UI and clear txt_carPB when a key is pressed
            HandleTrackChange(clearCarPB: false);
            InfoGroupBox.Visibility = Visibility.Visible;
        }
        // Handles the logic when a key is released while the Combo_Track is focused.
        private void Combo_Track_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            // Call HandleTrackChange to update the UI and clear txt_carPB when a key is pressed
            HandleTrackChange(clearCarPB: false);
        }

        private void Combo_Track_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HandleTrackChange(clearCarPB: false);
        }

        // Centralized method to handle changes when a track is selected or changed.
        private void HandleTrackChange(bool clearCarPB = false)
        {
            // Hide all UI elements initially
            if (isCalculated == 0)
            {
                HideAll();
            }

            // If no track is selected, exit early
            if (combo_Track.SelectedIndex == -1) return;

            // If track is changed, reset class and vehicle
            //combo_Vehicle.SelectedIndex = -1;

            // Update UI elements based on the track selection
            UpdateTrackUI();
            UpdateRaceTypeUI();
            FinalizeUI();

            // Set Orientation


            // Optionally clear txt_carPB based on the flag
            if (clearCarPB)
                txt_carPB.Text = "";

            // Fetch and display vehicle data if a vehicle is selected
            if (!string.IsNullOrEmpty(combo_Vehicle.Text))
                FetchAndDisplayVehicleData();
            isCalculated = 0;
        }

        // Hides all UI elements and resets their associated text fields.
        private void HideAll()
        {
            if (isCalculated == 0)
            {
                // Hide and reset all UI elements
                OrientationGroupBox.Visibility = Visibility.Collapsed;
                WeatherGroupBox.Visibility = Visibility.Collapsed;
                InfoGroupBox.Visibility = Visibility.Collapsed;
                TrackProgressGroupBox.Visibility = Visibility.Collapsed;
                txt_Progress_1.Text = string.Empty;
                txt_Progress_2.Text = string.Empty;
                ClassRestictionGroupBox.Visibility = Visibility.Collapsed;
                ClassRestrictionTextBlock.Text = string.Empty;
                VehicleRestictionGroupBox.Visibility = Visibility.Collapsed;
                VehicleRestrictionTextBlock.Text = string.Empty;
                LapLengthGroupBox.Visibility = Visibility.Collapsed;
                lap_Length.Text = string.Empty;
                LapsGroupBox.Visibility = Visibility.Collapsed;
                LapsTextBlock.Text = string.Empty;
                RaceLengthGroupBox.Visibility = Visibility.Collapsed;
                race_Length.Text = string.Empty;
                RaceLength_Distance_Unit.Text = string.Empty;
                TrackRecordGroupbox.Visibility = Visibility.Collapsed;
                txt_lrecCar.Text = string.Empty;
                weatherS_lrec.Visibility = Visibility.Collapsed;
                weatherR_lrec.Visibility = Visibility.Collapsed;
                orientationC_lrec.Visibility = Visibility.Collapsed;
                orientationA_lrec.Visibility = Visibility.Collapsed;
                PointsGroupBox.Visibility = Visibility.Collapsed;
                PointsTextBlock.Text = string.Empty;
                MinimumSpeedGroupBox.Visibility = Visibility.Collapsed;
                MinimumSpeedTextBlock.Text = string.Empty;
                CheckPointsGroupBox.Visibility = Visibility.Collapsed;
                CheckpointsTextBlock.Text = string.Empty;
            }
        }

        // Updates the UI based on the selected race type.
        // Update UI elements related to the selected race type.
        private void UpdateRaceTypeUI()
        {
            if (isCalculated == 0) {
                //HideAll();
                if (combo_Type.SelectedItem is ComboBoxItem selectedItem)
                {
                    string selectedRaceType = selectedItem.Value;
                    // Determine event types
                    bool isRace = selectedRaceType == "Race SP" || selectedRaceType == "Race MP" || selectedRaceType == "Race RMP";
                    bool isEliminator = selectedRaceType == "Eliminator SP";
                    bool isTimeAttack = selectedRaceType == "Time Attack SP";
                    bool isSpeedEvent = selectedRaceType == "Speed SP" || selectedRaceType == "Speed MP";
                    bool isSpeedTrap = selectedRaceType == "Speed Trap SP" || selectedRaceType == "Speed Trap MP";
                    bool isFYLEvent = selectedRaceType == "Follow The Leader MP";
                    bool isKYDEvent = selectedRaceType == "Keep Your Distance MP";

                    // Define visibility conditions
                    bool hasRestrictedClass = isRace || isEliminator || isTimeAttack || isSpeedEvent || isSpeedTrap;
                    bool hasRestrictedVehicle = isRace || isEliminator || isTimeAttack || isSpeedEvent || isSpeedTrap;
                    bool hasOrientation = isRace || isEliminator || isTimeAttack;
                    bool hasDistance = isRace || isEliminator || isTimeAttack;
                    bool hasLaps = isRace || isEliminator || isTimeAttack; // Laps for race-related types
                    bool hasTrackLapRecord = isRace || isEliminator || isTimeAttack;
                    bool hasScore = isSpeedEvent || isSpeedTrap || isFYLEvent || isKYDEvent;
                    bool hasWeather = isRace || isEliminator || isTimeAttack || isSpeedEvent || isSpeedTrap;

                    // Update the headers and visibility of the UI elements based on the selected race type
                    TrackRecordGroupbox.Header = hasTrackLapRecord ? "Track Record" : "Fastest Time";
                    TrackRecordGroupbox.Visibility = hasTrackLapRecord ? Visibility.Visible : Visibility.Collapsed;
                    OrientationGroupBox.Visibility = hasOrientation ? Visibility.Visible : Visibility.Collapsed;
                    LapLengthGroupBox.Visibility = hasDistance ? Visibility.Visible : Visibility.Collapsed;
                    LapsGroupBox.Visibility = hasLaps ? Visibility.Visible : Visibility.Collapsed;
                    RaceLengthGroupBox.Visibility = hasDistance ? Visibility.Visible : Visibility.Collapsed;
                    PointsGroupBox.Visibility = hasScore ? Visibility.Visible : Visibility.Collapsed;
                    MinimumSpeedGroupBox.Visibility = isSpeedEvent ? Visibility.Visible : Visibility.Collapsed;
                    PointsGroupBox.Visibility = isSpeedEvent ? Visibility.Visible : Visibility.Collapsed;
                    CheckPointsGroupBox.Visibility = isSpeedTrap ? Visibility.Visible : Visibility.Collapsed;
                    VehicleRestrictionTextBlock.Visibility = hasRestrictedVehicle ? Visibility.Visible : Visibility.Collapsed;
                    ClassRestictionGroupBox.Visibility = hasRestrictedVehicle ? Visibility.Visible : Visibility.Collapsed;
                    WeatherGroupBox.Visibility = hasWeather ? Visibility.Visible : Visibility.Collapsed;
                    RaceTimeGroupBox.Visibility = hasTrackLapRecord ? Visibility.Visible : Visibility.Collapsed;
                    isCalculated++;
                }
            }
        }

        // Updates the track-related UI elements based on the selected system of units.
        private void UpdateTrackUI()
        {
            bool isMetric = SI == "Metric"; // Determine if the selected system is Metric

            // Enable/disable buttons based on the system of units
            btn_loadrec_Metric.IsEnabled = isMetric;
            btn_loadrec_Imperial.IsEnabled = !isMetric;

            // Uncheck both buttons initially
            btn_loadrec_Metric.IsChecked = false;
            btn_loadrec_Imperial.IsChecked = false;

            // Set button content based on the selected class and system of units
            if (combo_Class.SelectedIndex < 0)
            {
                string buttonText = "Load Track Records";
                btn_loadrec_Metric.Content = isMetric ? buttonText : btn_loadrec_Metric.Content;
                btn_loadrec_Imperial.Content = !isMetric ? buttonText : btn_loadrec_Imperial.Content;
            }

            //// Adjust visibility based on the system of units
            //ViewEntries_Metric.Visibility = isMetric ? Visibility.Collapsed : Visibility.Visible;
            //ViewEntries_Imperial.Visibility = isMetric ? Visibility.Visible : Visibility.Collapsed;
        }

        // Final adjustments to the UI after updating track information.
        private void FinalizeUI()
        {
            calc_Lap_Length_And_LoadLapRecord();     // Loads the lap record for the selected track
            CheckProgress();     // Checks the current progress or state of the race/track
        }


        // Fetches and displays vehicle data based on the selected track and vehicle.
        private void FetchAndDisplayVehicleData()
        {
            // Checks if a valid vehicle and track are selected
            if (string.IsNullOrEmpty(combo_Vehicle.Text) || string.IsNullOrEmpty(combo_Track.Text)) return;

            int conditions = cb_conditions.IsChecked == true ? 1 : 0;
            int orientation = cb_orientation.IsChecked == false ? 0 : 1;
            string veh = combo_Vehicle.Text.Replace("'", "''"); // Still escaping single quotes in case of any issues
            int trackId = Convert.ToInt32(combo_Track.SelectedValue);
            string odoColumn = SI == "Metric" ? "_odometer_metric" : "_odometer_imperial";
            double conversionFactor = SI == "Imperial" ? 1.60934 : 1.0;

            // SQL query with parameters to avoid SQL injection and ensure safe execution
            string query = $@"
            SELECT 
            (SELECT {odoColumn} FROM vehicles WHERE _vehicle_name = @VehicleName),
            (SELECT Min(Total_Time) FROM records WHERE carName = @VehicleName AND trackId = @TrackId AND conditions = @Conditions AND orientation = @Orientation),
            (SELECT COUNT(*) FROM records WHERE trackId = @TrackId AND carName = @VehicleName AND conditions = @Conditions AND orientation = @Orientation)";

            try
            {
                // Execute the query and fetch data from the database
                using (var dbConn = new SQLiteConnection(connectionString))
                {
                    dbConn.Open();
                    using (var dbCmd = new SQLiteCommand(query, dbConn))
                    {
                        // Adding parameters to the query
                        dbCmd.Parameters.AddWithValue("@VehicleName", veh);
                        dbCmd.Parameters.AddWithValue("@TrackId", trackId);
                        dbCmd.Parameters.AddWithValue("@Conditions", conditions);
                        dbCmd.Parameters.AddWithValue("@Orientation", orientation);

                        using (var reader = dbCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Update the UI with the fetched data
                                if (!reader.IsDBNull(0))
                                {
                                    double value = reader.GetDouble(0) * conversionFactor;
                                    txt_odometer.Text = Math.Round(value, 2).ToString();
                                }
                                else
                                {
                                    txt_odometer.Text = "0";
                                }

                                txt_carPB.Text = reader.IsDBNull(1) ? "No Record" : reader.GetString(1);
                                txt_rRan.Text = reader.GetInt32(2).ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Display an error message if the database query fails
                MessageBox.Show($"Error: {ex.Message}");
            }
        }


        private void ResetWindowState()
        {
            Application.Current.MainWindow.Height = 500;
        }


        private void CheckProgress(SQLiteConnection dbConn = null)
        {
            if (combo_Track.SelectedIndex == -1) { return; }
            txt_Progress_1.Text = "";
            txt_Progress_2.Text = "";

            if (TrackProgressGroupBox.Visibility != Visibility.Visible)
            {
                TrackProgressGroupBox.Visibility = Visibility.Visible;
            }

            if (string.IsNullOrEmpty(combo_Track.Text)) { return; }

            int trackId = Convert.ToInt32(combo_Track.SelectedValue);
            int conditions = cb_conditions.IsChecked == true ? 1 : 0;
            int orientation = cb_orientation.IsChecked == true ? 1 : 0;
            string param = ClassRestrictionTextBlock.Visibility == Visibility.Visible && ClassRestrictionTextBlock.Text.Length > 0
                ? ClassRestrictionTextBlock.Text
                : "";

            double percentage;

            bool localConnection = false;
            if (dbConn == null)
            {
                dbConn = new SQLiteConnection(connectionString);
                dbConn.Open();
                localConnection = true;
            }

            try
            {
                int recordsCount = GetRecordCount(dbConn, trackId, conditions, orientation, param);
                int carsCount = GetCarsCount(dbConn, param);

                percentage = (recordsCount / (double)carsCount) * 100;

                if (recordsCount > 0)
                {
                    txt_Progress_1.Text = $"{recordsCount} / {carsCount}";
                    txt_Progress_2.Text = $"{Math.Round(percentage, 2)}%";
                }
                else
                {
                    txt_Progress_1.Text = $"0/{carsCount}";
                    txt_Progress_2.Text = "0%";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.ToString());
            }
            finally
            {
                if (localConnection && dbConn != null)
                {
                    dbConn.Close();
                }
            }
        }

        private int GetRecordCount(SQLiteConnection dbConn, int trackId, int conditions, int orientation, string param)
        {
            string query = string.IsNullOrEmpty(param)
                ? $"SELECT COUNT(DISTINCT carName) FROM[records] WHERE trackId = '{trackId}' AND conditions = '{conditions}' AND orientation = '{orientation}'; "
                : $"SELECT COUNT(DISTINCT carName) FROM [records] WHERE trackId = '{trackId}' AND carClass = '{param}' AND conditions = '{conditions}' AND orientation = '{orientation}'; ";
            using (var cmd = new SQLiteCommand(query, dbConn))
            using (var reader = cmd.ExecuteReader())
            {
                reader.Read();
                return reader.GetInt32(0);
            }
        }

        private int GetCarsCount(SQLiteConnection dbConn, string param)
        {
            string query = string.IsNullOrEmpty(param)
                ? "SELECT COUNT(*) FROM [vehicles] WHERE _is_active = 'true' and _is_owned = 'true';"
                : $"SELECT COUNT(*) FROM [vehicles] WHERE _vehiclecategory_name = '{param}' AND _is_active = 'true' AND _is_owned = 'true' ;";
            
            using (var cmd = new SQLiteCommand(query, dbConn))
            using (var reader = cmd.ExecuteReader())
            {
                reader.Read();
                return reader.GetInt32(0);
            }
        }


        // Select all text when the TextBox receives keyboard focus
        private void TextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            (sender as TextBox).SelectAll();
        }

        // Select all text when the TextBox receives mouse capture
        private void TextBox_GotMouseCapture(object sender, MouseEventArgs e)
        {
            (sender as TextBox).SelectAll();
        }

        // Replace commas with dots when the TextBox loses focus (e.g., for numerical input)
        private void Txt_odometer_LostFocus(object sender, RoutedEventArgs e)
        {
            txt_odometer.Text = txt_odometer.Text.Replace(",", ".");
        }

        public int RemoveIndex { get; private set; }

        // Define regex patterns for allowed/disallowed text
        private static readonly Regex _regexNoDec = new Regex("[^0-9]+"); // Allows only numeric characters (no decimals)

        // Check if the input text is allowed without decimal points
        private static bool IsDecAllowed(string text)
        {
            return !_regexNoDec.IsMatch(text);
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void Btn_TrackInfo_Click(object sender, MouseButtonEventArgs e)
        {
            if (combo_Track.SelectedIndex == -1)
            {
                MessageBox.Show("No Track Selected", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            if (string.IsNullOrEmpty(combo_Track.Text)) return;

            int trackId = Convert.ToInt32(combo_Track.SelectedValue);

            using (var dbConn = new SQLiteConnection(connectionString))
            {
                dbConn.Open();
                try
                {
                    // Query to fetch the track name and total runs for the selected track
                    string trackQuery = "SELECT Name, Runs FROM tracks WHERE Id = @TrackId";
                    string trackName = "";
                    int totalRuns = 0;

                    using (var dbCmd = new SQLiteCommand(trackQuery, dbConn))
                    {
                        dbCmd.Parameters.AddWithValue("@TrackId", trackId);
                        using (var reader = dbCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                trackName = reader.GetString(0); // Track Name
                                totalRuns = reader.GetInt32(1);  // Total Runs
                            }
                        }
                    }

                    // Query to fetch the fastest times by car class
                    string recordsQuery = @"
                SELECT 
                    r.carClass, 
                    r.carName, 
                    MIN(r.Total_Time) AS Fastest_Time
                FROM 
                    records r
                WHERE 
                    r.trackId = @TrackId
                    AND r.carClass IN ('A1', 'A2', 'A3', 'A4', 'A5', 'A6', 'A7', 
                                       'B1', 'B2', 'B3', 'B4', 
                                       'C1', 'C2', 'C3', 'C4', 
                                       'mA1', 'mA2')
                GROUP BY 
                    r.carClass
                ORDER BY 
                    r.carClass, Fastest_Time";

                    using (var dbCmd = new SQLiteCommand(recordsQuery, dbConn))
                    {
                        dbCmd.Parameters.AddWithValue("@TrackId", trackId);

                        using (var reader = dbCmd.ExecuteReader())
                        {
                            // Prepare a StringBuilder to collect the track info and lap records
                            var sb = new StringBuilder();
                            sb.AppendLine($"Track Name: {trackName}")
                              .AppendLine($"Total Runs: {totalRuns}")
                              .AppendLine("-----------------");

                            // Loop through each record and append the car class and the fastest time
                            while (reader.Read())
                            {
                                int x = 0;
                                string carClass = reader.GetString(0); // Car class (A1, A2, etc.)
                                string carName = reader.GetString(1);  // Car name

                                // Fastest time is treated as a string in the format "mm:ss.ms"
                                string fastestTime = reader.IsDBNull(2) ? "No Record Yet" : reader.GetString(2);

                                if (x > 0) { sb.AppendLine("-----------------"); }
                                sb.AppendLine($"{carClass} - {carName} - {fastestTime}")
                                  .AppendLine("-----------------");
                                x++;
                            }

                            // Display the result in a message box
                            MessageBox.Show(sb.ToString(), "Track Records", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Error handling if the query fails
                    MessageBox.Show("Error: " + ex.ToString());
                }
            }
        }

        private void FillDataGrid()
        {
            string avgSIRaceSpeed;
            string[] headers = { "Name", "Class", "Average Speed", "Average Lap", "Race Time" };
            int conditions = cb_conditions.IsChecked == true ? 1 : 0;
            int orientation = cb_orientation.IsChecked == true ? 1 : 0;

            if (string.IsNullOrEmpty(combo_Track.Text)) { return; }
            int trackId = Convert.ToInt32(combo_Track.SelectedValue);
            string carClass = combo_Class.Text;
            string dbTable = "records";

            // Set SI type
            if (SI == "Metric")
            {
                avgSIRaceSpeed = "Average_Speed_Metric";
            }
            else
            {
                avgSIRaceSpeed = "Average_Speed_Imperial";
            }

            // Create the query
            string query = $"SELECT carName, carClass, {avgSIRaceSpeed}, Average_Lap , Total_Time " +
                           $"FROM {dbTable} WHERE trackId=@track AND conditions=@conditions AND orientation=@orientation ";

            if (!string.IsNullOrEmpty(combo_Class.Text))
            {
                query += $"AND carClass=@carClass ";
            }
            if (!string.IsNullOrEmpty(combo_Vehicle.Text))
            {
                query += $"AND carName=@carName ";
            }

            query += "ORDER BY Total_Time ;";

            try
            {
                using (var dbConn = new SQLiteConnection(connectionString))
                {
                    dbConn.Open();

                    using (var dbCmd = new SQLiteCommand(query, dbConn))
                    {
                        dbCmd.Parameters.AddWithValue("@track", trackId);
                        dbCmd.Parameters.AddWithValue("@conditions", conditions);
                        dbCmd.Parameters.AddWithValue("@orientation", orientation);
                        if (!string.IsNullOrEmpty(combo_Class.Text))
                        {
                            dbCmd.Parameters.AddWithValue("@carClass", carClass);
                        }
                        if (!string.IsNullOrEmpty(combo_Vehicle.Text))
                        {
                            dbCmd.Parameters.AddWithValue("@carName", combo_Vehicle.Text);
                        }

                        using (var dbAdapter = new SQLiteDataAdapter(dbCmd))
                        {
                            DataTable mTable = new DataTable();
                            dbAdapter.Fill(mTable);

                            // Bind the DataTable to the appropriate DataGrid
                            var dataGrid = SI == "Metric" ? ViewEntries_Metric : ViewEntries_Imperial;
                            dataGrid.ItemsSource = mTable.DefaultView;

                            // Set headers
                            for (int i = 0; i < headers.Length; i++)
                            {
                                if (i < dataGrid.Columns.Count)
                                {
                                    dataGrid.Columns[i].Header = headers[i];
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Message: " + ex.Message);
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

        private void txt_odometer_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            string newText = textBox.Text.Insert(textBox.CaretIndex, e.Text);
            e.Handled = !IsOdoTextAllowed(newText);
        }

        private void txt_odometer_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        private void txt_odometer_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!(sender is TextBox textBox))
                return;

            if (textBox.Text.Contains('.'))
            {
                int dotIndex = textBox.Text.IndexOf('.');
                if (textBox.Text.Length - dotIndex - 1 > 1)
                {
                    textBox.Text = textBox.Text.Substring(0, dotIndex + 2);
                    textBox.CaretIndex = textBox.Text.Length;
                }
            }
        }

        private static bool IsOdoTextAllowed(string text)
        {
            // Check for multiple dots
            if (text.Count(c => c == '.') > 1)
            {
                return false;
            }

            // Check for more than one digit after the dot
            int dotIndex = text.IndexOf('.');
            if (dotIndex != -1 && text.Length - dotIndex - 1 > 1)
            {
                return false;
            }

            // Allow only digits and dot
            return text.All(c => char.IsDigit(c) || c == '.');
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (keep.IsChecked == false)
            {

                keep.IsChecked = true;
            }
            else
            {
                keep.IsChecked = false;
            }

        }
        private void Retry_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (retry.IsChecked == false)
            {

                retry.IsChecked = true;
            }
            else
            {
                retry.IsChecked = false;
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

        void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void VehicleManager_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var VehicleWindow = new Vehicle();
            VehicleWindow.Show();
        }

        private void Minimize_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Window.GetWindow(this).WindowState = WindowState.Minimized;
        }

        private void Close_Button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }
        private void OpenChildWindow<T>(string windowIdentifier) where T : Window, new()
        {
            var childWindow = new T();

            // Get the screen dimensions
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;

            double targetLeft, targetTop;

            // Check if the window has a remembered position
            if (lastWindowPositions.ContainsKey(windowIdentifier))
            {
                // Restore the remembered position
                (targetLeft, targetTop) = lastWindowPositions[windowIdentifier];
            }
            else
            {
                // Determine position based on windowIdentifier
                if (windowIdentifier == "SettingsWindow" || windowIdentifier == "EventWindow")
                {
                    // Position to the right of the main window for "SettingsWindow" or "EventWindow"
                    targetLeft = this.Left + this.Width + 10; // Right of the main window
                    targetTop = this.Top + (this.Height - childWindow.Height) / 2; // Vertically centered with respect to the main window
                }
                else
                {
                    // Center the child window for other windows
                    targetLeft = (screenWidth - childWindow.Width) / 2;
                    targetTop = (screenHeight - childWindow.Height) / 2;
                }
            }
            // Special handling for "DragWindow" or similar identifiers
            if (windowIdentifier.StartsWith("Drag"))
            {
                // Ensure the window starts higher to prevent it from going off the bottom of the screen
                targetTop -= 300; // Shift it 100 pixels higher. Adjust this value as needed.
            }

            // Adjust the position to ensure it's within screen bounds
            if (targetLeft + childWindow.Width > screenWidth)
            {
                targetLeft = screenWidth - childWindow.Width; // Prevent going off-screen to the right
            }
            if (targetLeft < 0)
            {
                targetLeft = 0; // Prevent going off-screen to the left
            }
            if (targetTop + childWindow.Height > screenHeight)
            {
                targetTop = screenHeight - childWindow.Height; // Prevent going off-screen down
            }
            if (targetTop < 0)
            {
                targetTop = 0; // Prevent going off-screen up
            }

            // Set the window position
            childWindow.Left = targetLeft;
            childWindow.Top = targetTop;

            // Subscribe to the closed event to remember the position
            childWindow.Closed += (sender, e) => ChildWindow_Closed(sender, windowIdentifier);

            // Show the child window
            if (windowIdentifier == "PowerLapBoard" || windowIdentifier == "DragWindow")
            {
                childWindow.Show();
            }
            else
            {
                childWindow.ShowDialog();
            }

            // Set focus on the child window
            childWindow.Focus();
        }


        private void ChildWindow_Closed(object sender, string windowIdentifier)
        {
            if (sender is Window childWindow)
            {
                lastWindowPositions[windowIdentifier] = (childWindow.Left, childWindow.Top);
            }

            // Additional logic when the window is closed, if needed
            if (windowIdentifier == "EventWindow") { 
            combo_Type.SelectedIndex = -1;
            }
        }
        private void PowerLaps_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenChildWindow<powerlap>("PowerLapBoard");
        }

        private void Drag_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenChildWindow<Drag>("DragWindow");
        }

        private void Objectives_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenChildWindow<ObjectivesWindow>("ObjectivesWindow");
        }

        private void Houses_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenChildWindow<houses>("HousesWindow");
        }

        private void Dealership_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenChildWindow<dealerships>("DealershipsWindow");
        }
        // Usage for opening specific child windows
        private void tracks_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenChildWindow<tracks>("EventWindow");
        }
        private void Settings_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Prompt the user before closing the main window
            if (Popup == 0)
            {
                MessageBoxResult result = MessageBox.Show(
                    "Changing the settings will reload the main window, and any unsaved data will be lost. Do you want to continue?",
                    "Warning",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    OpenChildWindow<SettingsWindow>("SettingsWindow");
                    //var settingsWindow = new SettingsWindow();

                    //settingsWindow.Left = this.Left + this.Width + 10;
                    //settingsWindow.Top = this.Top;
                    //settingsWindow.ShowDialog();
                }
            }
            else
            {
                OpenChildWindow<SettingsWindow>("SettingsWindow");
            }
            Popup++;
        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
        private void Race_Ms_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ResetAvgSpeedText(sender, e, AverageRace_Speed);
        }
        private void Lap1_Ms_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            HandleLapMsPreviewLostKeyboardFocus(Race_Min, Race_Sec, Race_Ms, AverageRace_Speed);
        }
        private void ResetAvgSpeedText(object sender, KeyboardFocusChangedEventArgs e, TextBlock avgSpeedTextBlock)
        {
            avgSpeedTextBlock.Text = "0.00";
            Speed_Text.Text = speed;
        }

        private void TextBlock_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenLapInputWindow();
        }
        private void OpenLapInputWindow()
        {
            LapInputWindow lapInputWindow = new LapInputWindow();
            if (lapInputWindow.ShowDialog() == true)
            {
                // Process LapTimes from the child window
                foreach (var lapTime in lapInputWindow.LapTimes)
                {
                    // Here you can sum up the minutes, seconds, and milliseconds
                    Race_Min.Text = lapTime.Minutes.ToString();
                    Race_Sec.Text = lapTime.Seconds.ToString();
                    Race_Ms.Text = lapTime.Milliseconds.ToString();
                }
            }
        }
    }
}
public class DataGridBehavior
{
    #region DisplayRowNumber

    public static DependencyProperty DisplayRowNumberProperty =
        DependencyProperty.RegisterAttached("DisplayRowNumber",
                                            typeof(bool),
                                            typeof(DataGridBehavior),
                                            new FrameworkPropertyMetadata(false, OnDisplayRowNumberChanged));
    public static bool GetDisplayRowNumber(DependencyObject target)
    {
        return (bool)target.GetValue(DisplayRowNumberProperty);
    }
    public static void SetDisplayRowNumber(DependencyObject target, bool value)
    {
        target.SetValue(DisplayRowNumberProperty, value);
    }

    private static void OnDisplayRowNumberChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
    {
        DataGrid dataGrid = target as DataGrid;
        if ((bool)e.NewValue == true)
        {
            EventHandler<DataGridRowEventArgs> loadedRowHandler = null;
            loadedRowHandler = (object sender, DataGridRowEventArgs ea) =>
            {
                if (GetDisplayRowNumber(dataGrid) == false)
                {
                    dataGrid.LoadingRow -= loadedRowHandler;
                    return;
                }
                ea.Row.Header = ea.Row.GetIndex();
            };
            dataGrid.LoadingRow += loadedRowHandler;

            ItemsChangedEventHandler itemsChangedHandler = null;
            itemsChangedHandler = (object sender, ItemsChangedEventArgs ea) =>
            {
                if (GetDisplayRowNumber(dataGrid) == false)
                {
                    dataGrid.ItemContainerGenerator.ItemsChanged -= itemsChangedHandler;
                    return;
                }
                GetVisualChildCollection<DataGridRow>(dataGrid).
                    ForEach(d => d.Header = d.GetIndex());
            };
            dataGrid.ItemContainerGenerator.ItemsChanged += itemsChangedHandler;
        }
    }

    #endregion // DisplayRowNumber

    #region Get Visuals

    private static List<T> GetVisualChildCollection<T>(object parent) where T : Visual
    {
        List<T> visualCollection = new List<T>();
        GetVisualChildCollection(parent as DependencyObject, visualCollection);
        return visualCollection;
    }

    private static void GetVisualChildCollection<T>(DependencyObject parent, List<T> visualCollection) where T : Visual
    {
        int count = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < count; i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(parent, i);
            if (child is T)
            {
                visualCollection.Add(child as T);
            }
            if (child != null)
            {
                GetVisualChildCollection(child, visualCollection);
            }
        }
    }

    #endregion // Get Visuals
}
