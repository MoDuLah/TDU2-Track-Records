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
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Linq;

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
            BindComboBoxes();
            GarageGroupBox.Visibility = Visibility.Collapsed;
        }

        private void IslandRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            // Determine the selected island based on which RadioButton is checked
            string selectedIsland = null;

            if (IbizaRadioButton.IsChecked == true)
            {
                OahuRadioButton.IsChecked = false;
                selectedIsland = "Ibiza";
            }
            else if (OahuRadioButton.IsChecked == true)
            {
                IbizaRadioButton.IsChecked = false;
                selectedIsland = "Oahu";
            }

            // Re-bind the HouseComboBox based on the selected island
            BindComboBox(HouseComboBox, selectedIsland);
        }

        private void BindComboBoxes()
        {
            // Bind VehicleBrandComboBox and VehicleSelection without any filter
            BindComboBox(VehicleBrandComboBox);
            BindComboBox(VehicleComboBox);
            LoadVehicles();
            // Initially bind the HouseComboBox without any filter
            BindComboBox(HouseComboBox);
        }

        private void BindComboBox(ComboBox comboBox, string filter = null)
        {
            string query = GetQueryForComboBox(comboBox, filter);

            if (string.IsNullOrEmpty(query))
            {
                MessageBox.Show("Invalid ComboBox provided.");
                return;
            }

            try
            {
                string origin = comboBox == HouseComboBox ? "houses" : "vehicles";

                using (var dbConn = new SQLiteConnection(connectionString))
                using (var dbCmd = new SQLiteCommand(query, dbConn))
                {
                    if (filter != null)
                    {
                        dbCmd.Parameters.AddWithValue("filter", filter); // Use ? placeholder for SQLite
                    }

                    using (var dbAdapter = new SQLiteDataAdapter(dbCmd))
                    {
                        dbConn.Open();

                        var ds = new DataSet();
                        dbAdapter.Fill(ds, origin);

                        comboBox.ItemsSource = ds.Tables[0].DefaultView;
                        comboBox.DisplayMemberPath = GetDisplayMemberPath(comboBox);
                        comboBox.SelectedValuePath = "id";

                    }
                }
            }
            catch (SQLiteException sqlEx)
            {
                MessageBox.Show($"Database error occurred while loading data.\n{sqlEx.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading data.\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetQueryForComboBox(ComboBox comboBox, string filter = null)
        {
            if (comboBox == VehicleBrandComboBox)
            {
                return @"
            SELECT MIN(id) AS id, _brand_name
            FROM vehicles 
            GROUP BY _brand_name 
            ORDER BY _brand_name ASC;";
            }
            else if (comboBox == VehicleComboBox)
            {
                return "SELECT id, _vehicle_name FROM vehicles ORDER BY _vehicle_name ASC;";
            }
            else if (comboBox == HouseComboBox)
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    // Use ? placeholder for SQLite
                    return "SELECT id, Name FROM houses WHERE Island = ? ORDER BY id ASC";
                }
                return "SELECT id, Name FROM houses ORDER BY id ASC";
            }
            else
            {
                return null;
            }
        }

        private string GetDisplayMemberPath(ComboBox comboBox)
        {
            return comboBox == VehicleBrandComboBox ? "_brand_name" : "_vehicle_name";
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
            string errorMSG = "";
            try
            {
                using (var conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    string query = $"SELECT * FROM vehicles ORDER BY _vehicle_name ASC;";
                    using (var cmd = new SQLiteCommand(query, conn))
                    {
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            _vehicles.Clear(); // Clear the list before reloading data.
            
                            // Retrieve column indices once
                            var columnIndices = GetColumnIndices(reader);
            
                            // Log all column names for debugging
                            errorMSG = ("Available columns:\n");
                            foreach (var col in columnIndices)
                            {
                                errorMSG += ($"{col.Key} => {col.Value}\n");
                            }
            
                            while (reader.Read())
                            {
                                var vehicle = new VehicleManagement
                                {
                                    id = GetInt(reader, columnIndices["_id_car"]),
                                    VehicleName = GetString(reader, columnIndices["_vehicle_name"]),
                                    VehicleBrand = GetString(reader, columnIndices["_brand_name"]),
                                    VehicleModel = GetString(reader, columnIndices["_modelfull_name"]),
                                    VehicleTags = GetString(reader, columnIndices["_vehicle_tags"]),
                                    VehicleCategory = GetString(reader, columnIndices["_vehiclecategory_name"]),
                                    EnginePositionName = GetString(reader, columnIndices["_engine_position_name"]),
                                    GearboxName = GetString(reader, columnIndices["_gearbox_name"]),
                                    DriveName = GetString(reader, columnIndices["_drive_name"]),
                                    FrontBrakesDesc = GetString(reader, columnIndices["_brakes_characteristics_front"]),
                                    RearBrakesDesc = GetString(reader, columnIndices["_brakes_characteristics_rear"]),
                                    HouseStoredName = GetString(reader, columnIndices["_house_stored_name"]),
                                    VehicleEnginePosition = GetString(reader, columnIndices["_engine_position"]),
            
                                    // Integer properties
                                    VehiclePrice = GetInt(reader, columnIndices["_price"]),
                                    VehicleEngineDisplacement = GetInt(reader, columnIndices["_displacement"]),
                                    VehicleGearboxType = GetInt(reader, columnIndices["_gearbox_id"]),
                                    VehicleNbGears = GetInt(reader, columnIndices["_nb_gears"]),
                                    VehicleDealershipIdIbiza = GetInt(reader, columnIndices["_dealership_id_in_ibiza"]),
                                    VehicleDealershipIdHawaii = GetInt(reader, columnIndices["_dealership_id_in_hawaii"]),
                                    VehicleFrontTires = GetString(reader, columnIndices["_default_front_tire_description"]),
                                    VehicleRearTires = GetString(reader, columnIndices["_default_rear_tire_description"]),
                                    VehicleInGarage = GetInt(reader, columnIndices["_house_stored_slot"]),
                                    VehicleUpgradeLevel = GetInt(reader, columnIndices["_vehicle_can_upgrade"]),
                                    VehicleRacesRan = GetInt(reader, columnIndices["_races_ran"]),
                                    VehicleAccelerationStat = GetInt(reader, columnIndices["_stat_acc"]),
                                    VehicleSpeedStat = GetInt(reader, columnIndices["_stat_speed"]),
                                    VehicleBrakingStat = GetInt(reader, columnIndices["_stat_break"]),
                                    VehicleDifficultyStat = GetInt(reader, columnIndices["_difficulty"]),
                                    VehicleTopSpeed = GetInt(reader, columnIndices["_max_theorical_speed"]),
                                    VehicleMaxTorque = GetInt(reader, columnIndices["_torque_nm"]),
                                    VehicleMaxTorqueRPM = GetInt(reader, columnIndices["_torque_rpm"]),
                                    VehicleMaxPower = GetInt(reader, columnIndices["_power_bhp"]),
                                    VehicleMaxPowerRPM = GetInt(reader, columnIndices["_power_rpm"]),
                                    VehicleWeight = GetInt(reader, columnIndices["_mass"]),
            
                                    // Double properties
                                    VehicleOdometerMetric = GetDouble(reader, columnIndices["_odometer_metric"]),
                                    VehicleOdometerImperial = GetDouble(reader, columnIndices["_odometer_imperial"]),
                                    VehicleAccelerationTime = GetDouble(reader, columnIndices["_acceleration_0_100_kmh"]),
                                    VehiclePowerWeightRatio = GetDouble(reader, columnIndices["_power_bhp"]) /
                                        (GetDouble(reader, columnIndices["_mass"]) == 0 ? 1 : GetDouble(reader, columnIndices["_mass"])),
            
                                    // Boolean properties
                                    VehicleActive = GetBoolean(reader, columnIndices["_is_active"]),
                                    VehicleOwned = GetBoolean(reader, columnIndices["_is_owned"]),
                                    VehiclePurchasable = GetBoolean(reader, columnIndices["_is_purchasable"]),
            
                                    // Image property
                                    VehicleImage = reader.IsDBNull(columnIndices["_vehicle_image"]) ? null : (byte[])reader["_vehicle_image"], // Adjusted to retrieve BLOB data
                                };
            
                                _vehicles.Add(vehicle);
                            }
            
                            // Bind the vehicles list to the ComboBox
                            VehicleComboBox.ItemsSource = _vehicles;
                            VehicleComboBox.DisplayMemberPath = "VehicleName"; // Or another property to display
                            VehicleComboBox.SelectedValuePath = "id"; // Optional, use if you need to bind the Id
                        }
                    }
                }
            }
            catch (InvalidCastException ex)
            {
                MessageBox.Show($"Invalid cast: {ex.Message}");
            }
            catch (KeyNotFoundException ex)
            {
                MessageBox.Show($"Key not found: {ex.Message}");
                MessageBox.Show(errorMSG);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
            
        }

        private bool GetBoolean(SQLiteDataReader reader, int columnIndex)
        {
            return reader.GetInt32(columnIndex) == 1;
        }

        private Dictionary<string, int> GetColumnIndices(SQLiteDataReader reader)
        {
            var indices = new Dictionary<string, int>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                indices[reader.GetName(i)] = i;
            }
            return indices;
        }

        private int GetInt(SQLiteDataReader reader, int index) => reader.IsDBNull(index) ? default : reader.GetInt32(index);
        private string GetString(SQLiteDataReader reader, int index) => reader.IsDBNull(index) ? null : reader.GetString(index);
        private double GetDouble(SQLiteDataReader reader, int index) => reader.IsDBNull(index) ? default : reader.GetDouble(index);


        private void VehicleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (VehicleComboBox.SelectedItem == null)
            {
                return;
            }
            else
            {
                AddVehicle.IsEnabled = false;
                var selectedVehicle = VehicleComboBox.SelectedItem as VehicleManagement;

                if (selectedVehicle != null)
                {
                    // Populate the UI fields with the selected vehicle's details
                    PopulateVehicleDetails(selectedVehicle);

                    // Update the display of the selected vehicle ID
                    VehicleComboBoxSelectionDisplay.Text = selectedVehicle.id.ToString();
                }
            }
        }


        private void PopulateVehicleDetails(VehicleManagement selectedVehicle)
        {
            string selectedSpeedUnit = Settings.Default.speed;
            string selectedWeightUnit = Settings.Default.weight;

            // Set Vehicle Brand
            VehicleBrandComboBox.Text = selectedVehicle.VehicleBrand ?? string.Empty;

            // Set Vehicle Model and Category
            VehicleModelTextBox.Text = selectedVehicle.VehicleModel ?? string.Empty;
            VehicleClassComboBox.Text = selectedVehicle.VehicleCategory ?? string.Empty;

            // Set Odometer visibility and text
            if (SI == "Metric")
            {
                _odometer_metric.Text = selectedVehicle.VehicleOdometerMetric.ToString("F2");
                _odometer_metric.Visibility = Visibility.Visible;
                _odometer_imperial.Visibility = Visibility.Collapsed;
            }
            else
            {
                _odometer_imperial.Text = selectedVehicle.VehicleOdometerImperial.ToString("F2");
                _odometer_metric.Visibility = Visibility.Collapsed;
                _odometer_imperial.Visibility = Visibility.Visible;
            }

            // Set other vehicle details
            VehiclePriceTextBox.Text = selectedVehicle.VehiclePrice.ToString();
            VehicleActiveCheckBox.IsChecked = selectedVehicle.VehicleActive;
            VehicleOwnedCheckBox.IsChecked = selectedVehicle.VehicleOwned;

            AccelerationStatSlider.Value = selectedVehicle.VehicleAccelerationStat;
            SpeedStatSlider.Value = selectedVehicle.VehicleSpeedStat;
            BrakingStatSlider.Value = selectedVehicle.VehicleBrakingStat;
            DifficultyStatSlider.Value = selectedVehicle.VehicleDifficultyStat;

            TopSpeedTextBox.Text = selectedVehicle.VehicleTopSpeed.ToString();
            MaxPowerTextBox.Text = selectedVehicle.VehicleMaxPower.ToString();
            WeightTextBox.Text = selectedVehicle.VehicleWeight.ToString();

            // Set the vehicle image if available
            if (selectedVehicle.VehicleImage != null && selectedVehicle.VehicleImage.Length > 0)
            {
                using (var ms = new MemoryStream(selectedVehicle.VehicleImage))
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
                UploadedImage.Source = null;
            }
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
            double mileage;
            
            int price, topSpeed, accelerationStat, speedStat, brakingStat, difficultyStat, maxPower, maxPowerRPM, maxTorque, maxTorqueRPM, weight;
            double accelerationTime;

            List<string> errors = ValidateInputs(out mileage, out price, out topSpeed,
                                                out accelerationStat, out speedStat, out brakingStat,
                                                out difficultyStat, out accelerationTime, out maxPower,
                                                out maxPowerRPM, out maxTorque, out maxTorqueRPM, out weight);

            if (errors.Any())
            {
                // Display or handle errors
                foreach (string error in errors)
                {
                    MessageBox.Show(error);
                }
            }
            else
            {

                if (VehicleComboBox.SelectedItem is VehicleManagement selectedVehicle)
                {
                    SetVehicleDetails(selectedVehicle, mileage, price, topSpeed, accelerationStat,
                                      speedStat, brakingStat, difficultyStat, accelerationTime,
                                      maxPower, maxPowerRPM, maxTorque, maxTorqueRPM, weight);

                    // Convert the image to a byte array
                    byte[] imageBytes = ConvertImageToByteArray(UploadedImage.Source as BitmapImage);

                    string query = @"
                            UPDATE vehicles SET 
                            Name = @name,
                            Brand = @brand,
                            Model = @model,
                            _vehiclecategory_name = @class,
                            _odometer_metric = @odometerMetric,
                            _odometer_imperial = @odometerImperial,
                            _is_owned = @owned,
                            _is_active = @active,
                            _price = @price, 
                            _stat_acc = @accelerationStat,
                            _stat_speed = @speedStat,
                            _stat_brake = @brakingStat, 
                            _difficulty = @difficultyStat,
                            _theoretical_Top_Speed = @topSpeed,
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
                        AddVehicle.IsEnabled = true;
                        BindComboBoxes(); // Refresh the ComboBoxes
                        LoadVehicles();  // Refresh the list
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An error occurred while saving changes: {ex.Message}");
                    }
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
        private List<string> ValidateInputs(out double mileage, out int price, out int topSpeed,
                                           out int accelerationStat, out int speedStat, out int brakingStat,
                                           out int difficultyStat, out double accelerationTime, out int maxPower,
                                           out int maxPowerRPM, out int maxTorque, out int maxTorqueRPM, out int weight)
        {
            // Initialize out parameters
            mileage = 0;
            topSpeed = 0;
            accelerationStat = 0;
            speedStat = 0;
            price = 0;
            brakingStat = 0;
            difficultyStat = 0;
            accelerationTime = 0;
            maxPower = 0;
            maxPowerRPM = 0;
            maxTorque = 0;
            maxTorqueRPM = 0;
            weight = 0;

            List<string> errorMessages = new List<string>();

            // Validate mileage
            if (!(SI == "Metric" ? double.TryParse(_odometer_metric.Text, out mileage) : double.TryParse(_odometer_imperial.Text, out mileage)))
            {
                errorMessages.Add("Invalid or empty mileage.");
            }

            // Validate price
            if (string.IsNullOrWhiteSpace(VehiclePriceTextBox.Text)) {
                errorMessages.Add("Price is required.");
            } else
            {
                price = Convert.ToInt32(VehiclePriceTextBox.Text);
            }

            // Validate top speed
            if (!int.TryParse(TopSpeedTextBox.Text, out topSpeed))
            {
                errorMessages.Add("Invalid or empty top speed.");
            }

            // Validate sliders
            if (!int.TryParse(AccelerationStatSlider.Value.ToString(), out accelerationStat))
            {
                errorMessages.Add("Invalid or empty acceleration stat.");
            }

            if (!int.TryParse(SpeedStatSlider.Value.ToString(), out speedStat))
            {
                errorMessages.Add("Invalid or empty speed stat.");
            }

            if (!int.TryParse(BrakingStatSlider.Value.ToString(), out brakingStat))
            {
                errorMessages.Add("Invalid or empty braking stat.");
            }

            if (!int.TryParse(DifficultyStatSlider.Value.ToString(), out difficultyStat))
            {
                errorMessages.Add("Invalid or empty difficulty stat.");
            }

            // Validate acceleration time
            if (!double.TryParse(AccelerationTextBox.Text, out accelerationTime))
            {
                errorMessages.Add("Invalid or empty acceleration time.");
            }

            // Validate max power
            if (!int.TryParse(MaxPowerTextBox.Text, out maxPower))
            {
                errorMessages.Add("Invalid or empty max power.");
            }

            // Validate max power RPM
            if (!int.TryParse(MaxPowerRPMTextBox.Text, out maxPowerRPM))
            {
                errorMessages.Add("Invalid or empty max power RPM.");
            }

            // Validate max torque
            if (!int.TryParse(MaxTorqueTextBox.Text, out maxTorque))
            {
                errorMessages.Add("Invalid or empty max torque.");
            }

            // Validate max torque RPM
            if (!int.TryParse(MaxTorqueRPMTextBox.Text, out maxTorqueRPM))
            {
                errorMessages.Add("Invalid or empty max torque RPM.");
            }

            // Validate weight
            if (!int.TryParse(WeightTextBox.Text, out weight))
            {
                errorMessages.Add("Invalid or empty weight.");
            }

            return errorMessages;
        }
        private void Textbox_GotFocus(object sender, RoutedEventArgs e)
        {
                // Cast the sender to a TextBox
                TextBox textBox = sender as TextBox;

                // Select all text in the TextBox
                textBox.SelectAll();
        
        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            // Check if the text length equals MaxLength
            if (textBox.Text.Length == textBox.MaxLength)
            {
                MoveFocusToNextElement(textBox);
            }
        }
        private void MoveFocusToNextElement(TextBox currentTextBox)
        {
            // Move focus to the next element in the tab order
            currentTextBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private void SetVehicleDetails(VehicleManagement vehicle, double mileage, int price, int topSpeed,
                                        int accelerationStat, int speedStat, int brakingStat, int difficultyStat,
                                        double accelerationTime, int maxPower, int maxPowerRPM, int maxTorque,
                                        int maxTorqueRPM, int weight)
        {
            vehicle.VehicleName = VehicleBrandComboBox.Visibility == Visibility.Visible
                ? VehicleBrandComboBox.Text + " " + VehicleModelTextBox.Text
                : VehicleBrandTextBox.Text + " " + VehicleModelTextBox.Text;
            vehicle.VehicleBrand = VehicleBrandComboBox.Text;
            vehicle.VehicleModel = VehicleModelTextBox.Text;
            vehicle.VehicleCategory = VehicleClassComboBox.Text;

            if (SI == "Metric")
            {
                vehicle.VehicleOdometerMetric = mileage;
                vehicle.VehicleOdometerImperial = Math.Round(mileage * 0.621371,1);
            }
            else
            {
                vehicle.VehicleOdometerImperial = mileage;
                vehicle.VehicleOdometerMetric = Math.Round(mileage * 1.60934,1);
            }

            vehicle.VehicleOwned = VehicleOwnedCheckBox.IsChecked == true ? true : false;
            vehicle.VehicleActive = VehicleActiveCheckBox.IsChecked == true ? true : false;
            vehicle.VehiclePrice = price;
            vehicle.VehicleAccelerationStat = accelerationStat;
            vehicle.VehicleSpeedStat = speedStat;
            vehicle.VehicleBrakingStat = brakingStat;
            vehicle.VehicleDifficultyStat = difficultyStat;
            vehicle.VehicleAccelerationTime = accelerationTime;
            vehicle.VehicleTopSpeed = topSpeed;
            vehicle.VehicleEngineDisplacement = Convert.ToInt32(EngineDisplacementTextBox.Text);
            vehicle.VehicleEnginePosition = EnginePositionComboBox.Tag.ToString();
            vehicle.GearboxName = GearboxComboBox.Text;
            vehicle.VehicleMaxPower = maxPower;
            vehicle.VehicleMaxPowerRPM = maxPowerRPM;
            vehicle.VehicleMaxTorque = maxTorque;
            vehicle.VehicleMaxTorqueRPM = maxTorqueRPM;
            vehicle.VehicleWeight = weight;

        }

        private void AddParameters(SQLiteCommand cmd, VehicleManagement vehicle, byte[] image)
        {
            cmd.Parameters.AddWithValue("@id", vehicle.id);
            cmd.Parameters.AddWithValue("@name", vehicle.VehicleName ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@brand", vehicle.VehicleBrand ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@model", vehicle.VehicleModel ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@class", vehicle.VehicleCategory ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@odometerMetric", vehicle.VehicleOdometerMetric);
            cmd.Parameters.AddWithValue("@odometerImperial", vehicle.VehicleOdometerImperial);
            cmd.Parameters.AddWithValue("@owned", vehicle.VehicleOwned);
            cmd.Parameters.AddWithValue("@active", vehicle.VehicleActive);
            cmd.Parameters.AddWithValue("@price", vehicle.VehiclePrice);
            cmd.Parameters.AddWithValue("@accelerationStat", vehicle.VehicleAccelerationStat);
            cmd.Parameters.AddWithValue("@speedStat", vehicle.VehicleSpeedStat);
            cmd.Parameters.AddWithValue("@brakingStat", vehicle.VehicleBrakingStat);
            cmd.Parameters.AddWithValue("@difficultyStat", vehicle.VehicleDifficultyStat);
            cmd.Parameters.AddWithValue("@topSpeed", vehicle.VehicleTopSpeed);
            cmd.Parameters.AddWithValue("@accelerationTime", vehicle.VehicleAccelerationTime);
            cmd.Parameters.AddWithValue("@engine", vehicle.VehicleEngineDisplacement);
            cmd.Parameters.AddWithValue("@layout", vehicle.VehicleEnginePosition);
            cmd.Parameters.AddWithValue("@gearbox", vehicle.VehicleGearboxType);
            cmd.Parameters.AddWithValue("@maxPower", vehicle.VehicleMaxPower);
            cmd.Parameters.AddWithValue("@maxPowerRPM", vehicle.VehicleMaxPowerRPM);
            cmd.Parameters.AddWithValue("@maxTorque", vehicle.VehicleMaxTorque);
            cmd.Parameters.AddWithValue("@maxTorqueRPM", vehicle.VehicleMaxTorqueRPM);
            cmd.Parameters.AddWithValue("@weight", vehicle.VehicleWeight);
            cmd.Parameters.AddWithValue("@image", image ?? (object)DBNull.Value);
        }


        private void AddVehicleButton_Click(object sender, RoutedEventArgs e)
        {
            double mileage;
           
            int price, topSpeed, accelerationStat, speedStat, brakingStat, difficultyStat, maxPower, maxPowerRPM, maxTorque, maxTorqueRPM, weight;
            double accelerationTime;

            List<string> errors = ValidateInputs(out mileage, out price, out topSpeed,
                                                out accelerationStat, out speedStat, out brakingStat,
                                                out difficultyStat, out accelerationTime, out maxPower,
                                                out maxPowerRPM, out maxTorque, out maxTorqueRPM, out weight);

            if (errors.Any())
            {
                // Display or handle errors
                foreach (string error in errors)
                {
                    MessageBox.Show(error);
                }
                return;
            }
            else
            {
                double meter_Metric;
                double meter_Imperial;
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
                if (SI == "Metric")
                {
                    meter_Metric = double.Parse(_odometer_metric.Text);
                    meter_Imperial = Math.Round(meter_Metric * 0.621371, 1);
                }
                else
                {
                    meter_Imperial = double.Parse(_odometer_imperial.Text);
                    meter_Metric = Math.Round(meter_Imperial * 1.60934, 1);
                }
                int races = 0;
                price = Convert.ToInt32(VehiclePriceTextBox.Text);
                bool isActive = VehicleActiveCheckBox.IsChecked.GetValueOrDefault();
                int is_active = 0;
                if (isActive == true)
                {
                    is_active = 1;
                }
                bool isOwned = VehicleOwnedCheckBox.IsChecked.GetValueOrDefault();
                int is_owned = 0;
                if (isOwned == true)
                {
                    is_owned = 1;
                }
                accelerationStat = Convert.ToInt32(AccelerationStatSlider.Value);
                speedStat = Convert.ToInt32(SpeedStatSlider.Value);
                brakingStat = Convert.ToInt32(BrakingStatSlider.Value);
                difficultyStat = Convert.ToInt32(DifficultyStatSlider.Value);
                accelerationTime = Convert.ToDouble(AccelerationTextBox.Text);
                topSpeed = int.Parse(TopSpeedTextBox.Text);
                string engineSize = EngineDisplacementTextBox.Text;
                string engineLayout = EngineLayoutComboBox.Text;
                string gearbox = GearboxComboBox.Text;
                maxTorque = int.Parse(MaxTorqueTextBox.Text);
                maxTorqueRPM = int.Parse(MaxTorqueRPMTextBox.Text);
                maxPower = int.Parse(MaxPowerTextBox.Text);
                maxPowerRPM = int.Parse(MaxPowerRPMTextBox.Text);
                weight = int.Parse(WeightTextBox.Text);
                // Convert the image to a byte array
                byte[] image = ConvertImageToByteArray(UploadedImage.Source as BitmapImage);

                using (var conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    query = @"
                            INSERT INTO vehicles 
                            (Name, Brand, Model, Class, Races_Ran, _odometer_metric, _odometer_imperial, Price, Active, Owned, Acceleration_Stat, Speed_Stat, Braking_Stat, 
                            Difficulty_Stat, Top_Speed, Acceleration_Time, EngineDisplacement, Engine_Layout, 
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
                        cmd.Parameters.AddWithValue("@active", is_active);
                        cmd.Parameters.AddWithValue("@owned", is_owned);
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
                        cmd.Parameters.AddWithValue("@image", image ?? (object)DBNull.Value);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Vehicle Added");
                BindComboBox(VehicleBrandComboBox);
                BindComboBox(VehicleComboBox);
                LoadVehicles();
            }
        }

        private void DeleteVehicleButton_Click(object sender, RoutedEventArgs e)
        {
            if (VehicleComboBox.SelectedItem == null) return;

            var selectedVehicle = (VehicleManagement)VehicleComboBox.SelectedItem;  // Assume you have a Vehicle class

            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = "DELETE FROM Vehicles WHERE Id = @id";
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", selectedVehicle.id);  // Assuming your Vehicle class has an Id property
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

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (VehicleBrandComboBox.Visibility == Visibility.Visible)
            {
                btn_BrandCancel.Visibility = Visibility.Visible;
                VehicleBrandTextBox.Visibility = Visibility.Visible;
                VehicleBrandComboBox.Visibility = Visibility.Collapsed;
                btn_BrandAdd.Visibility = Visibility.Collapsed;
            } else {
                btn_BrandCancel.Visibility = Visibility.Collapsed;
                VehicleBrandTextBox.Visibility = Visibility.Collapsed;
                VehicleBrandComboBox.Visibility = Visibility.Visible;
                btn_BrandAdd.Visibility = Visibility.Visible;
            }
        }
        private void OneDecimalPointTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Define a regular expression pattern to allow only non-negative numbers with one decimal point
            var regex = new Regex(@"^\d*\.?\d{0,1}$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void OneDecimalPointTextBox_DataObject_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(DataFormats.Text))
            {
                var pasteText = e.DataObject.GetData(DataFormats.Text) as string;
                var regex = new Regex(@"^\d*\.?\d{0,1}$");

                if (!regex.IsMatch(pasteText))
                {
                    e.CancelCommand();
                }
            }
        }

        private void OneDecimalPointTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!(sender is TextBox textBox)) return;

            var regex = new Regex(@"^\d*\.?\d{0,1}$");
            var text = textBox.Text;
            if (!regex.IsMatch(text))
            {
                textBox.Text = text.Remove(text.Length - 1);
                textBox.SelectionStart = textBox.Text.Length;
            }
        }

        private void TwoDecimalPointsTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Define a regular expression pattern to allow only non-negative numbers with up to two decimal points
            var regex = new Regex(@"^\d*\.?\d{0,2}$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void TwoDecimalPointsTextBox_DataObject_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(DataFormats.Text))
            {
                var pasteText = e.DataObject.GetData(DataFormats.Text) as string;
                var regex = new Regex(@"^\d*\.?\d{0,2}$");

                if (!regex.IsMatch(pasteText))
                {
                    e.CancelCommand();
                }
            }
        }

        private void TwoDecimalPointsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!(sender is TextBox textBox)) return;

            var regex = new Regex(@"^\d*\.?\d{0,2}$");
            var text = textBox.Text;
            if (!regex.IsMatch(text))
            {
                textBox.Text = text.Remove(text.Length - 1);
                textBox.SelectionStart = textBox.Text.Length;
            }
        }
        private void ResetVehicle_Click(object sender, RoutedEventArgs e)
        {
            ResetControls(this);
        }

        private void VehicleManagement_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
        private void Minimize_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Window.GetWindow(this).WindowState = WindowState.Minimized;
        }

        private void Close_Button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }



        private void PopulateHouseComboBox(string islandName)
        {
            // Clear the existing items
            HouseComboBox.Items.Clear();

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // Define the SQL query based on the island name
                string query = "SELECT * FROM Houses WHERE IslandName = @IslandName AND OWNED = '1'";

                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@IslandName", islandName);

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string houseName = reader["Name"].ToString();

                            HouseComboBox.Items.Add(new ComboBoxItem
                            {
                                Value = houseName
                            });
                        }
                    }
                }
            }
        }

        private void VehicleOwnedCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (VehicleOwnedCheckBox.IsChecked == true)
            {
                GarageGroupBox.Visibility = Visibility.Visible;
            }
            else
            {
                GarageGroupBox.Visibility = Visibility.Collapsed;
            }
        }
        private void VehicleOwnedCheckBoxChange()
        {
            if (VehicleOwnedCheckBox.IsChecked == true)
            {
                GarageGroupBox.Visibility = Visibility.Visible;
            }
            else
            {
                GarageGroupBox.Visibility = Visibility.Collapsed;
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

                // Recurse into child elements
                ResetControls(child);
                AddVehicle.IsEnabled = true;
            }
        }
    }
}