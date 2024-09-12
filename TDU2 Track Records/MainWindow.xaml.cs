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
using TDU2_Track_Records.Models;
using TDU2_Track_Records.Properties;


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
        int vehicle = 0;
        readonly string connectionString = Settings.Default.connectionString;
        public string distance = Settings.Default.distance;
        public string speed = Settings.Default.speed;
        readonly string SI = Settings.Default.system;
        private static double lastTracksLeft;
        private static double lastTracksTop;
        private static readonly Regex _regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
        SQLiteDataReader reader;
        //SQLiteConnection dbConn; // Declare the SQLiteConnection-Object
        SQLiteCommand dbCmd;
        public double onemile = 0.621371192;


        public MainWindow()
        {

            InitializeComponent();
            //SI_Setter.Text = SI;
            if (SI == "Metric")
            {
                btn_loadrec_Metric.Visibility = Visibility.Visible;
                btn_loadrec_Imperial.Visibility = Visibility.Collapsed;
                ViewEntries_Metric.Visibility = Visibility.Collapsed;
                ViewEntries_Imperial.Visibility = Visibility.Collapsed;
            }
            else
            {
                btn_loadrec_Imperial.Visibility = Visibility.Visible;
                btn_loadrec_Metric.Visibility = Visibility.Collapsed;
                ViewEntries_Metric.Visibility = Visibility.Collapsed;
                ViewEntries_Imperial.Visibility = Visibility.Collapsed;
            }
            // Bind the TextBox to the TextBlock
            Binding binding = new Binding();
            binding.Source = LapsTextBlock;
            binding.Path = new PropertyPath("Text");
            binding.Mode = BindingMode.OneWay; // TextBox will only receive text from the TextBlock
            LapsTextBox.SetBinding(TextBox.TextProperty, binding);
            // bind ends here
            //GenerateLapsInStackPanel(1);
            DynamicRaceLapsGroupBox.Visibility = Visibility.Collapsed;
            lbl_AverageSpeed.Visibility = Visibility.Collapsed;
            lbl_avgLapTime.Visibility = Visibility.Collapsed;
            lbl_TotalTime.Visibility = Visibility.Collapsed;
            old_LapsGroupBox.Visibility = Visibility.Collapsed;
            ClassRestictionGroupBox.Visibility = Visibility.Collapsed;
            VehicleRestictionGroupBox.Visibility = Visibility.Collapsed;
            OrientationGroupBox.Visibility = Visibility.Collapsed;
            LapLengthGroupBox.Visibility = Visibility.Collapsed;
            LapsGroupBox.Visibility = Visibility.Collapsed;
            RaceLengthGroupBox.Visibility = Visibility.Collapsed;
            TrackLapRecordGroupbox.Visibility = Visibility.Collapsed;
            PointsGroupBox.Visibility = Visibility.Collapsed;
            MinimumSpeedGroupBox.Visibility = Visibility.Collapsed;
            CheckPointsGroupBox.Visibility = Visibility.Collapsed;


            BindVehicleComboBox(combo_Vehicle);
            calc_Total_Odometer();

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
                new ComboBoxItem { ImagePath = "Images/carClasses/MA1.png", Description = "Motorcycles 1", Value = "MA1" },
                new ComboBoxItem { ImagePath = "Images/carClasses/MA2.png", Description = "Motorcycles 2", Value = "mA2" }
            };

            combo_Class.ItemsSource = items;

            List<ComboBoxItem> itemb = new List<ComboBoxItem>
            {
                new ComboBoxItem { ImagePath = "Images/ico/events/ico_cup_race.png", Description = "Race Cup", Value = "Race SP" },
                new ComboBoxItem { ImagePath = "Images/ico/events/ico_cup_speed.png", Description = "Speed Cup", Value = "Speed SP" },
                new ComboBoxItem { ImagePath = "Images/ico/events/ico_cup_eliminator.png", Description = "Eliminator Cup", Value = "Eliminator SP" },
                new ComboBoxItem { ImagePath = "Images/ico/events/ico_cup_speedtrap.png", Description = "Speed Trap Cup", Value = "Speed Trap SP" },
                new ComboBoxItem { ImagePath = "Images/ico/events/ico_cup_timeattack.png", Description = "Time Attack Cup", Value = "Time Attack SP" },
                new ComboBoxItem { ImagePath = "Images/ico/events/ico_mp_race.png", Description = "Race Multiplayer", Value = "Race MP" },
                new ComboBoxItem { ImagePath = "Images/ico/events/ico_mp_speed.png", Description = "Speed Multiplayer", Value = "Speed MP" },
                new ComboBoxItem { ImagePath = "Images/ico/events/ico_mp_ftl.png", Description = "Follow The Leader", Value = "Follow The Leader MP" },
                new ComboBoxItem { ImagePath = "Images/ico/events/ico_mp_kyd.png", Description = "Keep Your Distance", Value = "Keep Your Distance MP" },
                new ComboBoxItem { ImagePath = "Images/ico/events/ico_mp_speedtrap.png", Description = "Speed Trap Multiplayer", Value = "Speed Trap MP" }
            };

            combo_Type.ItemsSource = itemb;
        }
        private void LapsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ComboBoxItem selectedItem = (ComboBoxItem)combo_Type.SelectedItem;
            string selectedRaceType = selectedItem?.Value;
            switch (selectedRaceType)
            {
                case "Race SP":
                case "Time Attack SP":
                    GenerateLapsInStackPanel(1);  // No laps, just total time
                    DynamicRaceLapsGroupBox.Visibility = Visibility.Visible;
                    LapsGroupBox.Visibility = Visibility.Collapsed;
                    break;
                case "Eliminator SP":
                    GenerateLapsInStackPanel(7);  // No laps, just total time
                    DynamicRaceLapsGroupBox.Visibility = Visibility.Visible;
                    lbl_AverageSpeed.Visibility = Visibility.Visible;
                    lbl_avgLapTime.Visibility = Visibility.Visible;
                    lbl_TotalTime.Visibility = Visibility.Visible;

                    break;
                case "Race MP":
                    GenerateLapsInStackPanel(5);  // Example for multiple laps
                    DynamicRaceLapsGroupBox.Visibility = Visibility.Visible;
                    lbl_AverageSpeed.Visibility = Visibility.Visible;
                    lbl_avgLapTime.Visibility = Visibility.Visible;
                    lbl_TotalTime.Visibility = Visibility.Visible;

                    break;
                default:
                    GenerateLapsInStackPanel(0);
                    DynamicRaceLapsGroupBox.Visibility = Visibility.Collapsed;
                    LapsGroupBox.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        public void GenerateLapsInStackPanel(int lapCount)
        {
            if (string.IsNullOrEmpty(lapCount.ToString()) || lapCount < 1) return;
            DynamicRaceLapsStackPanel.Children.Clear();

            for (int lap = 1; lap <= lapCount; lap++)
            {
                StackPanel lapPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0)
                };

                TextBlock lapLabel = new TextBlock
                {
                    Text = $"Lap {lap}",
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 0),
                    //Width = 50
                };
                lapPanel.Children.Add(lapLabel);

                TextBox minTextBox = CreateTextBox(lap, "Min", "00", 1);
                minTextBox.TextChanged += MyTextBox_TextChanged;
                minTextBox.GotFocus += TextBox_GotFocus;
                minTextBox.LostFocus += TextBox_LostFocus;

                TextBox secTextBox = CreateTextBox(lap, "Sec", "00", 2);
                secTextBox.TextChanged += MyTextBox_TextChanged;
                secTextBox.GotFocus += TextBox_GotFocus;
                secTextBox.LostFocus += TextBox_LostFocus;

                TextBox msTextBox = CreateTextBox(lap, "Ms", "00", 2);
                msTextBox.TextChanged += MyTextBox_TextChanged;
                msTextBox.GotFocus += TextBox_GotFocus;
                msTextBox.LostFocus += TextBox_LostFocus;

                TextBlock avgSpeedTextBlock = new TextBlock
                {
                    Text = "",
                    Tag = $"avg_SpeedLap{lap}",
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(10, 0, 5, 0),
                    //Width = 70,
                    TextAlignment = TextAlignment.Center
                };


                msTextBox.PreviewLostKeyboardFocus += (s, e) => HandleLapMsPreviewLostKeyboardFocus(minTextBox, secTextBox, msTextBox, avgSpeedTextBlock);
                lapPanel.Children.Add(minTextBox);
                lapPanel.Children.Add(AddSymbolTextBlock(":"));
                lapPanel.Children.Add(secTextBox);
                lapPanel.Children.Add(AddSymbolTextBlock("."));
                lapPanel.Children.Add(msTextBox);
                lapPanel.Children.Add(avgSpeedTextBlock);
                DynamicRaceLapsStackPanel.Children.Add(lapPanel);
            }
        }

        private TextBox CreateTextBox(int lap, string type, string defaultText, int tabIndex)
        {
            return new TextBox
            {
                Name = $"Lap{lap}_{type}",
                Text = defaultText,
                MaxLength = 2, //type == "Ms" ? 3 : if back to miliseconds.
                TabIndex = tabIndex,
                MinWidth = 32,
                Margin = new Thickness(0, 0, 0, 0),
                AutoWordSelection = true
            };
        }

        private TextBlock AddSymbolTextBlock(string symbol)
        {
            return new TextBlock
            {
                Text = symbol,
                FontWeight = FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
        }
        // GotFocus event handler
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            FextBox_GotFocus((TextBox)sender);
        }

        private void FextBox_GotFocus(TextBox textBox)
        {
            // Clear the "00" value when the text box gains focus
            if (textBox.Text == "00" || textBox.Text == "000")
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
            if (string.IsNullOrEmpty(textBox.Text))
            {
                // If the TextBox is empty, reset to "00" or "000" based on type
                textBox.Text = textBox.Name.EndsWith("_Ms") ? "00" : "00";
            }

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
                if (textBox.Text.Length == 1)
                {
                    textBox.Text = "00" + textBox.Text;
                }
                else if (textBox.Text.Length == 2)
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
            if (string.IsNullOrEmpty(minTextBox.Text)) { return; }
            if (string.IsNullOrEmpty(secTextBox.Text)) { return; }
            if (string.IsNullOrEmpty(msTextBox.Text)) { return; }
            if (combo_Track.SelectedIndex == -1) { return; }

            int removeIndex = 0;
            double RaceOrLapDistance = 0;
            int laps = 0;
            if (race_Length.Text.Length > 0)
            {
                removeIndex = race_Length.Text.Length - 2;
                RaceOrLapDistance = Convert.ToDouble(race_Length.Text.Remove(removeIndex)) * 1000;
            }
            else
            {
                if (lap_Length.Text.Length == 0 || lap_Length.Text == "N/A") { return; }
                removeIndex = lap_Length.Text.Length - 2;
                RaceOrLapDistance = Convert.ToDouble(lap_Length.Text.Remove(removeIndex)) * 1000;
            }

            int msLength = msTextBox.Text.Length;
            if (msLength == 1)
            {
                msTextBox.Text = msTextBox.Text + "00";
            }
            else if (msLength == 2)
            {
                msTextBox.Text = msTextBox.Text + "0";
            }

            int ms = Convert.ToInt32(msTextBox.Text);
            int totalMs = (Convert.ToInt32(minTextBox.Text) * 60 * 1000) + (Convert.ToInt32(secTextBox.Text) * 1000) + ms;
            if (LapsTextBlock.Text == "N/A") {
                laps = 1; 
            } else {
                laps = Convert.ToInt32(LapsTextBlock.Text);
            }
            if (laps > 1)
            {
                RaceOrLapDistance = RaceOrLapDistance / laps;
            }
            if (totalMs > 0)
            {
                
                double averageSpeed = Math.Round(((RaceOrLapDistance * 3600000) / totalMs) / 1000, 2);
                avgSpeedTextBlock.Text = averageSpeed.ToString() + speed;
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


        private void calc_Total_Lap_Time()
        {
            if (LapsGroupBox.Visibility == Visibility.Collapsed || string.IsNullOrEmpty(LapsTextBox.Text) || LapsTextBox.Text == "N/A") return;
            int lapCount = Convert.ToInt32(LapsTextBox.Text);  // Number of laps, can be adjusted if needed
            int totalLapTime = 0;

            for (int i = 1; i <= lapCount; i++)
            {
                int lapTime = GetLapTimeInMilliseconds(i);

                if (lapTime == -1)
                {
                    MessageBox.Show($"Invalid time input for Lap {i}. Please enter valid numbers.");
                    return;
                }

                totalLapTime += lapTime;
            }

            DisplayTotalLapTime(totalLapTime);
        }

        private int GetLapTimeInMilliseconds(int lapNumber)
        {
            TextBox lapMinTextBox = (TextBox)FindName($"Lap{lapNumber}_Min");
            TextBox lapSecTextBox = (TextBox)FindName($"Lap{lapNumber}_Sec");
            TextBox lapMsTextBox = (TextBox)FindName($"Lap{lapNumber}_Ms");

            if (lapMinTextBox != null && lapSecTextBox != null && lapMsTextBox != null &&
                int.TryParse(lapMinTextBox.Text, out int minutes) &&
                int.TryParse(lapSecTextBox.Text, out int seconds) &&
                int.TryParse(lapMsTextBox.Text, out int milliseconds))
            {
                // Calculate lap time in milliseconds
                return (minutes * 60 * 1000) + (seconds * 1000) + milliseconds;
            }

            return -1;  // Return -1 to indicate an error
        }

        private void DisplayTotalLapTime(int totalLapTime)
        {
            if (totalLapTime > 0)
            {
                TimeSpan timeSpan = TimeSpan.FromMilliseconds(totalLapTime);
                string formattedTime = string.Format("{0:D2}:{1:D2}.{2:D3}",
                                                     timeSpan.Minutes + timeSpan.Hours * 60,
                                                     timeSpan.Seconds,
                                                     timeSpan.Milliseconds);
                Total_Time.Text = formattedTime;
            }
            else
            {
                Total_Time.Text = "00:00.000";
            }
        }
        private void calc_Average_Speed()
        {
            if (string.IsNullOrEmpty(LapsTextBlock.Text) || LapsTextBlock.Text == "N/A") return ;
            if(LapsGroupBox.Visibility == Visibility.Collapsed) { return; }
            // Convert race length from km to meters
            double totalDistance;
            if (RaceLengthGroupBox.Visibility == Visibility.Visible) {
                RemoveIndex = race_Length.Text.Length - 2;
                totalDistance = Convert.ToDouble(race_Length.Text.Remove(RemoveIndex)) * 1000;
            }
            else
            {
                RemoveIndex = lap_Length.Text.Length - 2;
                totalDistance = Convert.ToDouble(lap_Length.Text.Remove(RemoveIndex));
            }
            int totalLapTime = 0;
            int lapCount = Convert.ToInt32(LapsTextBlock.Text); // Adjust this if you have a different number of laps

            for (int i = 1; i <= lapCount; i++)
            {
                // Dynamically find the TextBoxes for each lap
                var lapMinTextBox = (TextBox)FindName($"Lap{i}_Min");
                var lapSecTextBox = (TextBox)FindName($"Lap{i}_Sec");
                var lapMsTextBox = (TextBox)FindName($"Lap{i}_Ms");

                // Check if the TextBoxes exist and contain valid numbers
                if (lapMinTextBox != null && lapSecTextBox != null && lapMsTextBox != null)
                {
                    if (int.TryParse(lapMinTextBox.Text, out int minutes) &&
                        int.TryParse(lapSecTextBox.Text, out int seconds) &&
                        int.TryParse(lapMsTextBox.Text, out int milliseconds))
                    {
                        milliseconds *= 10;  // Convert to milliseconds
                        int lapTime = (minutes * 60 * 1000) + (seconds * 1000) + milliseconds;

                        totalLapTime += lapTime;
                    }
                    else
                    {
                        // Handle parsing errors, e.g., show an error message
                        MessageBox.Show("Please enter valid lap times.");
                        return;
                    }
                }
            }

            // Calculate the average speed in km/h
            double averageSpeed = Math.Round((totalDistance * 3600000) / totalLapTime / 1000, 2);

            // Display the average speed
            Average_Speed.Text = averageSpeed.ToString() + speed;
        }
        private void calc_Lap_Length()
        {
            if (string.IsNullOrEmpty(combo_Type.Text)) { return; }
            if (string.IsNullOrEmpty(combo_Track.Text)) { return; }
            string trackId = combo_Track.Text.Replace("'","''");
            SQLiteConnection dbConn; // Declare the SQLiteConnection-Object
            dbConn = new SQLiteConnection(connectionString);
            try
            {
                string query = $"SELECT Length, Laps, RestrictedClass, RestrictedCar, MinSpeed, Checkpoints From [tracks] where Name ='{trackId}' ; ";
                if (dbConn.State == ConnectionState.Closed)
                {
                    dbConn.Open();
                }
                dbCmd = new SQLiteCommand(query, dbConn);
                reader = dbCmd.ExecuteReader();
                while (reader.Read())
                {
                    double value = Convert.ToDouble(reader[0].ToString());
                    int laps = Convert.ToInt32(reader[1].ToString());
                    string ClassRestriction = reader[2].ToString();
                    string VehicleRestriction = reader[3].ToString();
                    double MinimumSpeed = Convert.ToDouble(reader[4]);
                    int Checkpoints = Convert.ToInt32(reader[5].ToString());

                    if (!string.IsNullOrEmpty(ClassRestriction)) {
                        //ClassRestictionGroupBox.Visibility = Visibility.Visible;
                        ClassRestrictionTextBlock.Text = ClassRestriction; // Assuming carClass holds the value like "A1", "B2", etc.

                        string valued = ClassRestrictionTextBlock.Text;

                        combo_Class.SelectedValuePath = "Value";  // Set to the property name you're matching against
                        combo_Class.SelectedValue = valued;       // Set the value to match
                        combo_Class.IsEnabled = false;

                    }
                    else
                    {
                        ClassRestictionGroupBox.Visibility= Visibility.Collapsed;
                        ClassRestrictionTextBlock.Text = null;
                        combo_Class.IsEnabled = true;
                    }
                    if (!string.IsNullOrEmpty(VehicleRestriction))
                    {
                        VehicleRestictionGroupBox.Visibility = Visibility.Visible;
                        VehicleRestrictionTextBlock.Text = VehicleRestriction; // Assuming carClass holds the value like "A1", "B2", etc.

                        string valued = VehicleRestrictionTextBlock.Text;

                        combo_Vehicle.SelectedValuePath = "Value";  // Set to the property name you're matching against
                        combo_Vehicle.SelectedValue = valued;       // Set the value to match
                        BindVehicleComboBox(combo_Vehicle, valued);
                        combo_Vehicle.IsEnabled = false;
                    }
                    else
                    {
                        VehicleRestictionGroupBox.Visibility = Visibility.Collapsed;
                        VehicleRestrictionTextBlock.Text = null;
                        combo_Vehicle.IsEnabled = true;
                    }
                    if (SI == "Imperial")
                    {
                        value = Math.Round(value * onemile, 2);
                        MinimumSpeed = Math.Round(MinimumSpeed * onemile, 2);
                    }
                    lap_Length.Text = value.ToString() + distance;
                    if (MinimumSpeedGroupBox.Visibility == Visibility.Visible)
                    {
                        MinimumSpeedTextBlock.Text = MinimumSpeed.ToString() + speed;
                    }
                    else
                    {
                        MinimumSpeedTextBlock.Text = null;
                    }
                    if (CheckPointsGroupBox.Visibility == Visibility.Visible)
                    {
                        CheckpointsTextBlock.Text = Checkpoints.ToString();
                    }
                    else
                    {
                        CheckpointsTextBlock.Text = null;
                    }
                    if (laps > 0) {
                    LapsTextBlock.Text = laps.ToString();
                    race_Length.Text = Convert.ToString(value * laps) + distance;
                        if (laps == 1)
                        {
                            lap_Length.Text = "N/A";
                            LapsTextBlock.Text = "N/A";
                            race_Length.Text = value.ToString() + distance;
                        }
                        LapsGroupBox.Visibility = Visibility.Visible;
                        RaceLengthGroupBox.Visibility = Visibility.Visible;
                        RaceLengthGroupBox.Visibility = Visibility.Visible;
                        TrackLapRecordGroupbox.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        LapsGroupBox.Visibility = Visibility.Collapsed;
                        RaceLengthGroupBox.Visibility = Visibility.Collapsed;
                        TrackLapRecordGroupbox.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error" + ex.ToString());
                if (dbConn.State == ConnectionState.Open)
                {
                    dbConn.Close();
                }
            }
            finally
            {
                if (dbConn.State == ConnectionState.Open)
                {

                    dbConn.Close();
                }
            }
        }

        private int ConvertToMilliseconds(string minutes, string seconds, string milliseconds)
        {
            return (Convert.ToInt32(minutes) * 60 * 1000) +
                   (Convert.ToInt32(seconds) * 1000) +
                   (Convert.ToInt32(milliseconds));
        }

        private void calc_Average_Lap_Time()
        {
            // Collection of tuples representing lap time controls (minutes, seconds, milliseconds)
            var lapTimeControls = new List<(TextBox Min, TextBox Sec, TextBox Ms)>
            {
                (Lap1_Min, Lap1_Sec, Lap1_Ms),
                (Lap2_Min, Lap2_Sec, Lap2_Ms),
                (Lap3_Min, Lap3_Sec, Lap3_Ms),
                (Lap4_Min, Lap4_Sec, Lap4_Ms),
                (Lap5_Min, Lap5_Sec, Lap5_Ms)
            };

            int totalLapTime = 0;
            int lapCount = lapTimeControls.Count;

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
            //combo_Track.SelectedIndex = -1;
            //combo_Class.SelectedIndex = -1;
            //cb_conditions.IsChecked = false;
            //combo_Vehicle.SelectedIndex = -1;
            //keep.IsChecked = false;

            ResetControls(this);

            // Arrays of TextBoxes for laps and average lap speeds
            isCalculated = 0;
            TextBox[] lapMinutes = { Lap1_Min, Lap2_Min, Lap3_Min, Lap4_Min, Lap5_Min };
            TextBox[] lapSeconds = { Lap1_Sec, Lap2_Sec, Lap3_Sec, Lap4_Sec, Lap5_Sec };
            TextBox[] lapMilliseconds = { Lap1_Ms, Lap2_Ms, Lap3_Ms, Lap4_Ms, Lap5_Ms };
            TextBlock[] avgLapSpeeds = { avg_SpeedLap1, avg_SpeedLap2, avg_SpeedLap3, avg_SpeedLap4, avg_SpeedLap5 };

            // Reset lap times and average lap speeds
            for (int i = 0; i < lapMinutes.Length; i++)
            {
                lapMinutes[i].Text = "00";
                lapSeconds[i].Text = "00";
                lapMilliseconds[i].Text = "000";
                avgLapSpeeds[i].Text = "0.0";
            }
            combo_Type.SelectedIndex = -1;
            combo_Track.SelectedIndex = -1;

            combo_Class.SelectedIndex = -1;
            lap_Length.Text = "";
            LapLengthGroupBox.Visibility = Visibility.Collapsed;
            race_Length.Text = "";
            RaceLengthGroupBox.Visibility = Visibility.Collapsed;
            txt_odometer.Text = "";
            txt_lrec.Text = "";
            txt_lrecCar.Text = "";
            txt_rRan.Text = "";
            txt_Progress_1.Text = "";
            txt_Progress_2.Text = "";
            txt_carPB.Text = "";
            average_LapTime.Text = "";
            Total_Time.Text = "";
            Average_Speed.Text = "";

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


            PrepareSI(out string odo,
                 out string otherSiName,
                 out double calcOtherSIOdometer,
                 out double calcOtherSIAVG,
                 out string otherSIHAVG,
                 out string otherSIAVG,
                 out string havg,
                 out string avg,
                 out string epidia);

            if (string.IsNullOrEmpty(combo_Track.Text)) { return; }
            int trackId = Convert.ToInt32(combo_Track.SelectedValue);

            using (var dbConn = new SQLiteConnection(connectionString))
            {
                dbConn.Open();
                using (var dbCmd = new SQLiteCommand(BuildQuery(trackId, odo, otherSiName, calcOtherSIOdometer, otherSIHAVG, otherSIAVG, havg, avg, epidia), dbConn))
                {
                    dbCmd.ExecuteNonQuery();
                }
            }

            ResetLapFields();

            if (!keep.IsChecked.GetValueOrDefault())
                ResetAllFields();

            Total_Time.Text = string.Empty;

            loadLapRecord();
            CheckProgress();

            if (keep.IsChecked == true)
                LoadNextVehicleData(odo);

            UpdateViewEntries();
            isCalculated = 0;
        }

        private bool ValidateSelections()
        {
            return combo_Track.SelectedIndex >= 0 && combo_Class.SelectedIndex >= 0 && combo_Vehicle.SelectedIndex >= 0;
        }

        private void PrepareSI(out string odo, out string otherSiName, out double calcOtherSIOdometer, out double calcOtherSIAVG,
                               out string otherSIHAVG, out string otherSIAVG, out string havg, out string avg, out string epidia)
        {
            string answer = Average_Speed.Text.Substring(0, Average_Speed.Text.Length - speed.Length);
            if (SI == "Metric")
            {
                odo = "_odometer_metric";
                otherSiName = "_odometer_imperial";
                calcOtherSIOdometer = Math.Round(Convert.ToDouble(txt_odometer.Text) * onemile, 2);
                havg = "Higher_Avg_Metric";
                avg = "Average_Speed_Metric";
                otherSIHAVG = "Higher_Avg_Imperial";
                otherSIAVG = "Average_Speed_Imperial";
                epidia = "*";
                calcOtherSIAVG = Math.Round(Convert.ToDouble(answer) * onemile, 2);
            }
            else
            {
                odo = "_odometer_imperial";
                otherSiName = "_odometer_metric";
                calcOtherSIOdometer = Math.Round(Convert.ToDouble(txt_odometer.Text) / onemile, 2);
                havg = "Higher_Avg_Imperial";
                avg = "Average_Speed_Imperial";
                otherSIHAVG = "Higher_Avg_Metric";
                otherSIAVG = "Average_Speed_Metric";
                epidia = "/";
                calcOtherSIAVG = Math.Round(Convert.ToDouble(answer) / onemile, 2);
            }
        }

        private string BuildQuery(int trackId, string odo, string otherSiName, double calcOtherSIOdometer, string otherSIHAVG, string otherSIAVG,
                                  string havg, string avg, string epidia)
        {
            return $@"
        INSERT INTO [records] 
        (trackId, carName, carClass, conditions, orientation, Lap1, Lap2, Lap3, Lap4, Lap5, Average_Lap, Total_Time, {avg}, Avg_Lap1, Avg_Lap2, Avg_Lap3, Avg_Lap4, Avg_Lap5) 
        VALUES 
        ('{trackId}', '{combo_Vehicle.Text.Replace("'", "''")}', '{combo_Class.Text}', '{(cb_conditions.IsChecked == true ? 1 : 0)}', '{(cb_orientation.IsChecked == true ? 1 : 0)}',  
        '{Lap1_Min.Text}:{Lap1_Sec.Text}.{Lap1_Ms.Text}', '{Lap2_Min.Text}:{Lap2_Sec.Text}.{Lap2_Ms.Text}', 
        '{Lap3_Min.Text}:{Lap3_Sec.Text}.{Lap3_Ms.Text}', '{Lap4_Min.Text}:{Lap4_Sec.Text}.{Lap4_Ms.Text}', 
        '{Lap5_Min.Text}:{Lap5_Sec.Text}.{Lap5_Ms.Text}','{average_LapTime.Text}', '{Total_Time.Text}', '{Average_Speed.Text.Substring(0, Average_Speed.Text.Length - speed.Length)}', 
        '{avg_SpeedLap1.Text}', '{avg_SpeedLap2.Text}', '{avg_SpeedLap3.Text}', '{avg_SpeedLap4.Text}', '{avg_SpeedLap5.Text}');
        
        UPDATE [records] 
        SET Fastest_Lap = Min(Lap1, Lap2, Lap3, Lap4, Lap5);

        UPDATE [records] 
        SET {havg} = Max(Avg_Lap1, Avg_Lap2, Avg_Lap3, Avg_Lap4, Avg_Lap5) 
        WHERE carName = '{combo_Vehicle.Text.Replace("'", "''")}' AND trackId = '{trackId}';

        UPDATE [records] 
        SET {otherSIHAVG} = Round({havg} {epidia} {onemile}, 2) 
        WHERE carName = '{combo_Vehicle.Text.Replace("'", "''")}' AND trackId = '{trackId}';

        UPDATE [records] 
        SET {otherSIAVG} = Round({avg} {epidia} {onemile}, 2) 
        WHERE carName = '{combo_Vehicle.Text.Replace("'", "''")}' AND trackId = '{trackId}';

        UPDATE [tracks] 
        SET Runs = Runs + 1 
        WHERE id = '{trackId}';

        UPDATE [vehicles] 
        SET {odo} = '{Convert.ToDouble(txt_odometer.Text)}', _races_ran = _races_ran + 1 
        WHERE _vehicle_name = '{combo_Vehicle.Text.Replace("'", "''")}';

        UPDATE [vehicles] 
        SET {otherSiName} = '{calcOtherSIOdometer}' 
        WHERE _vehicle_name = '{combo_Vehicle.Text.Replace("'", "''")}'";

        }

        private void ResetLapFields()
        {
            TextBox[] mins = { Lap1_Min, Lap2_Min, Lap3_Min, Lap4_Min, Lap5_Min };
            TextBox[] secs = { Lap1_Sec, Lap2_Sec, Lap3_Sec, Lap4_Sec, Lap5_Sec };
            TextBox[] ms = { Lap1_Ms, Lap2_Ms, Lap3_Ms, Lap4_Ms, Lap5_Ms };
            TextBlock[] speedLaps = { avg_SpeedLap1, avg_SpeedLap2, avg_SpeedLap3, avg_SpeedLap4, avg_SpeedLap5 };

            for (int i = 0; i < Convert.ToInt32(LapsTextBlock.Text); i++)
            {
                mins[i].Text = "00";
                secs[i].Text = "00";
                ms[i].Text = "00";
                speedLaps[i].Text = "0.0";
            }
        }

        private void ResetAllFields()
        {
            combo_Type.SelectedIndex = -1;
            combo_Track.SelectedIndex = -1;
            combo_Class.SelectedIndex = -1;
            LapLengthGroupBox.Visibility = Visibility.Collapsed;
            RaceLengthGroupBox.Visibility = Visibility.Collapsed;
            race_Length.Text = string.Empty;
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
            Total_Time.Text = string.Empty;
            Average_Speed.Text = string.Empty;
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

        private void combo_Class_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!combo_Class.IsLoaded || combo_Class.Items.Count == 0 || combo_Class.SelectedIndex < 0) return;
            if (e.AddedItems.Count == 0) return;  // Ensure the selection actually changed
            BindVehicleComboBox(combo_Vehicle);

        }

        private void HandleComboClassChange()
        {
            if (VehicleRestictionGroupBox.Visibility == Visibility.Collapsed)
            {
                BindVehicleComboBox(combo_Vehicle);
                combo_Vehicle.IsEnabled = true;
            }
            else
            {
                BindVehicleComboBox(combo_Vehicle, VehicleRestrictionTextBlock.Text);
                combo_Vehicle.IsEnabled = false;
            }
            if (ClassRestictionGroupBox.Visibility == Visibility.Collapsed)
            {
                combo_Class.IsEnabled = true;
            }
            else
            {
                combo_Class.IsEnabled = false;
            }
            CheckProgress();
            loadLapRecord();

            bool isClassSelected = combo_Class.SelectedIndex >= 1;
            UpdateUIForSI(isClassSelected);
        }

        private void UpdateUIForSI(bool isClassSelected)
        {
                btn_loadrec_Metric.Content = isClassSelected ? "Load _vehiclecategory_name Records" : "Load Track Records";
                TrackLapRecordGroupbox.Header = isClassSelected ? "Class Lap Record" : "Track Lap Record";
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


        private void loadLapRecord()
        {
            // Hide all related UI elements initially
            orientationA_lrec.Visibility = Visibility.Collapsed;
            orientationC_lrec.Visibility = Visibility.Collapsed;
            weatherS_lrec.Visibility = Visibility.Collapsed;
            weatherR_lrec.Visibility = Visibility.Collapsed;

            // Check if the track is selected
            if (string.IsNullOrEmpty(combo_Track.Text)) return;

            int conditions = cb_conditions.IsChecked == true ? 1 : 0;
            int orientation = cb_orientation.IsChecked == true ? 1 : 0;
            int trackId = Convert.ToInt32(combo_Track.SelectedValue);
            string spec = combo_Class.Text;

            using (var dbConn = new SQLiteConnection(connectionString))
            {
                dbConn.Open();

                string query = !string.IsNullOrEmpty(combo_Class.Text)
                    ? $"SELECT carName, Min(Fastest_Lap), conditions, orientation FROM [records] WHERE carClass = '{spec}' AND trackId = '{trackId}';"
                    : $"SELECT carName, Min(Fastest_Lap), conditions, orientation FROM [records] WHERE trackId = '{trackId}';";

                using (var dbCmd = new SQLiteCommand(query, dbConn))
                using (var reader = dbCmd.ExecuteReader())
                {
                    if (!reader.Read() || reader.IsDBNull(0))
                    {
                        txt_lrec.Text = "No Record";
                        txt_lrecCar.Text = "No Record";
                        return;
                    }

                    string carName = reader.IsDBNull(0) ? "No Record" : reader.GetString(0);
                    string value = reader.IsDBNull(1) ? "No Record" : reader.GetString(1);
                    int weather = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                    int orient = reader.IsDBNull(3) ? 0 : reader.GetInt32(3);

                    txt_lrec.Text = value;
                    txt_lrecCar.Text = carName;

                    // Weather conditions
                    if (weather == 0)
                    {
                        weatherS_lrec.Visibility = Visibility.Visible;
                    }
                    else if (weather == 1)
                    {
                        weatherR_lrec.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        weatherS_lrec.Visibility = Visibility.Collapsed;
                        weatherR_lrec.Visibility = Visibility.Collapsed;
                    }

                    // Orientation
                    if (orient == 0)
                    {
                        orientationC_lrec.Visibility = Visibility.Visible;
                    }
                    else if (orient == 1)
                    {
                        orientationA_lrec.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        orientationA_lrec.Visibility = Visibility.Collapsed;
                        orientationC_lrec.Visibility = Visibility.Collapsed;
                    }

                    // Record type
                    string recordX;
                    switch (combo_Class.SelectedIndex)
                    {
                        case -1:
                        case 0:
                            recordX = "Track_Lap_Record";
                            break;
                        case 1:
                            recordX = "Record_A1";
                            break;
                        case 2:
                            recordX = "Record_A2";
                            break;
                        case 3:
                            recordX = "Record_A3";
                            break;
                        case 4:
                            recordX = "Record_A4";
                            break;
                        case 5:
                            recordX = "Record_A5";
                            break;
                        case 6:
                            recordX = "Record_A6";
                            break;
                        case 7:
                            recordX = "Record_A7";
                            break;
                        case 8:
                            recordX = "Record_B2";
                            break;
                        case 9:
                            recordX = "Record_B3";
                            break;
                        case 10:
                            recordX = "Record_B4";
                            break;
                        case 11:
                            recordX = "Record_C1";
                            break;
                        case 12:
                            recordX = "Record_C2";
                            break;
                        case 13:
                            recordX = "Record_C3";
                            break;
                        case 14:
                            recordX = "Record_C4";
                            break;
                        case 15:
                            recordX = "Record_MA1";
                            break;
                        case 16:
                            recordX = "Record_MA2";
                            break;
                        default:
                            recordX = "Track_Lap_Record";
                            break;
                    }

                    if (!string.IsNullOrEmpty(carName) && !string.IsNullOrEmpty(value))
                    {
                        string trackName = combo_Track.Text.Replace("'", "''");
                        string updateQuery = $"UPDATE [tracks] SET {recordX} = '{carName} - {txt_lrec.Text}' WHERE Name = '{trackName}'";

                        using (var updateCmd = new SQLiteCommand(updateQuery, dbConn))
                        {
                            updateCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }


        private void FillComboBoxWithTracks(ComboBox comboBox)
        {
            ComboBoxItem selectedItem = (ComboBoxItem)combo_Type.SelectedItem;
            string selectedRaceType = selectedItem?.Value;
            string query = $"SELECT * FROM tracks WHERE RaceType = '{selectedRaceType}'";
            //MessageBox.Show(query);
            ExecuteQuery(query, "tracks", dataSet =>
            {
                SetComboBoxSource(comboBox, dataSet, "Name", "id");
            },
            ex => MessageBox.Show($"An error occurred while loading tracks:\n{ex.Message}"));
        }

        private void BindVehicleComboBox(ComboBox comboBox, string selectedId = null)
        {
            //if(comboBox.SelectedIndex == -1) { return; }

            string query = "SELECT * FROM vehicles WHERE _is_purchasable = 'true' AND _is_active = 'true' AND _is_owned = 'true'";

            if (combo_Class.SelectedIndex > -1)
            {
                ComboBoxItem selectedItem = (ComboBoxItem)combo_Class.SelectedItem;
                string selectedValue = selectedItem.Value;
                if (selectedValue != "All")
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
                    ClearVehicleDetails();
                    SetComboBoxSource(comboBox, dataSet, "_vehicle_name", "id");
                    // If a specific ID is provided, select the corresponding item
                    if (!string.IsNullOrEmpty(selectedId))
                    {
                        comboBox.SelectedValue = selectedId;
                    }
                    else if (combo_Class.SelectedIndex > -1)
                    {
                        comboBox.SelectedIndex = 0; // Default to the first item if no ID is provided
                    }
                }
                else
                {
                    Popup++;
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

        private void ClearVehicleDetails()
        {
            txt_odometer.Text = "";
            txt_carPB.Text = "";
            txt_rRan.Text = "";
        }

        private void Btn_submit_GotFocus(object sender, RoutedEventArgs e)
        {
            //calc_Average_Lap_Time();
            //calc_Total_Lap_Time();
            //calc_Average_Speed();
        }

        private void Btn_loadrec_Click(object sender, RoutedEventArgs e)
        {
            if(combo_Track.SelectedIndex == -1) { return; }
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

            FetchAndDisplayVehicleData();
        }


        private void combo_Type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            isCalculated = 0;
            HideAll();
            FillComboBoxWithTracks(combo_Track);
        }

        // Handles the logic when the Combo_Track loses focus.
        private void Combo_Track_LostFocus(object sender, RoutedEventArgs e)
        {
            // Call HandleTrackChange to update the UI based on the track selection
            HandleTrackChange();
            FinalizeUI();
        }
        // Handles the logic when a key is released while the Combo_Track is focused.
        private void Combo_Track_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            // Call HandleTrackChange to update the UI and clear txt_carPB when a key is pressed
            HandleTrackChange(clearCarPB: true);
            FinalizeUI();
        }

        private void Combo_Track_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Ensure that the selection change is valid before proceeding
            if (!combo_Track.IsLoaded || combo_Track.Items.Count == 0 || combo_Track.SelectedIndex < 0 || e.AddedItems.Count == 0)
                return;
            if (LapsTextBlock.Text == "N/A" || LapsGroupBox.Visibility == Visibility.Hidden || string.IsNullOrEmpty(LapsTextBlock.Text))
            {
                return;
            } else {

                int NoLaps = Convert.ToInt32(LapsTextBlock.Text);
                GenerateLapsInStackPanel(NoLaps);
            }
            // Call HandleTrackChange to update the UI based on the track selection
            HandleTrackChange();

            // Update UI elements based on the selected race type
            UpdateRaceTypeUI();
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

            // Update UI elements based on the track selection
            UpdateTrackUI();
            FinalizeUI();

            // Optionally clear txt_carPB based on the flag
            if (clearCarPB)
                txt_carPB.Text = "";

            // Fetch and display vehicle data if a vehicle is selected
            if (!string.IsNullOrEmpty(combo_Vehicle.Text))
                FetchAndDisplayVehicleData();
        }

        // Hides all UI elements and resets their associated text fields.
        private void HideAll()
        {
            if (isCalculated == 0) { 
            // Hide and reset all UI elements
            ClassRestictionGroupBox.Visibility = Visibility.Collapsed;
            ClassRestrictionTextBlock.Text = "";
            VehicleRestictionGroupBox.Visibility = Visibility.Collapsed;
            VehicleRestrictionTextBlock.Text = "";
            OrientationGroupBox.Visibility = Visibility.Collapsed;
            LapLengthGroupBox.Visibility = Visibility.Collapsed;
            lap_Length.Text = "";
            LapsGroupBox.Visibility = Visibility.Collapsed;
            LapsTextBlock.Text = "";
            RaceLengthGroupBox.Visibility = Visibility.Collapsed;
            race_Length.Text = "";
            TrackLapRecordGroupbox.Visibility = Visibility.Collapsed;
            txt_lrecCar.Text = "";
            weatherS_lrec.Visibility = Visibility.Collapsed;
            weatherR_lrec.Visibility = Visibility.Collapsed;
            orientationC_lrec.Visibility = Visibility.Collapsed;
            orientationA_lrec.Visibility = Visibility.Collapsed;
            PointsGroupBox.Visibility = Visibility.Collapsed;
            PointsTextBlock.Text = "";
            MinimumSpeedGroupBox.Visibility = Visibility.Collapsed;
            MinimumSpeedTextBlock.Text = "";
            CheckPointsGroupBox.Visibility = Visibility.Collapsed;
            CheckpointsTextBlock.Text = "";
            }
        }

        // Updates the UI based on the selected race type.
        // Update UI elements related to the selected race type.
        private void UpdateRaceTypeUI()
        {
            // Ensure that combo_Type is not null and has selected items
            if (isCalculated == 0) {
                if (combo_Track.SelectedIndex > -1)
                {
                    if (combo_Type.SelectedItem is ComboBoxItem selectedItem)
                    {
                        string selectedRaceType = selectedItem.Value;
                        // Determine event types
                        bool isRace = selectedRaceType == "Race SP" || selectedRaceType == "Race RMP";
                        bool isEliminator = selectedRaceType.StartsWith("Eli");
                        bool isTimeAttack = selectedRaceType == "Time Attack SP";
                        bool isSpeedEvent = selectedRaceType == "Speed SP" || selectedRaceType == "Speed MP";
                        bool isSpeedTrap = selectedRaceType == "Speed Trap SP" || selectedRaceType == "Speed Trap MP";
                        bool isFYLEvent = selectedRaceType == "Follow The Leader MP";
                        bool isKYDEvent = selectedRaceType == "Keep Your Distance MP";

                        // Define visibility conditions
                        bool hasRestrictedClass = isRace || isEliminator || isTimeAttack || isSpeedEvent || isSpeedTrap || isFYLEvent || isKYDEvent;
                        bool hasRestrictedVehicle = isRace || isEliminator || isTimeAttack || isSpeedEvent || isSpeedTrap || isFYLEvent || isKYDEvent;
                        bool hasOrientation = selectedRaceType == "Race MP";
                        bool hasDistance = isRace || isEliminator || isTimeAttack;
                        bool hasLaps = isRace || isEliminator || isTimeAttack; // Laps for race-related types
                        bool hasTrackLapRecord = isRace || isEliminator || isTimeAttack;
                        bool hasScore = isSpeedEvent || isSpeedTrap || isFYLEvent || isKYDEvent;

                        // Update the headers and visibility of the UI elements based on the selected race type
                        TrackLapRecordGroupbox.Header = hasTrackLapRecord ? "Track Lap Record" : "Fastest Time";
                        TrackLapRecordGroupbox.Visibility = hasTrackLapRecord ? Visibility.Visible : Visibility.Collapsed;
                        OrientationGroupBox.Visibility = hasOrientation ? Visibility.Visible : Visibility.Collapsed;
                        LapLengthGroupBox.Visibility = hasDistance ? Visibility.Visible : Visibility.Collapsed;
                        LapsGroupBox.Visibility = hasLaps ? Visibility.Visible : Visibility.Collapsed;
                        RaceLengthGroupBox.Visibility = hasDistance ? Visibility.Visible : Visibility.Collapsed;

                        // Correct visibility logic for Speed Event types
                        MinimumSpeedGroupBox.Visibility = isSpeedEvent ? Visibility.Visible : Visibility.Collapsed;
                        PointsGroupBox.Visibility = isSpeedEvent ? Visibility.Visible : Visibility.Collapsed;
                        CheckPointsGroupBox.Visibility = isSpeedTrap ? Visibility.Visible : Visibility.Collapsed;
                        VehicleRestrictionTextBlock.Visibility = hasRestrictedVehicle ? Visibility.Visible : Visibility.Collapsed;
                        ClassRestictionGroupBox.Visibility = hasRestrictedVehicle ? Visibility.Visible : Visibility.Collapsed;

                        isCalculated++;
                        // Show a message box with the details and visibility status for debugging purposes
                        //MessageBox.Show(
                        //    $"Race type: {selectedRaceType}\n" +
                        //    $"Is _vehiclecategory_name Restricted? {hasRestrictedClass} (Visibility: {(ClassRestictionGroupBox.Visibility == Visibility.Visible ? "Visible" : "Collapsed")})\n" +
                        //    $"Is Vehicle Restricted? {hasRestrictedVehicle} (Visibility: {(VehicleRestictionGroupBox.Visibility == Visibility.Visible ? "Visible" : "Collapsed")})\n" +
                        //    $"Is it Race? {isRace} (Visibility: {(RaceLengthGroupBox.Visibility == Visibility.Visible ? "Visible" : "Collapsed")})\n" +
                        //    $"Is it Eliminator? {isEliminator}\n" +
                        //    $"Is it Time Attack? {isTimeAttack}\n" +
                        //    $"Is it Speed Event? {isSpeedEvent} (Visibility: {(MinimumSpeedGroupBox.Visibility == Visibility.Visible ? "Visible" : "Collapsed")})\n" +
                        //    $"Is it Speed Trap? {isSpeedTrap} (Visibility: {(CheckPointsdGroupBox.Visibility == Visibility.Visible ? "Visible" : "Collapsed")})\n" +
                        //    $"Is it Follow Your Leader? {isFYLEvent}\n" +
                        //    $"Is it Keep Your Distance? {isKYDEvent}\n" +
                        //    $"Has Lap Record? {hasTrackLapRecord} (Visibility: {(TrackLapRecordGroupbox.Visibility == Visibility.Visible ? "Visible" : "Collapsed")})\n" +
                        //    $"Has Orientation? {hasOrientation} (Visibility: {(OrientationGroupBox.Visibility == Visibility.Visible ? "Visible" : "Collapsed")})\n" +
                        //    $"Has Distance? {hasDistance} (Visibility: {(LapLengthGroupBox.Visibility == Visibility.Visible ? "Visible" : "Collapsed")})");
                    }
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
            calc_Lap_Length();   // Calculates the lap length based on the selected track
            CheckProgress();     // Checks the current progress or state of the race/track
            loadLapRecord();     // Loads the lap record for the selected track
        }

        // Fetches and displays vehicle data based on the selected track and vehicle.
        // Fetches and displays vehicle data based on the selected track and vehicle.
        private void FetchAndDisplayVehicleData()
        {
            // Checks if a valid vehicle and track are selected
            if (string.IsNullOrEmpty(combo_Vehicle.Text) || string.IsNullOrEmpty(combo_Track.Text)) return;

            int conditions = cb_conditions.IsChecked == true ? 1 : 0;
            int orientation = cb_orientation.IsChecked == false ? 0 : 1;
            string veh = combo_Vehicle.Text.Replace("'", "''");
            int trackId = Convert.ToInt32(combo_Track.SelectedValue);
            string odoColumn = SI == "Metric" ? "_odometer_metric" : "_odometer_imperial";
            double conversionFactor = SI == "Imperial" ? 1.60934 : 1.0;

            // SQL query to fetch odometer reading, fastest lap, and the number of records for the vehicle and track
            string query = $@"
                            SELECT 
                            (SELECT {odoColumn} FROM vehicles WHERE _vehicle_name = '{veh}'),
                            (SELECT Fastest_Lap FROM records WHERE carName = '{veh}' AND trackId = {trackId}),
                            (SELECT COUNT(*) FROM records WHERE trackId = {trackId} AND carName = '{veh}' AND conditions = {conditions} AND orientation = {orientation})";

            try
            {
                // Execute the query and fetch data from the database
                using (var dbConn = new SQLiteConnection(connectionString))
                {
                    dbConn.Open();
                    using (var dbCmd = new SQLiteCommand(query, dbConn))
                    {
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


        private void CheckProgress()
        {
            txt_Progress_1.Text = "";
            txt_Progress_2.Text = "";
            if (string.IsNullOrEmpty(combo_Track.Text)) { return; }
            int trackId = Convert.ToInt32(combo_Track.SelectedValue);
            int conditions = cb_conditions.IsChecked == true ? 1 : 0;
            int orientation = cb_orientation.IsChecked == true ? 1 : 0;
            string param ="";
            if (ClassRestrictionTextBlock.Visibility == Visibility.Visible && ClassRestrictionTextBlock.Text.Length > 0)
            {
                param = ClassRestrictionTextBlock.Text;
            }
            double percentage;

            try
            {
                using (var dbConn = new SQLiteConnection(connectionString))
                {
                    dbConn.Open();

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
                        txt_Progress_1.Text = $"0/"+carsCount;
                        txt_Progress_2.Text = "0%";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.ToString());
            }
        }

        private int GetRecordCount(SQLiteConnection dbConn, int trackId, int conditions, int orientation, string param)
        {
            string query = string.IsNullOrEmpty(param)
                ? $"SELECT COUNT(*) FROM [records] WHERE trackId = '{trackId}' AND conditions = '{conditions}' and orientation = '{orientation}' ;"
                : $"SELECT COUNT(*) FROM [records] WHERE trackId = '{trackId}' AND carClass = '{param}' AND conditions = '{conditions}' ;";

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
                ? "SELECT COUNT(*) FROM [vehicles] WHERE _is_active = 'true' and _is_purchasable = 'true' and _is_owned = 'true';"
                : $"SELECT COUNT(*) FROM [vehicles] WHERE _vehiclecategory_name = '{param}' and _is_active = 'true' and _is_purchasable = 'true' and _is_owned = 'true';";

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

        //// Preview text input to allow only specific characters (e.g., numerical input)
        //private new void PreviewTextInput(object sender, TextCompositionEventArgs e)
        //{
        //    e.Handled = !IsTextAllowed(e.Text);
        //}

        //// Preview text input to disallow decimal points (e.g., for integer input)
        //private void PreviewNoDecInput(object sender, TextCompositionEventArgs e)
        //{
        //    e.Handled = !IsDecAllowed(e.Text);
        //}

        public int RemoveIndex { get; private set; }

        // Define regex patterns for allowed/disallowed text
        private static readonly Regex _regexNoDec = new Regex("[^0-9]+"); // Allows only numeric characters (no decimals)

        // Check if the input text is allowed (e.g., only numerical characters)
        //private static bool IsTextAllowed(string text)
        //{
        //    return !_regex.IsMatch(text);
        //}

        // Check if the input text is allowed without decimal points
        private static bool IsDecAllowed(string text)
        {
            return !_regexNoDec.IsMatch(text);
        }


        //private void HandleLapMsPreviewLostKeyboardFocus(TextBox minTextBox, TextBox secTextBox, TextBox msTextBox, TextBlock avgSpeedTextBlock)
        //{
        //    if (string.IsNullOrEmpty(minTextBox.Text)) { return; }
        //    if (string.IsNullOrEmpty(secTextBox.Text)) { return; }
        //    if (string.IsNullOrEmpty(msTextBox.Text)) { return; }
        //    if (combo_Track.SelectedIndex == -1) { return; }
        //    int removeIndex = 0;
        //    double RaceOrLapDistance = 0;
        //    if (race_Length.Text.Length > 0)
        //    {
        //        removeIndex = race_Length.Text.Length - 2;
        //        RaceOrLapDistance = Convert.ToDouble(race_Length.Text.Remove(removeIndex)) * 1000;
        //    }
        //    else { 
        //    //LapLengthGroupBox.Visibility = Visibility.Visible;
        //    if (lap_Length.Text.Length == 0 || lap_Length.Text == "N/A" ) { return; }
        //        removeIndex = race_Length.Text.Length - 2;
        //        //MessageBox.Show($"Lap Length: {lap_Length.Text.Remove(removeIndex)},\nTime: {minTextBox.Text}:{secTextBox.Text}.{msTextBox.Text}");
        //        RaceOrLapDistance = Convert.ToDouble(lap_Length.Text.Remove(removeIndex)) * 1000;
        //    }
        //    int msLength = msTextBox.Text.Length;

        //    if (msLength == 1)
        //    {
        //        msTextBox.Text = msTextBox.Text + "00";
        //    }
        //    else if (msLength == 2)
        //    {
        //        msTextBox.Text = msTextBox.Text + "0";
        //    }

        //    int ms = Convert.ToInt32(msTextBox.Text);
        //    int totalMs = (Convert.ToInt32(minTextBox.Text) * 60 * 1000) + (Convert.ToInt32(secTextBox.Text) * 1000) + ms;

        //    if (totalMs > 0)
        //    {
        //        double averageSpeed = Math.Round(((RaceOrLapDistance * 3600000) / totalMs) / 1000, 2);
        //        avgSpeedTextBlock.Text = averageSpeed.ToString();
        //    }
        //}
        private void Lap1_Ms_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            HandleLapMsPreviewLostKeyboardFocus(Lap1_Min, Lap1_Sec, Lap1_Ms, avg_SpeedLap1);
        }

        private void Lap2_Ms_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            HandleLapMsPreviewLostKeyboardFocus(Lap2_Min, Lap2_Sec, Lap2_Ms, avg_SpeedLap2);
        }

        private void Lap3_Ms_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            HandleLapMsPreviewLostKeyboardFocus(Lap3_Min, Lap3_Sec, Lap3_Ms, avg_SpeedLap3);
        }

        private void Lap4_Ms_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            HandleLapMsPreviewLostKeyboardFocus(Lap4_Min, Lap4_Sec, Lap4_Ms, avg_SpeedLap4);
        }

        private void Lap5_Ms_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            HandleLapMsPreviewLostKeyboardFocus(Lap5_Min, Lap5_Sec, Lap5_Ms, avg_SpeedLap5);
        }

        private void ResetAvgSpeedText(object sender, KeyboardFocusChangedEventArgs e, TextBlock avgSpeedTextBlock)
        {
            avgSpeedTextBlock.Text = "0.0";
        }
        private void Lap1_Ms_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ResetAvgSpeedText(sender, e, avg_SpeedLap1);
        }

        private void Lap2_Ms_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ResetAvgSpeedText(sender, e, avg_SpeedLap2);
        }

        private void Lap3_Ms_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ResetAvgSpeedText(sender, e, avg_SpeedLap3);
        }

        private void Lap4_Ms_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ResetAvgSpeedText(sender, e, avg_SpeedLap4);
        }

        private void Lap5_Ms_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ResetAvgSpeedText(sender, e, avg_SpeedLap5);
        }
        private void LapMs_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {

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

            if (string.IsNullOrEmpty(combo_Track.Text)) { return; }
            int trackId = Convert.ToInt32(combo_Track.SelectedValue);
            using (var dbConn = new SQLiteConnection(connectionString))
            {
                dbConn.Open();
                try
                {
                    string query = $@"SELECT Name,
                                            Runs,
                                            Record_A1,
                                            Record_A2,
                                            Record_A3,
                                            Record_A4,
                                            Record_A5,
                                            Record_A6,
                                            Record_A7,
                                            Record_B1,
                                            Record_B2,
                                            Record_B3,
                                            Record_B4,
                                            Record_C1,
                                            Record_C2,
                                            Record_C3,
                                            Record_C4,
                                            Record_MA1,
                                            Record_MA2 FROM [tracks] WHERE id = '{trackId}'";
                    using (var dbCmd = new SQLiteCommand(query, dbConn))
                    using (var reader = dbCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var labels = new[]
                            {
                        "Record A1", "Record A2", "Record A3", "Record A4", "Record A5", "Record A6", "Record A7",
                        "Record B1", "Record B2", "Record B3", "Record B4", "Record C1", "Record C2", "Record C3", "Record C4",
                        "Record MA1", "Record MA2"
                    };

                            var values = new List<string>();
                            for (int i = 0; i < reader.FieldCount && i < 20; i++)
                            {
                                if (!reader.IsDBNull(i))
                                {
                                    values.Add(reader[i].ToString());
                                }
                                else
                                {
                                    values.Add("No Record Yet");
                                }
                            }

                            // Ensure values list has the same length as labels list
                            while (values.Count < labels.Length)
                            {
                                values.Add("No Record Yet");
                            }

                            var sb = new StringBuilder();
                            sb.AppendLine($"Track Name: {reader[0]}")
                              .AppendLine($"Total Runs: {reader[1]}");

                            for (int i = 2; i < labels.Length; i++)
                            {
                                string value = values[i] == string.Empty ? "No Record Yet" : values[i];
                                sb.AppendLine($"-----------------\n{labels[i]}:\n{value}");
                            }

                            MessageBox.Show(sb.ToString(), reader[1] + " Records", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.ToString());
                }
            }
        }

        private void FillDataGrid()
        {
            string avgSIName, avgSIRaceSpeed;
            string[] headers = { "Name", "Class", "Fastest Lap", "Lap Rec. Speed", "Average Lap", "Race Time" };
            int conditions = cb_conditions.IsChecked == true ? 1 : 0;
            int orientation = cb_orientation.IsChecked == true ? 1 : 0;

            if (string.IsNullOrEmpty(combo_Track.Text)) { return; }
            int trackId = Convert.ToInt32(combo_Track.SelectedValue);
            string carClass = combo_Class.Text;
            string dbTable = "records";

            // Set SI type
            if (SI == "Metric")
            {
                avgSIName = "Higher_Avg_Metric";
                avgSIRaceSpeed = "Average_Speed_Metric";
            }
            else
            {
                avgSIName = "Higher_Avg_Imperial";
                avgSIRaceSpeed = "Average_Speed_Imperial";
            }

            // Create the query
            string query = $"SELECT carName, carClass, Fastest_Lap, Average_Lap, {avgSIName}, {avgSIRaceSpeed}, Total_Time " +
                           $"FROM {dbTable} WHERE trackId=@track AND conditions=@conditions AND orientation=@orientation ";

            if (!string.IsNullOrEmpty(combo_Class.Text))
            {
                query += $"AND carClass=@carClass ";
            }
            if (!string.IsNullOrEmpty(combo_Vehicle.Text))
            {
                query += $"AND carName=@carName ";
            }

            query += "ORDER BY Fastest_Lap;";

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

        private void Combo_Track_Loaded(object sender, RoutedEventArgs e)
        {
            combo_Track.SelectionChanged += Combo_Track_SelectionChanged;
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

        void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void VehicleManager_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var VehicleWindow = new Vehicle();
            VehicleWindow.Show();
        }

        private void Settings_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Prompt the user before closing the main window
            Popup = Popup + 1;
            if (Popup == 1) { 
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
            }
            else
            {
                var settingsWindow = new SettingsWindow();

                settingsWindow.Left = this.Left + this.Width + 10;
                settingsWindow.Top = this.Top;
                settingsWindow.ShowDialog();
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

        private void PowerLaps_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var powerlap = new powerlap();
            powerlap.Show();
        }

        private void Objectives_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var objectives = new objectives();
            objectives.Show();
        }

        private void Houses_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var houses = new houses();
            houses.Show();
        }

        private void Dealership_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var dealerships = new dealerships();
            dealerships.Show();
        }

        private void tracks_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var tracks = new tracks();

            // Get the screen dimensions
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;

            // Set the position to the stored values, or default to the main window position if not set
            double targetLeft = lastTracksLeft != 0 ? lastTracksLeft : this.Left + this.Width + 10;
            double targetTop = lastTracksTop != 0 ? lastTracksTop : this.Top + (this.Height - tracks.Height) / 2;

            // Adjust if the window would be positioned outside the screen bounds
            if (targetLeft + tracks.Width > screenWidth)
            {
                targetLeft = screenWidth - tracks.Width;
            }
            if (targetTop + tracks.Height > screenHeight)
            {
                targetTop = screenHeight - tracks.Height;
            }
            if (targetLeft < 0)
            {
                targetLeft = 0;
            }
            if (targetTop < 0)
            {
                targetTop = 0;
            }

            // Set the window position
            tracks.Left = targetLeft;
            tracks.Top = targetTop;

            tracks.Closed += tracks_Closed;
            tracks.Show();

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

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void SGM_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var SaveGameManager = new SaveGameManager();
            SaveGameManager.Show();
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
