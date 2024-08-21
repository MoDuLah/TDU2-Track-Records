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
using System.Windows.Input;
using System.Windows.Media;
using TDU2_Track_Records.Properties;


namespace TDU2_Track_Records
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public int RecordsOn = 0;
        readonly string connectionString = Settings.Default.connectionString;
        public string distance = Settings.Default.distance;
        public string speed = Settings.Default.speed;
        readonly string SI = Settings.Default.system;
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
            Fill(combo_Track);
            BindComboBox(combo_Vehicle);
            calc_Total_Odometer();
        }
        public void UpdateMeasurementSystem()
        {
            // Update UI elements directly
            sysMSG.Text = "Measurement system updated!";
        }
        private void calc_Total_Lap_Time()
        {
            const int lapCount = 5;  // Number of laps, can be adjusted if needed
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
            // Convert race length from km to meters
            double totalDistance = Convert.ToDouble(race_Length.Text) * 1000;

            int totalLapTime = 0;
            int lapCount = 5;  // Adjust this if you have a different number of laps

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
            string speed = " km/h";  // Define the speed unit
            Average_Speed.Text = averageSpeed.ToString() + speed;
        }
        private void calc_Lap_Length()
        {
            if (string.IsNullOrEmpty(combo_Track.Text)) { return; }
            int trackId = combo_Track.SelectedIndex +1;
            SQLiteConnection dbConn; // Declare the SQLiteConnection-Object
            dbConn = new SQLiteConnection(connectionString);
            dbConn.Open();
            try
            {
                string query = $"SELECT Length From [tracks] where id ='{trackId}' ; ";
                if (dbConn.State == ConnectionState.Closed)
                {
                    dbConn.Open();
                }
                dbCmd = new SQLiteCommand(query, dbConn);
                reader = dbCmd.ExecuteReader();
                while (reader.Read())
                {
                    double value = Convert.ToDouble(reader[0].ToString());
                    if (SI == "Imperial")
                    {
                        value = Math.Round(value * onemile,2);
                    }
                    lap_Length.Text = value.ToString();
                    lap_distance_unit.Visibility = Visibility.Visible;
                    race_Length.Text = Convert.ToString(value * 5);
                    race_distance_unit.Visibility = Visibility.Visible;
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
            string odo = SI == "Metric" ? "Odometer_Metric" : "Odometer_Imperial";
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
            lap_Length.Text = "";
            lap_distance_unit.Visibility = Visibility.Collapsed;
            race_Length.Text = "";
            race_distance_unit.Visibility = Visibility.Collapsed;
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

            // Set window height
            Application.Current.MainWindow.Height = 500;
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

            int trackId = combo_Track.SelectedIndex + 1;

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
                odo = "Odometer_Metric";
                otherSiName = "Odometer_Imperial";
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
                odo = "Odometer_Imperial";
                otherSiName = "Odometer_Metric";
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
        SET {odo} = '{Convert.ToDouble(txt_odometer.Text)}', Races_Ran = Races_Ran + 1 
        WHERE Name = '{combo_Vehicle.Text.Replace("'", "''")}';

        UPDATE [vehicles] 
        SET {otherSiName} = '{calcOtherSIOdometer}' 
        WHERE Name = '{combo_Vehicle.Text.Replace("'", "''")}'";

        }

        private void ResetLapFields()
        {
            TextBox[] mins = { Lap1_Min, Lap2_Min, Lap3_Min, Lap4_Min, Lap5_Min };
            TextBox[] secs = { Lap1_Sec, Lap2_Sec, Lap3_Sec, Lap4_Sec, Lap5_Sec };
            TextBox[] ms = { Lap1_Ms, Lap2_Ms, Lap3_Ms, Lap4_Ms, Lap5_Ms };
            TextBlock[] speedLaps = { avg_SpeedLap1, avg_SpeedLap2, avg_SpeedLap3, avg_SpeedLap4, avg_SpeedLap5 };

            for (int i = 0; i < mins.Length; i++)
            {
                mins[i].Text = "00";
                secs[i].Text = "00";
                ms[i].Text = "000";
                speedLaps[i].Text = "0.0";
            }
        }

        private void ResetAllFields()
        {
            combo_Track.SelectedIndex = -1;
            combo_Class.SelectedIndex = -1;
            lap_Length.Text = string.Empty;
            lap_distance_unit.Visibility = Visibility.Collapsed;
            race_Length.Text = string.Empty;
            race_distance_unit.Visibility = Visibility.Collapsed;
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
                    using (var dbCmd = new SQLiteCommand($"SELECT {odo} FROM [vehicles] WHERE Name = '{veh}'", dbConn))
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


        private void Combo_Class_LostFocus(object sender, RoutedEventArgs e)
        {
            HandleComboClassChange();
        }

        private void Combo_Class_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            HandleComboClassChange();
            AdjustWindowHeight();
        }

        private void HandleComboClassChange()
        {
            BindComboBox(combo_Vehicle);
            CheckProgress();
            loadLapRecord();

            bool isClassSelected = combo_Class.SelectedIndex >= 1;
            UpdateUIForSI(isClassSelected);
        }

        private void UpdateUIForSI(bool isClassSelected)
        {
            if (SI == "Metric")
            {
                btn_loadrec_Metric.Content = isClassSelected ? "Load Class Records" : "Load Track Records";
                Lap_Record_Header.Header = isClassSelected ? "Class Lap Record" : "Track Lap Record";
                ViewEntries_Metric.Visibility = Visibility.Collapsed;
                btn_loadrec_Metric.IsChecked = false;
            }
            else
            {
                btn_loadrec_Imperial.Content = isClassSelected ? "Load Class Records" : "Load Track Records";
                Lap_Record_Header.Header = isClassSelected ? "Class Lap Record" : "Track Lap Record";
                ViewEntries_Imperial.Visibility = Visibility.Collapsed;
                btn_loadrec_Imperial.IsChecked = false;
            }
        }

        private void AdjustWindowHeight()
        {
            Application.Current.MainWindow.Height -= 300;
        }

        private void loadLapRecord()
        {

            orientationA_lrec.Visibility = Visibility.Collapsed;
            orientationC_lrec.Visibility = Visibility.Collapsed;
            weatherS_lrec.Visibility = Visibility.Collapsed;
            weatherR_lrec.Visibility = Visibility.Collapsed;
            if (string.IsNullOrEmpty(combo_Track.Text)) { return; }
            int conditions = cb_conditions.IsChecked == true ? 1 : 0;
            int track = combo_Track.SelectedIndex + 1;
            string spec = combo_Class.Text;
            SQLiteConnection dbConn; // Declare the SQLiteConnection-Object
            dbConn = new SQLiteConnection(connectionString);
            dbConn.Open();
            try
            {
                string query = "";
                if (!string.IsNullOrEmpty(combo_Track.Text) && string.IsNullOrEmpty(combo_Class.Text))
                {
                    query = $"SELECT carName, Min(Fastest_Lap),conditions,orientation From [records] where trackID ='{track}'; ";
                }
                else { 
                if (!string.IsNullOrEmpty(combo_Track.Text) && !string.IsNullOrEmpty(combo_Class.Text))
                {
                    query = $"SELECT carName, Min(Fastest_Lap),conditions,orientation From [records] where carClass ='{spec}' and trackID ='{track}'; ";
                }
                }
                if (dbConn.State == ConnectionState.Closed)
                {
                    dbConn.Open();
                }
                dbCmd = new SQLiteCommand(query, dbConn);
                reader = dbCmd.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {
                        if (reader.IsDBNull(0))
                        {
                            txt_lrec.Text = "No Record";
                            txt_lrecCar.Text = "No Record";
                            reader.Close();
                            if (dbConn.State == ConnectionState.Open)
                            {
                                dbConn.Close();
                            }
                            return;
                        }
                        string carName = reader[0].ToString();
                        string value = reader[1].ToString();
                        int weather = Convert.ToInt32(reader[2].ToString());
                        int orient = Convert.ToInt32(reader[3].ToString());
                        if (string.IsNullOrEmpty(carName))
                        {
                            carName = "No Record";
                        }
                        if (string.IsNullOrEmpty(value))
                        {
                            value = "No Record";
                        }
                        txt_lrec.Text = value;
                        txt_lrecCar.Text = carName;

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

                        string recordX = "";
                        string track3r = combo_Track.Text.Replace("'", "''");
                        switch (combo_Class.SelectedIndex)
                        {
                            case -1:
                                recordX = "Track_Lap_Record";
                                break;
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
                        }
                        if (!string.IsNullOrEmpty(carName) & !string.IsNullOrEmpty(value))
                        {
                            string fuery = $"UPDATE [tracks] SET {recordX} = '" + carName + " - " + txt_lrec.Text + "' WHERE Name = '" + track3r + "'";
                            if (dbConn.State == ConnectionState.Closed)
                            {
                                dbConn.Open();
                            }
                            dbCmd = new SQLiteCommand(fuery, dbConn);
                            int resultAffectedRows = dbCmd.ExecuteNonQuery();
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

        private void Fill(ComboBox comboBox)
        {
            const string query = "SELECT * FROM tracks";

            try
            {
                using (var dbConn = new SQLiteConnection(connectionString))
                {
                    dbConn.Open();
                    using (var dbCmd = new SQLiteCommand(query, dbConn))
                    using (var dbAdapter = new SQLiteDataAdapter(dbCmd))
                    {
                        var dataSet = new DataSet();
                        dbAdapter.Fill(dataSet, "tracks");

                        SetComboBoxSource(comboBox, dataSet);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading tracks:\n{ex.Message}");
            }
        }

        private void SetComboBoxSource(ComboBox comboBox, DataSet dataSet)
        {
            if (dataSet.Tables.Count > 0)
            {
                comboBox.ItemsSource = dataSet.Tables[0].DefaultView;
                comboBox.DisplayMemberPath = "Name";  // Display the 'Name' column
                comboBox.SelectedValuePath = "id";    // Use the 'id' column as the value
            }
            else
            {
                MessageBox.Show("No tracks found in the database.");
            }
        }



        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            FextBox_GotFocus((TextBox)sender);
        }

        private void FextBox_GotFocus(TextBox textBox)
        {
            // Auto-tab when maxlength is reached
            if (textBox.Text == "00")
            {
                textBox.Text = "";
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox_LostFocus((TextBox)sender);
        }

        private void TextBox_LostFocus(TextBox textBox)
        {
            if (string.IsNullOrEmpty(textBox.Text))
            {
                textBox.Text = "00";
            }

            if (textBox.Name.EndsWith("_Sec"))
            {
                if (int.TryParse(textBox.Text, out int seconds) && seconds > 59)
                {
                    textBox.Text = "59";
                }
            }

            if (textBox.Text.Length == 1)
            {
                textBox.Text = "0" + textBox.Text;
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


        private void BindComboBox(ComboBox comboBox)
        {
            txt_odometer.Text = "";
            txt_carPB.Text = "";
            txt_rRan.Text = "";

            using (var dbConn = new SQLiteConnection(connectionString))
            {
                dbConn.Open();
                using (var dbCmd = new SQLiteCommand())
                {
                    dbCmd.Connection = dbConn;
                    dbCmd.CommandType = CommandType.Text;
                    dbCmd.CommandText = "SELECT * FROM vehicles";

                    if (!string.IsNullOrEmpty(combo_Class.Text))
                    {
                        dbCmd.CommandText += " WHERE Class = @Class AND Active = '1' ORDER BY Name ASC;";
                        dbCmd.Parameters.AddWithValue("@Class", combo_Class.Text);
                    }
                    else
                    {
                        dbCmd.CommandText += " where Active = '1' ORDER BY Name ASC;";
                    }

                    using (var dbAdapter = new SQLiteDataAdapter(dbCmd))
                    {
                        var ds = new DataSet();
                        try
                        {
                            dbAdapter.Fill(ds, "vehicles");
                            comboBox.ItemsSource = ds.Tables[0].DefaultView;
                            comboBox.DisplayMemberPath = "Name";
                            comboBox.SelectedValuePath = "id";
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("An error occurred while loading categories.\n" + ex.ToString());
                        }
                    }
                }
            }
        }


        private void Btn_loadrec_Click(object sender,
                                       RoutedEventArgs e)
        {
            int adjustment = 300;
            //if(combo_Track.SelectedIndex == -1) { return; }
            if (btn_loadrec_Metric.IsChecked == true || btn_loadrec_Imperial.IsChecked == true) {
            Application.Current.MainWindow.Height = Application.Current.MainWindow.Height + adjustment;
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
  
                Application.Current.MainWindow.Height = Application.Current.MainWindow.Height - adjustment;
  
            }
        }
 

        private void Combo_Vehicle_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(combo_Vehicle.Text)) {
                return;
            }
            else
            {
                btn_loadrec_Metric.Content = "Load Vehicle Records";
            }

            string odo = SI == "Metric" ? "Odometer_Metric" : "Odometer_Imperial";
            int conditions = cb_conditions.IsChecked == true ? 1 : 0;
            int orientation = cb_orientation.IsChecked == false ? 0 : 1;
            string veh = combo_Vehicle.Text.Replace("'", "''");
            int trackId = combo_Track.SelectedIndex + 1;

            txt_carPB.Text = "";
            txt_odometer.Text = "";
            txt_rRan.Text = "";

            string query = $@"
        SELECT 
            (SELECT {odo} AS Odometer FROM vehicles WHERE Name = '{veh}') ,
            (SELECT Fastest_Lap AS FastestLap FROM records WHERE carName = '{veh}' AND trackId = '{trackId}' AND conditions = '{conditions}') ,
            (SELECT COUNT(*) AS RecordCount FROM records WHERE trackId = '{trackId}' AND carName = '{veh}' AND conditions = '{conditions}') ";

            try
            {
                using (var dbConn = new SQLiteConnection(connectionString))
                {
                    dbConn.Open();
                    using (var dbCmd = new SQLiteCommand(query, dbConn))
                    using (var reader = dbCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            txt_odometer.Text = reader.IsDBNull(0) ? "0" : reader.GetDouble(0).ToString();
                            txt_carPB.Text = reader.IsDBNull(1) ? "No Record" : reader.GetString(1);
                            txt_rRan.Text = reader.GetInt32(2).ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.ToString());
            }
        }



        private void Btn_submit_GotFocus(object sender, RoutedEventArgs e)
        {
            calc_Average_Lap_Time();
            calc_Total_Lap_Time();
            calc_Average_Speed();
        }

        private void Combo_Track_LostFocus(object sender, RoutedEventArgs e)
        {

            if (SI == "Metric")
            {
                if (combo_Track.SelectedIndex == -1)
                {
                    return;
                }
                else
                {
                    btn_loadrec_Metric.IsEnabled = true;
                    btn_loadrec_Imperial.IsEnabled = false;
                    btn_loadrec_Metric.IsChecked = false;
                    if (combo_Class.SelectedIndex < 0)
                    {
                        btn_loadrec_Metric.Content = "Load Track Records";
                    }
                }
                ViewEntries_Metric.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (combo_Track.SelectedIndex == -1)
                {
                    return;
                }
                else
                {
                    btn_loadrec_Imperial.IsChecked = false;
                    btn_loadrec_Imperial.IsEnabled = true;
                    btn_loadrec_Metric.IsEnabled = false;
                    btn_loadrec_Imperial.Content = "Load Track Records";
                }
                ViewEntries_Imperial.Visibility = Visibility.Collapsed;
            }
            Application.Current.MainWindow.Height = 500;
            calc_Lap_Length();
            CheckProgress();
            loadLapRecord();

            if (string.IsNullOrEmpty(combo_Vehicle.Text)) return;

            int conditions = cb_conditions.IsChecked == true ? 1 : 0;
            int orientation = cb_orientation.IsChecked == false ? 0 : 1;
            string veh = combo_Vehicle.Text.Replace("'", "''");
            int trackId = combo_Track.SelectedIndex + 1;

            string odoColumn = SI == "Metric" ? "Odometer_Metric" : "Odometer_Imperial";
            double onemile = 1.60934; // Adjust this value as necessary for conversion if not already defined

            string query = $@"
            SELECT 
                (SELECT {odoColumn} AS Odometer FROM vehicles WHERE Name = '{veh}') ,
                (SELECT Fastest_Lap AS FastestLap FROM records WHERE carName = '{veh}' AND trackId = {trackId} AND conditions = '{conditions}' AND orientation = '{orientation}'),
                (SELECT COUNT(*) AS RecordCount FROM records WHERE trackId = {trackId} AND carName = '{veh}' AND conditions = '{conditions}' AND orientation = '{orientation}')";

            try
            {
                using (var dbConn = new SQLiteConnection(connectionString))
                {
                    dbConn.Open();
                    using (var dbCmd = new SQLiteCommand(query, dbConn))
                    using (var reader = dbCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if (!reader.IsDBNull(0))
                            {
                                double value = reader.GetDouble(0);
                                if (SI == "Imperial")
                                {
                                    value = Math.Round(value * onemile, 2);
                                }
                                txt_odometer.Text = value.ToString();
                            }

                            txt_carPB.Text = reader.IsDBNull(1) ? "No Record" : reader.GetString(1);
                            txt_rRan.Text = reader.GetInt32(2).ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.ToString());
            }
        }

        private void Combo_Track_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (SI == "Metric")
            {
                btn_loadrec_Metric.IsEnabled = true;
                if (btn_loadrec_Metric.IsChecked == true)
                {
                    btn_loadrec_Metric.IsChecked = false;
                }
                ViewEntries_Metric.Visibility = Visibility.Collapsed;
                Application.Current.MainWindow.Height = 500;
            }
            else
            {
                btn_loadrec_Imperial.IsEnabled = true;
                if (btn_loadrec_Imperial.IsChecked == true)
                {
                    btn_loadrec_Imperial.IsChecked = false;
                }
                ViewEntries_Imperial.Visibility = Visibility.Collapsed;
                Application.Current.MainWindow.Height = 500;
            }
            CheckProgress();
            loadLapRecord();
            calc_Lap_Length();

            if (string.IsNullOrEmpty(combo_Vehicle.Text)) return;

            txt_carPB.Text = "";
            int conditions = cb_conditions.IsChecked == true ? 1 : 0;
            int orientation = cb_orientation.IsChecked == false ? 0 : 1;
            string veh = combo_Vehicle.Text.Replace("'", "''");
            int trackId = combo_Track.SelectedIndex + 1;

            string odoColumn = SI == "Metric" ? "Odometer_Metric" : "Odometer_Imperial";
            double onemile = 1.60934; // Adjust this value as necessary for conversion if not already defined

            string query = $@"
            SELECT 
            (SELECT {odoColumn} AS Odometer FROM vehicles WHERE Name = '{veh}'),
            (SELECT Fastest_Lap AS FastestLap FROM records WHERE carName = '{veh}' AND trackId = {trackId} AND conditions = {conditions} AND orientation = '{orientation}'),
            (SELECT COUNT(*) AS RecordCount FROM records WHERE trackId = {trackId} AND carName = '{veh}' AND conditions = {conditions} AND orientation = '{orientation}')";

            try
            {
                using (var dbConn = new SQLiteConnection(connectionString))
                {
                    dbConn.Open();
                    using (var dbCmd = new SQLiteCommand(query, dbConn))
                    using (var reader = dbCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if (!reader.IsDBNull(0))
                            {
                                double value = reader.GetDouble(0);
                                if (SI == "Imperial")
                                {
                                    value = Math.Round(value * onemile, 2);
                                }
                                txt_odometer.Text = value.ToString();
                            }

                            txt_carPB.Text = reader.IsDBNull(1) ? "No Record" : reader.GetString(1);
                            txt_rRan.Text = reader.GetInt32(2).ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.ToString());
            }
        }
        private void Combo_Track_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Early exits for various conditions
            if (keep.IsChecked == false) combo_Class.SelectedIndex = -1;
            if (!combo_Track.IsLoaded || combo_Track.Items.Count == 0 || combo_Track.SelectedIndex < 0) return;
            if (e.AddedItems.Count == 0) return;  // Ensure the selection actually changed

            UpdateUIForSI();
            ResetWindowState();
            calc_Lap_Length();
            CheckProgress();
            loadLapRecord();

            if (string.IsNullOrEmpty(combo_Vehicle.Text)) return;

            FetchAndDisplayVehicleData();
        }

        private void UpdateUIForSI()
        {
            bool isMetric = SI == "Metric";

            btn_loadrec_Metric.IsEnabled = isMetric;
            btn_loadrec_Imperial.IsEnabled = !isMetric;

            btn_loadrec_Metric.IsChecked = false;
            btn_loadrec_Imperial.IsChecked = false;

            if (combo_Class.SelectedIndex <= 0)
            {
                if (isMetric)
                {
                    btn_loadrec_Metric.Content = "Load Track Records";
                }
                else
                {
                    btn_loadrec_Imperial.Content = "Load Track Records";
                }
            }

            ViewEntries_Metric.Visibility = isMetric ? Visibility.Collapsed : ViewEntries_Metric.Visibility;
            ViewEntries_Imperial.Visibility = !isMetric ? Visibility.Collapsed : ViewEntries_Imperial.Visibility;
        }

        private void ResetWindowState()
        {
            Application.Current.MainWindow.Height = 500;
        }

        private void FetchAndDisplayVehicleData()
        {
            int conditions = cb_conditions.IsChecked == true ? 1 : 0;
            int orientation = cb_orientation.IsChecked == false ? 0 : 1;
            string veh = combo_Vehicle.Text.Replace("'", "''");
            int trackId = combo_Track.SelectedIndex + 1;

            string odoColumn = SI == "Metric" ? "Odometer_Metric" : "Odometer_Imperial";
            double conversionFactor = 1.60934; // Conversion factor for miles to kilometers

            string query = $@"
                             SELECT 
                             (SELECT {odoColumn} AS Odometer FROM vehicles WHERE Name = '{veh}') ,
                             (SELECT Fastest_Lap AS FastestLap FROM records WHERE carName = '{veh}' AND trackId = {trackId}) ,
                             (SELECT COUNT(*) AS RecordCount FROM records WHERE trackId = {trackId} AND carName = '{veh}' AND conditions = {conditions} AND orientation = {orientation})";

            try
            {
                using (var dbConn = new SQLiteConnection(connectionString))
                {
                    dbConn.Open();
                    using (var dbCmd = new SQLiteCommand(query, dbConn))
                    using (var reader = dbCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if (!reader.IsDBNull(0))
                            {
                                double value = reader.GetDouble(0);
                                if (SI == "Imperial")
                                {
                                    value = Math.Round(value * conversionFactor, 2);
                                }
                                txt_odometer.Text = value.ToString();
                            }

                            txt_carPB.Text = reader.IsDBNull(1) ? "No Record" : reader.GetString(1);
                            txt_rRan.Text = reader.GetInt32(2).ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.ToString());
            }
        }


        private void CheckProgress()
        {
            txt_Progress_1.Text = "";
            txt_Progress_2.Text = "";
            if (string.IsNullOrEmpty(combo_Track.Text)) { return; }

            int conditions = cb_conditions.IsChecked == true ? 1 : 0;
            int orientation = cb_orientation.IsChecked == true ? 1 : 0;
            int trackId = combo_Track.SelectedIndex + 1;
            string param = combo_Class.Text;

            try
            {
                using (var dbConn = new SQLiteConnection(connectionString))
                {
                    dbConn.Open();

                    int recordsCount = GetRecordCount(dbConn, trackId, conditions, orientation,param);
                    txt_Progress_1.Text = recordsCount.ToString();

                    int carsCount = GetCarsCount(dbConn, param);
                    double percentage = (recordsCount / (double)carsCount) * 100;

                    txt_Progress_1.Text = $"{recordsCount} / {carsCount}";
                    txt_Progress_2.Text = $"{Math.Round(percentage, 2)}%";
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
                ? $"SELECT COUNT(*) FROM [records] WHERE trackId = '{trackId}' AND conditions = '{conditions}' and orientation = '{orientation}';"
                : $"SELECT COUNT(*) FROM [records] WHERE trackId = '{trackId}' AND carClass = '{param}' AND conditions = '{conditions}';";

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
                ? "SELECT COUNT(*) FROM [vehicles];"
                : $"SELECT COUNT(*) FROM [vehicles] WHERE Class = '{param}';";

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

        // Preview text input to allow only specific characters (e.g., numerical input)
        private new void PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        // Preview text input to disallow decimal points (e.g., for integer input)
        private void PreviewNoDecInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsDecAllowed(e.Text);
        }

        public int RemoveIndex { get; private set; }

        // Define regex patterns for allowed/disallowed text
        private static readonly Regex _regexNoDec = new Regex("[^0-9]+"); // Allows only numeric characters (no decimals)

        // Check if the input text is allowed (e.g., only numerical characters)
        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

        // Check if the input text is allowed without decimal points
        private static bool IsDecAllowed(string text)
        {
            return !_regexNoDec.IsMatch(text);
        }


        private void HandleLapMsPreviewLostKeyboardFocus(TextBox minTextBox, TextBox secTextBox, TextBox msTextBox, TextBlock avgSpeedTextBlock)
        {
            if (combo_Track.SelectedIndex == -1) { return; }
            if (string.IsNullOrEmpty(minTextBox.Text)) { return; }
            if (string.IsNullOrEmpty(secTextBox.Text)) { return; }
            if (string.IsNullOrEmpty(msTextBox.Text)) { return; }

            //int removeIndex = lap_Length.Text.Length - 2;
            double lapLength = Convert.ToDouble(lap_Length.Text) * 1000; //.Text.Remove(removeIndex)
            lap_distance_unit.Visibility = Visibility.Visible;
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

            if (totalMs > 0)
            {
                double averageSpeed = Math.Round(((lapLength * 3600000) / totalMs) / 1000, 2);
                avgSpeedTextBlock.Text = averageSpeed.ToString();
            }
        }
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
            if (combo_Track.SelectedIndex == -1) { return; }

            int trackId = combo_Track.SelectedIndex + 1;
            using (var dbConn = new SQLiteConnection(connectionString))
            {
                dbConn.Open();
                try
                {
                    string query = $"SELECT * FROM [tracks] WHERE id = '{trackId}'";
                    using (var dbCmd = new SQLiteCommand(query, dbConn))
                    using (var reader = dbCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var labels = new[]
                            {
                        "Record A1", "Record A2", "Record A3", "Record A4", "Record A5", "Record A6", "Record A7",
                        "Record B2", "Record B3", "Record B4", "Record C1", "Record C2", "Record C3", "Record C4",
                        "Record MA1", "Record MA2"
                    };

                            var values = new List<string>();
                            for (int i = 4; i < reader.FieldCount && i < 20; i++)
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
                            sb.AppendLine($"Track Name: {reader[1]}")
                              .AppendLine($"Total Runs: {reader[3]}");

                            for (int i = 0; i < labels.Length; i++)
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

            int track = combo_Track.SelectedIndex + 1;
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
                        dbCmd.Parameters.AddWithValue("@track", track);
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
            if(cb_orientation.IsChecked == false)
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
            if(cb_conditions.IsChecked == false)
            { 
                cb_conditions.IsChecked = true; 
            } else {
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
            else {
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

        private void Image_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

                var powerlap = new powerlap();
                powerlap.Show();
        }

        void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
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
