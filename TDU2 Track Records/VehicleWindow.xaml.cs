using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
        readonly string SI = Settings.Default.system;

        public Vehicle()
        {
            InitializeComponent();
            BindComboBox(VehicleBrandComboBox);
            BindComboBox(VehicleSelection);
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
            return unit == "mph" ? Math.Round(speed * 0.621371,0) : speed;  // Convert to mph if selected
        }

        private double ConvertWeightToSelectedUnit(double weight, string unit)
        {
            return unit == "lbs" ? Math.Round(weight * 2.20462,0) : weight;  // Convert to lbs if selected
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
                                    Price = reader.IsDBNull(10) ? default : reader.GetString(10),
                                    AccelerationStat = reader.IsDBNull(11) ? default : reader.GetInt32(11),
                                    SpeedStat = reader.IsDBNull(12) ? default : reader.GetInt32(12),
                                    BrakingStat = reader.IsDBNull(13) ? default : reader.GetInt32(13),
                                    DifficultyStat = reader.IsDBNull(14) ? default : reader.GetInt32(14),
                                    TopSpeed = reader.IsDBNull(15) ? default : reader.GetInt32(15),
                                    AccelerationTime = reader.IsDBNull(16) ? default : reader.GetDouble(16),
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
            string selectedSpeedUnit = Settings.Default.speed;
            string selectedWeightUnit = Settings.Default.weight;
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

            VehiclePriceTextBox.Text = selectedVehicle.Price;
            VehicleActiveCheckBox.IsChecked = Convert.ToBoolean(selectedVehicle.Active);
            VehicleOwnedCheckBox.IsChecked = Convert.ToBoolean(selectedVehicle.Owned);
            AccelerationStatSlider.Value = selectedVehicle.AccelerationStat;
            SpeedStatSlider.Value = selectedVehicle.SpeedStat;
            BrakingStatSlider.Value = selectedVehicle.BrakingStat;
            DifficultyStatSlider.Value = selectedVehicle.DifficultyStat;
            TopSpeedTextBox.Text = ConvertSpeedToSelectedUnit(selectedVehicle.TopSpeed, selectedSpeedUnit).ToString();
            AccelerationTextBox.Text = selectedVehicle.AccelerationTime.ToString();
            EngineSizeTextBox.Text = selectedVehicle.Engine;
            EngineLayoutComboBox.Text = selectedVehicle.EngineLayout;
            GearboxComboBox.Text = selectedVehicle.Gearbox;
            MaxTorqueTextBox.Text = selectedVehicle.MaxTorque.ToString();
            MaxTorqueRPMTextBox.Text = selectedVehicle.MaxTorqueRPM.ToString();
            MaxPowerTextBox.Text = selectedVehicle.MaxPower.ToString();
            MaxPowerRPMTextBox.Text = selectedVehicle.MaxPowerRPM.ToString();
            WeightTextBox.Text = ConvertWeightToSelectedUnit(selectedVehicle.Weight, selectedWeightUnit).ToString();
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
        }
        private void UpdateVehicleDetails()
        {
            if (!ValidateInputs(out double mileage, out string price, out int topSpeed,
                                out int accelerationStat, out int speedStat, out int brakingStat,
                                out int difficultyStat, out double accelerationTime, out int maxPower,
                                out int maxPowerRPM, out int maxTorque, out int maxTorqueRPM, out int weight))
            {
                MessageBox.Show("Please ensure all fields are correctly formatted.");
                return;
            }

            if (VehicleSelection.SelectedItem is VehicleManagement selectedVehicle)
            {
                SetVehicleDetails(selectedVehicle, mileage, price, topSpeed, accelerationStat,
                                  speedStat, brakingStat, difficultyStat, accelerationTime,
                                  maxPower, maxPowerRPM, maxTorque, maxTorqueRPM, weight);

                // Convert the image to a byte array
                byte[] imageBytes = ConvertImageToByteArray(UploadedImage.Source as BitmapImage);

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
                            Acceleration_Stat = @accelerationStat,
                            Speed_Stat = @speedStat,
                            Braking_Stat = @brakingStat, 
                            Difficulty_Stat = @difficultyStat,
                            Top_Speed = @topSpeed,
                            Acceleration_Time = @accelerationTime,
                            Engine = @engine,
                            Engine_Layout = @layout,
                            Gearbox = @gearbox,
                            Max_Power = @maxPower,
                            Max_PowerRPM = @maxPowerRPM,
                            Max_Torque = @maxTorque,
                            Max_TorqueRPM = @maxTorqueRPM, 
                            Weight = @weight,
                            Image = @image
                            WHERE Id = @id";

                try
                {
                    ExecuteNonQuery(query, cmd =>
                    {
                        AddParameters(cmd, selectedVehicle, imageBytes);
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
        private byte[] ConvertImageToByteArray(BitmapImage bitmapImage)
        {
            using (var ms = new MemoryStream())
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                encoder.Save(ms);
                return ms.ToArray();
            }
        }
        private bool ValidateInputs(out double mileage, out string price, out int topSpeed,
                                         out int accelerationStat, out int speedStat, out int brakingStat,
                                         out int difficultyStat, out double accelerationTime, out int maxPower,
                                         out int maxPowerRPM, out int maxTorque, out int maxTorqueRPM, out int weight)
        {
            // Initialize out parameters
            mileage = 0;
            topSpeed = 0;
            accelerationStat = 0;
            speedStat = 0;
            price = "";
            brakingStat = 0;
            difficultyStat = 0;
            accelerationTime = 0;
            maxPower = 0;
            maxPowerRPM = 0;
            maxTorque = 0;
            maxTorqueRPM = 0;
            weight = 0;

            bool isValid = true;

            // Validate mileage, price, and top speed
            isValid &= (SI == "Metric" ? double.TryParse(Odometer_Metric.Text, out mileage) : double.TryParse(Odometer_Imperial.Text, out mileage)) &&
                       int.TryParse(TopSpeedTextBox.Text, out topSpeed);

            // Validate sliders
            isValid &= int.TryParse(AccelerationStatSlider.Value.ToString(), out accelerationStat) &&
                       int.TryParse(SpeedStatSlider.Value.ToString(), out speedStat) &&
                       int.TryParse(BrakingStatSlider.Value.ToString(), out brakingStat) &&
                       int.TryParse(DifficultyStatSlider.Value.ToString(), out difficultyStat);

            // Validate acceleration, max power, max power RPM, max torque, max torque RPM, and weight
            isValid &= double.TryParse(AccelerationTextBox.Text, out accelerationTime) &&
                       int.TryParse(MaxPowerTextBox.Text, out maxPower) &&
                       int.TryParse(MaxPowerRPMTextBox.Text, out maxPowerRPM) &&
                       int.TryParse(MaxTorqueTextBox.Text, out maxTorque) &&
                       int.TryParse(MaxTorqueRPMTextBox.Text, out maxTorqueRPM) &&
                       int.TryParse(WeightTextBox.Text, out weight);

            return isValid;
        }
 
        private void SetVehicleDetails(VehicleManagement vehicle, double mileage, string price, int topSpeed,
                                        int accelerationStat, int speedStat, int brakingStat, int difficultyStat,
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

            vehicle.Owned = VehicleOwnedCheckBox.IsChecked == true ? 1 : 0;
            vehicle.Active = VehicleActiveCheckBox.IsChecked == true ? 1 : 0;
            vehicle.Price = price;
            vehicle.AccelerationStat = accelerationStat;
            vehicle.SpeedStat = speedStat;
            vehicle.BrakingStat = brakingStat;
            vehicle.DifficultyStat = difficultyStat;
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

        private void AddParameters(SQLiteCommand cmd, VehicleManagement vehicle, byte[] image)
        {
            cmd.Parameters.AddWithValue("@name", vehicle.Name ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@brand", vehicle.Brand ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@model", vehicle.Model ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@class", vehicle.Class ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@odometerMetric", vehicle.OdometerMetric);
            cmd.Parameters.AddWithValue("@odometerImperial", vehicle.OdometerImperial);
            cmd.Parameters.AddWithValue("@owned", vehicle.Owned);
            cmd.Parameters.AddWithValue("@active", vehicle.Active);
            cmd.Parameters.AddWithValue("@price", vehicle.Price);
            cmd.Parameters.AddWithValue("@accelerationStat", vehicle.AccelerationStat);
            cmd.Parameters.AddWithValue("@speedStat", vehicle.SpeedStat);
            cmd.Parameters.AddWithValue("@brakingStat", vehicle.BrakingStat);
            cmd.Parameters.AddWithValue("@difficultyStat", vehicle.DifficultyStat);
            cmd.Parameters.AddWithValue("@topSpeed", vehicle.TopSpeed);
            cmd.Parameters.AddWithValue("@accelerationTime", vehicle.AccelerationTime);
            cmd.Parameters.AddWithValue("@engine", vehicle.Engine ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@layout", vehicle.EngineLayout ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@gearbox", vehicle.Gearbox ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@maxPower", vehicle.MaxPower);
            cmd.Parameters.AddWithValue("@maxPowerRPM", vehicle.MaxPowerRPM);
            cmd.Parameters.AddWithValue("@maxTorque", vehicle.MaxTorque);
            cmd.Parameters.AddWithValue("@maxTorqueRPM", vehicle.MaxTorqueRPM);
            cmd.Parameters.AddWithValue("@weight", vehicle.Weight);
            cmd.Parameters.AddWithValue("@image", image ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@id", vehicle.Id);
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
                meter_Imperial = Math.Round(meter_Metric * 0.621371,1);
            } else
            {
                meter_Imperial = double.Parse(Odometer_Imperial.Text);
                meter_Metric = Math.Round(meter_Imperial * 1.60934,1);
            }
            int races = 0;
            string price = VehiclePriceTextBox.Text;
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
            int accelerationStat = Convert.ToInt32(AccelerationStatSlider.Value);
            int speedStat = Convert.ToInt32(SpeedStatSlider.Value);
            int brakingStat = Convert.ToInt32(BrakingStatSlider.Value);
            int difficultyStat = Convert.ToInt32(DifficultyStatSlider.Value);
            double accelerationTime = Convert.ToDouble(AccelerationTextBox.Text);
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
            (Name, Brand, Model, Class, Races_Ran, Odometer_Metric, Odometer_Imperial, Price, Active, Owned, Acceleration_Stat, Speed_Stat, Braking_Stat, 
            Difficulty_Stat, Top_Speed, Acceleration_Time, Engine, Engine_Layout, 
            Gearbox, Max_Torque, Max_TorqueRPM, Max_Power, Max_PowerRPM, Weight, Image) 
            VALUES 
            (@name, @brand, @model ,@class, @races, @meter_Metric, @meter_Imperial, @price, @active, @owned, @acceleration, @speedStat, @brakingStat,
            @difficultyStat, @topSpeed, @accelerationValue, @engineSize, @engineLayout,
            @gearbox, @maxTorque, @maxTorqueRPM, @maxPower, @maxPowerRPM, @weight, @image)";
             

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
                    cmd.Parameters.AddWithValue("@acceleration", accelerationStat);
                    cmd.Parameters.AddWithValue("@speedStat", speedStat);
                    cmd.Parameters.AddWithValue("@brakingStat", brakingStat);
                    cmd.Parameters.AddWithValue("@difficultyStat", difficultyStat);
                    cmd.Parameters.AddWithValue("@topSpeed", topSpeed);
                    cmd.Parameters.AddWithValue("@accelerationValue", accelerationTime);
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
            if (VehicleSelection.SelectedItem == null) return;

            var selectedVehicle = (VehicleManagement)VehicleSelection.SelectedItem;  // Assume you have a Vehicle class

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
            LoadVehicles();  // Refresh the list after deletion
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

        private void ResetVehicle_Click(object sender, RoutedEventArgs e)
        {
            ResetControls(this);
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
                else if (child is TextBlock textBlock)
                {
                    textBlock.Text = string.Empty;
                }
                // Add more control types as needed

                // Recurse into child elements
                ResetControls(child);
            }
        }

    }
}