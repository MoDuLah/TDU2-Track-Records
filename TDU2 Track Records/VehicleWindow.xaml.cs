using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using TDU2_Track_Records.Properties;

namespace TDU2_Track_Records
{
    /// <summary>
    /// Interaction logic for Vehicle.xaml
    /// </summary>
    public partial class Vehicle : Window
    {
        private List<VehicleManagement> _vehicles = new List<VehicleManagement>();
        readonly string connectionString = Settings.Default.connectionString;
        public string distance, speed;
        string SI = Settings.Default.system;

        public Vehicle()
        {
            InitializeComponent();
            BindComboBox(VehicleBrandComboBox);
            BindComboBox(VehicleSelection);
            LoadVehiclesDataGrid();
            LoadVehicles();
        }

        private void BindComboBox(ComboBox comboBox)
        {
            string query = GetQueryForComboBox(comboBox);

            if (string.IsNullOrEmpty(query))
            {
                MessageBox.Show("Invalid ComboBox provided.");
                return;
            }

            try
            {
                using (var dbConn = new SQLiteConnection(connectionString))
                using (var dbCmd = new SQLiteCommand(query, dbConn))
                using (var dbAdapter = new SQLiteDataAdapter(dbCmd))
                {
                    dbConn.Open();

                    var ds = new DataSet();
                    dbAdapter.Fill(ds, "vehicles");

                    comboBox.ItemsSource = ds.Tables[0].DefaultView;
                    comboBox.DisplayMemberPath = GetDisplayMemberPath(comboBox);
                    comboBox.SelectedValuePath = "id";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading data.\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private string GetQueryForComboBox(ComboBox comboBox)
        {
            if (comboBox == VehicleBrandComboBox)
            {
                return @"
            SELECT MIN(id) AS id, Brand
            FROM vehicles 
            GROUP BY Brand 
            ORDER BY Brand ASC;";
            }
            else if (comboBox == VehicleSelection)
            {
                return "SELECT * FROM vehicles;";
            }
            else
            {
                return null;
            }
        }

        private string GetDisplayMemberPath(ComboBox comboBox)
        {
            return comboBox == VehicleBrandComboBox ? "Brand" : "Name";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string height = this.Height.ToString();
            string width = this.Width.ToString();
            MessageBox.Show($"Width: {width} \n Height: {height}");
        }
        private double ConvertSpeedToSelectedUnit(double speed, string unit)
        {
            return unit == "mph" ? speed * 0.621371 : speed;  // Convert to mph if selected
        }

        private double ConvertWeightToSelectedUnit(double weight, string unit)
        {
            return unit == "lbs" ? weight * 2.20462 : weight;  // Convert to lbs if selected
        }

        private void LoadVehicles()
        {
            try
            {
                using (var conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM vehicles";
                    using (var cmd = new SQLiteCommand(query, conn))
                    {
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            _vehicles.Clear(); // Clear the list before reloading data.

                            while (reader.Read())
                            {
                                var vehicle = new VehicleManagement
                                {
                                    Id = reader.IsDBNull(0) ? default : reader.GetInt32(0),
                                    Name = reader.IsDBNull(1) ? null : reader.GetString(1),
                                    Brand = reader.IsDBNull(2) ? null : reader.GetString(2),
                                    Model = reader.IsDBNull(3) ? null : reader.GetString(3),
                                    Class = reader.IsDBNull(4) ? null : reader.GetString(4),
                                    RacesRan = reader.IsDBNull(5) ? default : reader.GetInt32(5),
                                    OdometerMetric = reader.IsDBNull(6) ? default : reader.GetDouble(6),
                                    OdometerImperial = reader.IsDBNull(7) ? default : reader.GetDouble(7),
                                    Active = reader.IsDBNull(8) ? default : reader.GetInt32(8),
                                    Owned = reader.IsDBNull(9) ? default : reader.GetInt32(9),
                                    Price = reader.IsDBNull(10) ? default : reader.GetInt32(10),
                                    AccelerationStat = reader.IsDBNull(11) ? default : reader.GetInt32(11),
                                    SpeedStat = reader.IsDBNull(12) ? default : reader.GetInt32(12),
                                    BrakingStat = reader.IsDBNull(13) ? default : reader.GetInt32(13),
                                    DifficultyStat = reader.IsDBNull(14) ? default : reader.GetInt32(14),
                                    TopSpeed = reader.IsDBNull(15) ? default : reader.GetInt32(15),
                                    Acceleration = reader.IsDBNull(16) ? default : reader.GetDouble(16),
                                    Engine = reader.IsDBNull(17) ? default : reader.GetString(17),
                                    EngineLayout = reader.IsDBNull(18) ? null : reader.GetString(18),
                                    Gearbox = reader.IsDBNull(19) ? null : reader.GetString(19),
                                    MaxTorque = reader.IsDBNull(20) ? default : reader.GetInt32(20),
                                    MaxTorqueRPM = reader.IsDBNull(21) ? default : reader.GetInt32(21),
                                    MaxPower = reader.IsDBNull(22) ? default : reader.GetInt32(22),
                                    MaxPowerRPM = reader.IsDBNull(23) ? default : reader.GetInt32(23),
                                    Weight = reader.IsDBNull(24) ? default : reader.GetInt32(24),
                                    Image = reader.IsDBNull(25) ? null : (byte[])reader["Image"] // Load image data
                                };

                                _vehicles.Add(vehicle);
                            }

                            // Bind the vehicles list to the ComboBox
                            VehicleSelection.ItemsSource = _vehicles;
                            VehicleSelection.DisplayMemberPath = "Name"; // Or another property to display
                            VehicleSelection.SelectedValuePath = "Id"; // Optional, use if you need to bind the Id
                        }
                    }
                }
            }
            catch (InvalidCastException ex)
            {
                MessageBox.Show($"Invalid cast: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }
        private void VehicleSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (VehicleSelection.SelectedItem == null) return;

            var selectedVehicle = VehicleSelection.SelectedItem as VehicleManagement;
            if (selectedVehicle == null) return;

            // Load and display the image if available
            if (selectedVehicle.Image != null && selectedVehicle.Image.Length > 0)
            {
                using (var ms = new MemoryStream(selectedVehicle.Image))
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = ms;
                    bitmapImage.EndInit();
                    UploadedImage.Source = bitmapImage;
                }
            }
            else
            {
                // Set a default image or clear the image control if no image is available
                UploadedImage.Source = null;
            }

            PopulateVehicleDetails(selectedVehicle);
        }

        private void PopulateVehicleDetails(VehicleManagement selectedVehicle)
        {
            Tabctrler.SelectedIndex = 0; // Optionally change the tab if needed.

            if (VehicleBrandComboBox.Visibility == Visibility.Visible)
            {
                VehicleBrandComboBox.Text = selectedVehicle.Brand;
            }
            else
            {
                VehicleBrandTextBox.Text = selectedVehicle.Brand;
            }

            VehicleModelTextBox.Text = selectedVehicle.Model;
            VehicleClassComboBox.Text = selectedVehicle.Class;

            if (SI == "Metric")
            {
                Odometer_Metric.Text = selectedVehicle.OdometerMetric.ToString();
                Odometer_Metric.Visibility = Visibility.Visible;
                Odometer_Imperial.Visibility = Visibility.Collapsed;
            }
            else
            {
                Odometer_Imperial.Text = selectedVehicle.OdometerImperial.ToString();
                Odometer_Metric.Visibility = Visibility.Collapsed;
                Odometer_Imperial.Visibility = Visibility.Visible;
            }

            VehiclePriceTextBox.Text = selectedVehicle.Price.ToString();
            VehicleActiveCheckBox.IsChecked = Convert.ToBoolean(selectedVehicle.Active);
            VehicleOwnedCheckBox.IsChecked = Convert.ToBoolean(selectedVehicle.Owned);
            AccelerationSlider.Value = selectedVehicle.AccelerationStat;
            SpeedRatingSlider.Value = selectedVehicle.SpeedStat;
            BrakingRatingSlider.Value = selectedVehicle.BrakingStat;
            DifficultyRatingSlider.Value = selectedVehicle.DifficultyStat;
            TopSpeedTextBox.Text = selectedVehicle.TopSpeed.ToString();
            AccelerationTextBox.Text = selectedVehicle.Acceleration.ToString();
            EngineSizeTextBox.Text = selectedVehicle.Engine;
            EngineLayoutComboBox.Text = selectedVehicle.EngineLayout;
            GearboxComboBox.Text = selectedVehicle.Gearbox;
            MaxTorqueTextBox.Text = selectedVehicle.MaxTorque.ToString();
            MaxTorqueRPMTextBox.Text = selectedVehicle.MaxTorqueRPM.ToString();
            MaxPowerTextBox.Text = selectedVehicle.MaxPower.ToString();
            MaxPowerRPMTextBox.Text = selectedVehicle.MaxPowerRPM.ToString();
            WeightTextBox.Text = selectedVehicle.Weight.ToString();
        }

        private void PopulateVehicleFields(VehicleManagement vehicle)
        {
            VehicleBrandTextBox.Text = vehicle.Name;
            VehicleBrandComboBox.Text = vehicle.Name;
            VehicleClassComboBox.Text = vehicle.Class;
            if (SI == "Metric")
            {
                Odometer_Metric.Text = vehicle.OdometerMetric.ToString();
                Odometer_Imperial.Text = "";
                Odometer_Imperial.Visibility = Visibility.Collapsed;
            } else
            {
                Odometer_Imperial.Text = vehicle.OdometerImperial.ToString();
                Odometer_Metric.Text = "";
                Odometer_Metric.Visibility = Visibility.Collapsed;
            }
            VehiclePriceTextBox.Text = vehicle.Price.ToString();
            VehicleActiveCheckBox.IsChecked = Convert.ToBoolean(vehicle.Active);
            AccelerationSlider.Value = vehicle.AccelerationStat;
            SpeedRatingSlider.Value = vehicle.SpeedStat;
            BrakingRatingSlider.Value = vehicle.BrakingStat;
            DifficultyRatingSlider.Value = vehicle.DifficultyStat;

            // Correctly access the selected unit in ComboBox
            string selectedSpeedUnit = Settings.Default.speed as string;
            string selectedWeightUnit = Settings.Default.weight as string;

            TopSpeedTextBox.Text = ConvertSpeedToSelectedUnit(vehicle.TopSpeed, selectedSpeedUnit).ToString();
            AccelerationTextBox.Text = vehicle.AccelerationTime.ToString();
            EngineSizeTextBox.Text = vehicle.Engine.ToString();
            EngineLayoutComboBox.Text = vehicle.EngineLayout;
            GearboxComboBox.Text = vehicle.Gearbox;
            MaxTorqueTextBox.Text = vehicle.MaxTorque.ToString();
            MaxTorqueRPMTextBox.Text = vehicle.MaxTorqueRPM.ToString();
            MaxPowerTextBox.Text = vehicle.MaxPower.ToString();
            MaxPowerRPMTextBox.Text = vehicle.MaxPowerRPM.ToString();
            WeightTextBox.Text = ConvertWeightToSelectedUnit(vehicle.Weight, selectedWeightUnit).ToString();
        }

        private void ExecuteNonQuery(string query, Action<SQLiteCommand> setParameters)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    setParameters(cmd);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void SaveChangesButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateVehicleDetails();
            UpdateVehicleDetailsDataGrid();
        }
        private void UpdateVehicleDetailsDataGrid()
        {
            // Collect and validate input values
            if (!ValidateInputs(out double mileage, out int price, out int topSpeed,
                                out int accelerationRating, out int speedRating, out int brakingRating,
                                out int difficultyRating, out double accelerationTime, out int maxPower,
                                out int maxPowerRPM, out int maxTorque, out int maxTorqueRPM, out int weight))
            {
                MessageBox.Show("Please ensure all fields are correctly formatted.");
                return;
            }

            // Get selected vehicle
            if (EditVehicleDataGrid.SelectedItem is VehicleManagement selectedVehicle)
            {
                // Set values to the selected vehicle object
                SetVehicleDetails(selectedVehicle, mileage, price, topSpeed, accelerationRating,
                                  speedRating, brakingRating, difficultyRating, accelerationTime,
                                  maxPower, maxPowerRPM, maxTorque, maxTorqueRPM, weight);

                // Prepare and execute SQL query
                string query = @"
            UPDATE Vehicles SET 
            Name = @name,
            Brand = @brand,
            Model = @model,
            Class = @class,
            Odometer_Metric = @odometerMetric,
            Odometer_Imperial = @odometerImperial,
            Owned = @owned,
            Active = @active,
            Price = @price, 
            Acceleration_Stat = @accelerationRating,
            Speed_Stat = @speedRating,
            Braking_Stat = @brakingRating, 
            Difficulty_Stat = @difficultyRating,
            Top_Speed = @topSpeed,
            Acceleration_Time = @accelerationTime,
            Engine = @engine,
            Engine_Layout = @layout,
            Gearbox = @gearbox,
            Max_Power = @maxPower,
            Max_PowerRPM = @maxPowerRPM,
            Max_Torque = @maxTorque,
            Max_TorqueRPM = @maxTorqueRPM, 
            Weight = @weight 
            WHERE Id = @id";

                try
                {
                    ExecuteNonQuery(query, cmd =>
                    {
                        AddParameters(cmd, selectedVehicle);
                    });

                    MessageBox.Show("Vehicle details updated successfully.");
                    LoadVehiclesDataGrid();  // Refresh the list
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while saving changes: {ex.Message}");
                }
            }
        }
        private void UpdateVehicleDetails()
        {
            // Collect and validate input values
            if (!ValidateInputs(out double mileage, out int price, out int topSpeed,
                                out int accelerationRating, out int speedRating, out int brakingRating,
                                out int difficultyRating, out double accelerationTime, out int maxPower,
                                out int maxPowerRPM, out int maxTorque, out int maxTorqueRPM, out int weight))
            {
                MessageBox.Show("Please ensure all fields are correctly formatted.");
                return;
            }

            // Get selected vehicle
            if (VehicleSelection.SelectedItem is VehicleManagement selectedVehicle)
            {
                // Set values to the selected vehicle object
                SetVehicleDetails(selectedVehicle, mileage, price, topSpeed, accelerationRating,
                                  speedRating, brakingRating, difficultyRating, accelerationTime,
                                  maxPower, maxPowerRPM, maxTorque, maxTorqueRPM, weight);

                // Prepare and execute SQL query
                string query = @"
            UPDATE Vehicles SET 
            Name = @name,
            Brand = @brand,
            Model = @model,
            Class = @class,
            Odometer_Metric = @odometerMetric,
            Odometer_Imperial = @odometerImperial,
            Owned = @owned,
            Active = @active,
            Price = @price, 
            Acceleration_Stat = @accelerationRating,
            Speed_Stat = @speedRating,
            Braking_Stat = @brakingRating, 
            Difficulty_Stat = @difficultyRating,
            Top_Speed = @topSpeed,
            Acceleration_Time = @accelerationTime,
            Engine = @engine,
            Engine_Layout = @layout,
            Gearbox = @gearbox,
            Max_Power = @maxPower,
            Max_PowerRPM = @maxPowerRPM,
            Max_Torque = @maxTorque,
            Max_TorqueRPM = @maxTorqueRPM, 
            Weight = @weight 
            WHERE Id = @id";

                try
                {
                    ExecuteNonQuery(query, cmd =>
                    {
                        AddParameters(cmd, selectedVehicle);
                    });

                    MessageBox.Show("Vehicle details updated successfully.");
                    LoadVehicles();  // Refresh the list
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while saving changes: {ex.Message}");
                }
            }
        }
        private bool ValidateInputs(out double mileage, out int price, out int topSpeed,
                                         out int accelerationRating, out int speedRating, out int brakingRating,
                                         out int difficultyRating, out double accelerationTime, out int maxPower,
                                         out int maxPowerRPM, out int maxTorque, out int maxTorqueRPM, out int weight)
        {
            // Initialize out parameters
            mileage = 0;
            price = 0;
            topSpeed = 0;
            accelerationRating = 0;
            speedRating = 0;
            brakingRating = 0;
            difficultyRating = 0;
            accelerationTime = 0;
            maxPower = 0;
            maxPowerRPM = 0;
            maxTorque = 0;
            maxTorqueRPM = 0;
            weight = 0;

            bool isValid = true;

            // Validate mileage, price, and top speed
            isValid &= (SI == "Metric" ? double.TryParse(Odometer_Metric.Text, out mileage) : double.TryParse(Odometer_Imperial.Text, out mileage)) &&
                       int.TryParse(VehiclePriceTextBox.Text, out price) &&
                       int.TryParse(TopSpeedTextBox.Text, out topSpeed);

            // Validate sliders
            isValid &= int.TryParse(AccelerationSlider.Value.ToString(), out accelerationRating) &&
                       int.TryParse(SpeedRatingSlider.Value.ToString(), out speedRating) &&
                       int.TryParse(BrakingRatingSlider.Value.ToString(), out brakingRating) &&
                       int.TryParse(DifficultyRatingSlider.Value.ToString(), out difficultyRating);

            // Validate acceleration, max power, max power RPM, max torque, max torque RPM, and weight
            isValid &= double.TryParse(AccelerationTextBox.Text, out accelerationTime) &&
                       int.TryParse(MaxPowerTextBox.Text, out maxPower) &&
                       int.TryParse(MaxPowerRPMTextBox.Text, out maxPowerRPM) &&
                       int.TryParse(MaxTorqueTextBox.Text, out maxTorque) &&
                       int.TryParse(MaxTorqueRPMTextBox.Text, out maxTorqueRPM) &&
                       int.TryParse(WeightTextBox.Text, out weight);

            return isValid;
        }
        private void SetVehicleDetails(VehicleManagement vehicle, double mileage, int price, int topSpeed,
                                        int accelerationRating, int speedRating, int brakingRating, int difficultyRating,
                                        double accelerationTime, int maxPower, int maxPowerRPM, int maxTorque,
                                        int maxTorqueRPM, int weight)
        {
            vehicle.Name = VehicleBrandComboBox.Visibility == Visibility.Visible
                ? VehicleBrandComboBox.Text + " " + VehicleModelTextBox.Text
                : VehicleBrandTextBox.Text + " " + VehicleModelTextBox.Text;
            vehicle.Brand = VehicleBrandComboBox.Text;
            vehicle.Model = VehicleModelTextBox.Text;
            vehicle.Class = VehicleClassComboBox.Text;

            if (SI == "Metric")
            {
                vehicle.OdometerMetric = mileage;
                vehicle.OdometerImperial = Math.Round(mileage * 0.621371,1);
            }
            else
            {
                vehicle.OdometerImperial = mileage;
                vehicle.OdometerMetric = Math.Round(mileage * 1.60934,1);
            }

            vehicle.Active = VehicleActiveCheckBox.IsChecked == true ? 1 : 0;
            vehicle.Price = price;
            vehicle.Owned = VehicleOwnedCheckBox.IsChecked == true ? 1 : 0;
            vehicle.AccelerationRating = accelerationRating;
            vehicle.SpeedRating = speedRating;
            vehicle.BrakingRating = brakingRating;
            vehicle.DifficultyRating = difficultyRating;
            vehicle.AccelerationTime = accelerationTime;
            vehicle.TopSpeed = topSpeed;
            vehicle.Engine = EngineSizeTextBox.Text;
            vehicle.EngineLayout = EngineLayoutComboBox.Text;
            vehicle.Gearbox = GearboxComboBox.Text;
            vehicle.MaxPower = maxPower;
            vehicle.MaxPowerRPM = maxPowerRPM;
            vehicle.MaxTorque = maxTorque;
            vehicle.MaxTorqueRPM = maxTorqueRPM;
            vehicle.Weight = weight;
        }

        private void AddParameters(SQLiteCommand cmd, VehicleManagement vehicle)
        {
            cmd.Parameters.AddWithValue("@id", vehicle.Id);
            cmd.Parameters.AddWithValue("@name", vehicle.Name);
            cmd.Parameters.AddWithValue("@brand", vehicle.Brand);
            cmd.Parameters.AddWithValue("@model", vehicle.Model);
            cmd.Parameters.AddWithValue("@class", vehicle.Class);
            cmd.Parameters.AddWithValue("@odometerMetric", vehicle.OdometerMetric);
            cmd.Parameters.AddWithValue("@odometerImperial", vehicle.OdometerImperial);
            cmd.Parameters.AddWithValue("@owned", vehicle.Owned);
            cmd.Parameters.AddWithValue("@active", vehicle.Active);
            cmd.Parameters.AddWithValue("@price", vehicle.Price);
            cmd.Parameters.AddWithValue("@accelerationRating", vehicle.AccelerationRating);
            cmd.Parameters.AddWithValue("@speedRating", vehicle.SpeedRating);
            cmd.Parameters.AddWithValue("@brakingRating", vehicle.BrakingRating);
            cmd.Parameters.AddWithValue("@difficultyRating", vehicle.DifficultyRating);
            cmd.Parameters.AddWithValue("@accelerationTime", vehicle.AccelerationTime);
            cmd.Parameters.AddWithValue("@topSpeed", vehicle.TopSpeed);
            cmd.Parameters.AddWithValue("@engine", vehicle.Engine);
            cmd.Parameters.AddWithValue("@layout", vehicle.EngineLayout);
            cmd.Parameters.AddWithValue("@gearbox", vehicle.Gearbox);
            cmd.Parameters.AddWithValue("@maxPower", vehicle.MaxPower);
            cmd.Parameters.AddWithValue("@maxPowerRPM", vehicle.MaxPowerRPM);
            cmd.Parameters.AddWithValue("@maxTorque", vehicle.MaxTorque);
            cmd.Parameters.AddWithValue("@maxTorqueRPM", vehicle.MaxTorqueRPM);
            cmd.Parameters.AddWithValue("@weight", vehicle.Weight);
        }


        private void AddVehicleButton_Click(object sender, RoutedEventArgs e)
        {
            double meter_Metric = 0.0;
            double meter_Imperial = 0.0;
            string query;
            string brand;
            if (VehicleBrandComboBox.Visibility == Visibility.Visible)
            {
                brand = VehicleBrandComboBox.Text;
            }
            else
            {
                brand = VehicleBrandTextBox.Text;
            }
            string model = VehicleModelTextBox.Text;
            string name = brand + " " + model;
            string vehicleClass = VehicleClassComboBox.Text;
            if (SI == "Metric") {
                meter_Metric = double.Parse(Odometer_Metric.Text);
                meter_Imperial = meter_Metric * 0.621371;
            } else
            {
                meter_Imperial = double.Parse(Odometer_Imperial.Text);
                meter_Metric = meter_Imperial * 1.60934;
            }
            int races = 0;
            int price = int.Parse(VehiclePriceTextBox.Text);
            bool isActive = VehicleActiveCheckBox.IsChecked.GetValueOrDefault();
            int isactive = 0;
            if (isActive == true)
            {
                isactive = 1;
            }
            bool isOwned = VehicleOwnedCheckBox.IsChecked.GetValueOrDefault();
            int isowned = 0;
            if (isOwned == true)
            {
                isowned = 1;
            }
            int acceleration = Convert.ToInt32(AccelerationSlider.Value);
            int speedRating = Convert.ToInt32(SpeedRatingSlider.Value);
            int brakingRating = Convert.ToInt32(BrakingRatingSlider.Value);
            int difficultyRating = Convert.ToInt32(DifficultyRatingSlider.Value);
            double accelerationVal = Convert.ToDouble(AccelerationTextBox.Text);
            int topSpeed = int.Parse(TopSpeedTextBox.Text);
            string engineSize = EngineSizeTextBox.Text;
            string engineLayout = EngineLayoutComboBox.Text;
            string gearbox = GearboxComboBox.Text;
            int maxTorque = int.Parse(MaxTorqueTextBox.Text);
            int maxTorqueRPM = int.Parse(MaxTorqueRPMTextBox.Text);
            int maxPower = int.Parse(MaxPowerTextBox.Text);
            int maxPowerRPM = int.Parse(MaxPowerRPMTextBox.Text);
            int weight = int.Parse(WeightTextBox.Text);

            using (var conn = new SQLiteConnection(Settings.Default.connectionString))
            {
                conn.Open();
            query = @"
            INSERT INTO vehicles 
            (Name, Brand, Model, Class, Odometer_Metric, Odometer_Imperial, Races_Ran, Price, Active, Owned, Acceleration_Stat, Speed_Stat, Braking_Stat, 
            Difficulty_Stat, Top_Speed, Acceleration, Engine, Engine_Layout, 
            Gearbox, Max_Torque, Max_TorqueRPM, Max_Power, Max_PowerRPM, Weight) 
            VALUES 
            (@name, @brand, @model ,@class, @meter_Metric, @meter_Imperial, @races, @price, @active, @owned, @acceleration, @speedRating, @brakingRating,
            @difficultyRating, @topSpeed, @accelerationValue, @engineSize, @engineLayout,
            @gearbox, @maxTorque, @maxTorqueRPM, @maxPower, @maxPowerRPM, @weight)";
             

                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@brand", brand);
                    cmd.Parameters.AddWithValue("@model", model);
                    cmd.Parameters.AddWithValue("@class", vehicleClass);
                    cmd.Parameters.AddWithValue("@races", races);
                    cmd.Parameters.AddWithValue("@meter_Metric", meter_Metric);
                    cmd.Parameters.AddWithValue("@meter_Imperial", meter_Imperial);
                    cmd.Parameters.AddWithValue("@active", isactive);
                    cmd.Parameters.AddWithValue("@owned", isowned);
                    cmd.Parameters.AddWithValue("@price", price);
                    cmd.Parameters.AddWithValue("@acceleration", acceleration);
                    cmd.Parameters.AddWithValue("@speedRating", speedRating);
                    cmd.Parameters.AddWithValue("@brakingRating", brakingRating);
                    cmd.Parameters.AddWithValue("@difficultyRating", difficultyRating);
                    cmd.Parameters.AddWithValue("@topSpeed", topSpeed);
                    cmd.Parameters.AddWithValue("@accelerationValue", accelerationVal);
                    cmd.Parameters.AddWithValue("@engineSize", engineSize);
                    cmd.Parameters.AddWithValue("@engineLayout", engineLayout);
                    cmd.Parameters.AddWithValue("@gearbox", gearbox);
                    cmd.Parameters.AddWithValue("@maxTorque", maxTorque);
                    cmd.Parameters.AddWithValue("@maxTorqueRPM", maxTorqueRPM);
                    cmd.Parameters.AddWithValue("@maxPower", maxPower);
                    cmd.Parameters.AddWithValue("@maxPowerRPM", maxPowerRPM);
                    cmd.Parameters.AddWithValue("@weight", weight);

                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Vehicle Added");
            BindComboBox(VehicleBrandComboBox);
        }

        private void DeleteVehicleButton_Click(object sender, RoutedEventArgs e)
        {
            if (VehicleDataGrid.SelectedItem == null) return;

            var selectedVehicle = (VehicleManagement)VehicleDataGrid.SelectedItem;  // Assume you have a Vehicle class

            using (var conn = new SQLiteConnection(Settings.Default.connectionString))
            {
                conn.Open();
                string query = "DELETE FROM Vehicles WHERE Id = @id";
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", selectedVehicle.Id);  // Assuming your Vehicle class has an Id property
                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Vehicle Deleted");
            LoadVehiclesDataGrid();  // Refresh the list after deletion
        }

        private void LoadVehiclesDataGrid()
        {
            try
            {
                using (var conn = new SQLiteConnection(Settings.Default.connectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM vehicles";
                    using (var cmd = new SQLiteCommand(query, conn))
                    {
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            var vehicles = new List<VehicleManagement>();

                            while (reader.Read())
                            {
                                var vehicle = new VehicleManagement
                                {
                                    Id = reader.IsDBNull(0) ? default : reader.GetInt32(0),
                                    Name = reader.IsDBNull(1) ? null : reader.GetString(1),
                                    Brand = reader.IsDBNull(2) ? null : reader.GetString(2),
                                    Model = reader.IsDBNull(3) ? null : reader.GetString(3),
                                    Class = reader.IsDBNull(4) ? null : reader.GetString(4),
                                    RacesRan = reader.IsDBNull(5) ? default : reader.GetInt32(5),
                                    OdometerMetric = reader.IsDBNull(6) ? default : reader.GetDouble(6),
                                    OdometerImperial = reader.IsDBNull(7) ? default : reader.GetDouble(7),
                                    Active = reader.IsDBNull(8) ? default : reader.GetInt32(8),
                                    Owned = reader.IsDBNull(9) ? default : reader.GetInt32(9),
                                    Price = reader.IsDBNull(10) ? default : reader.GetInt32(10),
                                    AccelerationStat = reader.IsDBNull(11) ? default : reader.GetInt32(11),
                                    SpeedStat = reader.IsDBNull(12) ? default : reader.GetInt32(12),
                                    BrakingStat = reader.IsDBNull(13) ? default : reader.GetInt32(13),
                                    DifficultyStat = reader.IsDBNull(14) ? default : reader.GetInt32(14),
                                    TopSpeed = reader.IsDBNull(15) ? default : reader.GetInt32(15),
                                    Acceleration = reader.IsDBNull(16) ? default : reader.GetDouble(16),
                                    Engine = reader.IsDBNull(17) ? default : reader.GetString(17), // Update to GetString
                                    EngineLayout = reader.IsDBNull(18) ? null : reader.GetString(18),
                                    Gearbox = reader.IsDBNull(19) ? null : reader.GetString(19),
                                    MaxTorque = reader.IsDBNull(20) ? default : reader.GetInt32(20),
                                    MaxTorqueRPM = reader.IsDBNull(21) ? default : reader.GetInt32(21),
                                    MaxPower = reader.IsDBNull(22) ? default : reader.GetInt32(22),
                                    MaxPowerRPM = reader.IsDBNull(23) ? default : reader.GetInt32(23),
                                    Weight = reader.IsDBNull(24) ? default : reader.GetInt32(24)
                                };

                                vehicles.Add(vehicle);
                            }

                            // Bind data to the DataGrids
                            VehicleDataGrid.ItemsSource = vehicles;
                            EditVehicleDataGrid.ItemsSource = vehicles;
                        }
                    }
                }
            }
            catch (InvalidCastException ex)
            {
                MessageBox.Show($"Invalid cast: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }





        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            // Open file dialog to select an image
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png";

            if (openFileDialog.ShowDialog() == true)
            {
                // Load the selected image into the Image control
                BitmapImage bitmap = new BitmapImage(new Uri(openFileDialog.FileName));
                UploadedImage.Source = bitmap;
            }
        }

        private void SaveToDbButton_Click(object sender, RoutedEventArgs e)
        {
            // Ensure a vehicle is selected
            if (VehicleSelection.SelectedItem is VehicleManagement selectedVehicle)
            {
                // Convert the image to a byte array if an image is uploaded
                byte[] imageData = null;
                if (UploadedImage.Source is BitmapImage bitmapImage)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                        encoder.Save(ms);
                        imageData = ms.ToArray();
                    }
                }

                try
                {
                    using (var conn = new SQLiteConnection(Settings.Default.connectionString))
                    {
                        conn.Open();
                        string query = @"
                    UPDATE vehicles
                    SET 
                        Brand = @brand, 
                        Model = @model, 
                        Class = @class, 
                        Odometer_Metric = @odometerMetric, 
                        Odometer_Imperial = @odometerImperial, 
                        Active = @active, 
                        Owned = @owned, 
                        Price = @price, 
                        Acceleration_Stat = @accelerationStat, 
                        Speed_Stat = @speedStat, 
                        Braking_Stat = @brakingStat, 
                        Difficulty_Stat = @difficultyStat, 
                        Top_Speed = @topSpeed, 
                        Acceleration_Time = @acceleration, 
                        Engine = @engine, 
                        Engine_Layout = @engineLayout, 
                        Gearbox = @gearbox, 
                        Max_Torque = @maxTorque, 
                        Max_TorqueRPM = @maxTorqueRPM, 
                        Max_Power = @maxPower, 
                        Max_PowerRPM = @maxPowerRPM, 
                        Weight = @weight,
                        Image = @image -- This line updates the image
                    WHERE Id = @id";

                        using (var cmd = new SQLiteCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", selectedVehicle.Id);
                            cmd.Parameters.AddWithValue("@brand", selectedVehicle.Brand);
                            cmd.Parameters.AddWithValue("@model", selectedVehicle.Model);
                            cmd.Parameters.AddWithValue("@class", selectedVehicle.Class);
                            cmd.Parameters.AddWithValue("@odometerMetric", selectedVehicle.OdometerMetric);
                            cmd.Parameters.AddWithValue("@odometerImperial", selectedVehicle.OdometerImperial);
                            cmd.Parameters.AddWithValue("@active", selectedVehicle.Active);
                            cmd.Parameters.AddWithValue("@owned", selectedVehicle.Owned);
                            cmd.Parameters.AddWithValue("@price", selectedVehicle.Price);
                            cmd.Parameters.AddWithValue("@accelerationStat", selectedVehicle.AccelerationStat);
                            cmd.Parameters.AddWithValue("@speedStat", selectedVehicle.SpeedStat);
                            cmd.Parameters.AddWithValue("@brakingStat", selectedVehicle.BrakingStat);
                            cmd.Parameters.AddWithValue("@difficultyStat", selectedVehicle.DifficultyStat);
                            cmd.Parameters.AddWithValue("@topSpeed", selectedVehicle.TopSpeed);
                            cmd.Parameters.AddWithValue("@acceleration", selectedVehicle.Acceleration);
                            cmd.Parameters.AddWithValue("@engine", selectedVehicle.Engine);
                            cmd.Parameters.AddWithValue("@engineLayout", selectedVehicle.EngineLayout);
                            cmd.Parameters.AddWithValue("@gearbox", selectedVehicle.Gearbox);
                            cmd.Parameters.AddWithValue("@maxTorque", selectedVehicle.MaxTorque);
                            cmd.Parameters.AddWithValue("@maxTorqueRPM", selectedVehicle.MaxTorqueRPM);
                            cmd.Parameters.AddWithValue("@maxPower", selectedVehicle.MaxPower);
                            cmd.Parameters.AddWithValue("@maxPowerRPM", selectedVehicle.MaxPowerRPM);
                            cmd.Parameters.AddWithValue("@weight", selectedVehicle.Weight);
                            cmd.Parameters.AddWithValue("@image", imageData); // Image data

                            cmd.ExecuteNonQuery();
                        }
                    }
                    MessageBox.Show("Vehicle details and image updated successfully.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while saving the data: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Please select a vehicle.");
            }
        }


        private void LoadFromDbButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the selected vehicle
            if (VehicleSelection.SelectedItem is VehicleManagement selectedVehicle)
            {
                try
                {
                    using (var conn = new SQLiteConnection(Settings.Default.connectionString))
                    {
                        conn.Open();
                        string query = "SELECT Image FROM vehicles WHERE Id = @id";
                        using (var cmd = new SQLiteCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", selectedVehicle.Id);

                            byte[] imageData = cmd.ExecuteScalar() as byte[];
                            if (imageData != null && imageData.Length > 0)
                            {
                                // Convert byte array to image and display in the Image control
                                BitmapImage bitmap = new BitmapImage();
                                using (MemoryStream ms = new MemoryStream(imageData))
                                {
                                    bitmap.BeginInit();
                                    bitmap.StreamSource = ms;
                                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                    bitmap.EndInit();
                                }
                                UploadedImage.Source = bitmap;
                            }
                            else
                            {
                                MessageBox.Show("No image found for the selected vehicle.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while loading the image: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Please select a vehicle.");
            }
        }
        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (ImageContentPanel.Visibility == Visibility.Visible)
            {
                ImageContentPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                ImageContentPanel.Visibility = Visibility.Visible;
            }
        }

        private void Image_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (VehicleBrandComboBox.Visibility == Visibility.Visible)
            {
                btn_BrandCancel.Visibility = Visibility.Visible;
                VehicleBrandTextBox.Visibility = Visibility.Visible;
                VehicleBrandComboBox.Visibility = Visibility.Collapsed;
                btn_BrandAdd.Visibility = Visibility.Collapsed;
            }
            else
            {
                btn_BrandCancel.Visibility = Visibility.Collapsed;
                VehicleBrandTextBox.Visibility = Visibility.Collapsed;
                VehicleBrandComboBox.Visibility = Visibility.Visible;
                btn_BrandAdd.Visibility = Visibility.Visible;
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (Tabctrler.SelectedIndex == 0)
            {
                extras.Visibility = Visibility.Visible;
            }
            else
            {
                extras.Visibility = Visibility.Collapsed;
            }
        }

        private void VehicleDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Ensure an item is selected
            if (VehicleSelection.SelectedItem == null) return;

            // Cast the selected item to the correct type
            var selectedVehicle = VehicleSelection.SelectedItem as VehicleManagement;

            // Ensure casting was successful
            if (selectedVehicle == null) return;

            // Populate the fields with the selected vehicle's details
            PopulateVehicleDetailsDataGrid(selectedVehicle);
        }
        private void PopulateVehicleDetailsDataGrid(VehicleManagement selectedVehicle)
        {
            if (VehicleBrandComboBox.Visibility == Visibility.Visible)
            {
                VehicleBrandComboBox.Text = selectedVehicle.Brand;
            }
            else
            {
                VehicleBrandTextBox.Text = selectedVehicle.Brand;
            }

            VehicleModelTextBox.Text = selectedVehicle.Model;
            VehicleClassComboBox.Text = selectedVehicle.Class;

            if (SI == "Metric")
            {
                Odometer_Metric.Text = selectedVehicle.OdometerMetric.ToString();
                Odometer_Metric.Visibility = Visibility.Visible;
                Odometer_Imperial.Visibility = Visibility.Collapsed;
            }
            else
            {
                Odometer_Imperial.Text = selectedVehicle.OdometerImperial.ToString();
                Odometer_Metric.Visibility = Visibility.Collapsed;
                Odometer_Imperial.Visibility = Visibility.Visible;
            }

            VehiclePriceTextBox.Text = selectedVehicle.Price.ToString();
            VehicleActiveCheckBox.IsChecked = Convert.ToBoolean(selectedVehicle.Active);
            VehicleOwnedCheckBox.IsChecked = Convert.ToBoolean(selectedVehicle.Owned);
            AccelerationSlider.Value = selectedVehicle.AccelerationStat;
            SpeedRatingSlider.Value = selectedVehicle.SpeedStat;
            BrakingRatingSlider.Value = selectedVehicle.BrakingStat;
            DifficultyRatingSlider.Value = selectedVehicle.DifficultyStat;
            TopSpeedTextBox.Text = selectedVehicle.TopSpeed.ToString();
            AccelerationTextBox.Text = selectedVehicle.AccelerationTime.ToString();
            EngineSizeTextBox.Text = selectedVehicle.Engine;
            EngineLayoutComboBox.Text = selectedVehicle.EngineLayout;
            GearboxComboBox.Text = selectedVehicle.Gearbox;
            MaxTorqueTextBox.Text = selectedVehicle.MaxTorque.ToString();
            MaxTorqueRPMTextBox.Text = selectedVehicle.MaxTorqueRPM.ToString();
            MaxPowerTextBox.Text = selectedVehicle.MaxPower.ToString();
            MaxPowerRPMTextBox.Text = selectedVehicle.MaxPowerRPM.ToString();
            WeightTextBox.Text = selectedVehicle.Weight.ToString();
        }


        private void EditVehicleListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Tabctrler.SelectedIndex = 0;
            if (EditVehicleDataGrid.SelectedItem == null) return;

            var selectedVehicle = (VehicleManagement)EditVehicleDataGrid.SelectedItem;

            // Populate the fields with the selected vehicle's details
            if (selectedVehicle == null) return;

            if (VehicleBrandComboBox.Visibility == Visibility.Visible)
            {
                VehicleBrandComboBox.Text = selectedVehicle.Brand;
            }
            else
            {
                VehicleBrandTextBox.Text = selectedVehicle.Brand;
            }
            VehicleModelTextBox.Text = selectedVehicle.Model;
            VehicleClassComboBox.Text = selectedVehicle.Class;
            if (SI == "Metric") { 
                Odometer_Metric.Text = selectedVehicle.OdometerMetric.ToString();
                Odometer_Metric.Visibility = Visibility.Visible;
                Odometer_Imperial.Visibility = Visibility.Collapsed;
            } else
            {
                Odometer_Imperial.Text = selectedVehicle.OdometerImperial.ToString();
                Odometer_Metric.Visibility = Visibility.Collapsed;
                Odometer_Imperial.Visibility = Visibility.Visible;
            }
            VehiclePriceTextBox.Text = selectedVehicle.Price.ToString();
            VehicleActiveCheckBox.IsChecked = Convert.ToBoolean(selectedVehicle.Active);
            VehicleOwnedCheckBox.IsChecked = Convert.ToBoolean(selectedVehicle.Owned);
            AccelerationSlider.Value = selectedVehicle.AccelerationStat;
            SpeedRatingSlider.Value = selectedVehicle.SpeedStat;
            BrakingRatingSlider.Value = selectedVehicle.BrakingStat;
            DifficultyRatingSlider.Value = selectedVehicle.DifficultyStat;
            TopSpeedTextBox.Text = selectedVehicle.TopSpeed.ToString();
            AccelerationTextBox.Text = selectedVehicle.AccelerationTime.ToString();
            EngineSizeTextBox.Text = selectedVehicle.Engine;
            EngineLayoutComboBox.Text = selectedVehicle.EngineLayout;
            GearboxComboBox.Text = selectedVehicle.Gearbox;
            MaxTorqueTextBox.Text = selectedVehicle.MaxTorque.ToString();
            MaxTorqueRPMTextBox.Text = selectedVehicle.MaxTorqueRPM.ToString();
            MaxPowerTextBox.Text = selectedVehicle.MaxPower.ToString();
            MaxPowerRPMTextBox.Text = selectedVehicle.MaxPowerRPM.ToString();
            WeightTextBox.Text = selectedVehicle.Weight.ToString();
        }
    }
}