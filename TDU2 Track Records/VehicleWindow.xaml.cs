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
        }

        private void BindComboBoxes()
        {
            // Bind VehicleBrandComboBox and VehicleSelection without any filter
            BindComboBox(VehicleBrandComboBox);
            BindComboBox(VehicleComboBox);
            LoadVehicles();
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
                string origin = "vehicles";

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
            SELECT id, _brand_name
            FROM vehicles 
            GROUP BY _brand_name 
            ORDER BY _brand_name ASC;";
            }
            else if (comboBox == VehicleComboBox)
            {
                return "SELECT id, _vehicle_name FROM vehicles ORDER BY _vehicle_name ASC;";
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
            return unit == "mph" ? Math.Round(speed * 0.621371, 0) : speed;  // Convert to mph if selected
        }

        private double ConvertWeightToSelectedUnit(double weight, string unit)
        {
            return unit == "lbs" ? Math.Round(weight * 2.20462, 0) : weight;  // Convert to lbs if selected
        }

        private void LoadVehicles()
        {
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

                            while (reader.Read())
                            {
                                var vehicle = new VehicleManagement
                                {
                                    id = GetInt(reader, columnIndices["id"]),
                                    VehicleName = GetString(reader, columnIndices["_vehicle_name"]),
                                    VehicleBrand = GetString(reader, columnIndices["_brand_name"]),
                                    VehicleModel = GetString(reader, columnIndices["_modelfull_name"]),
                                    VehicleTags = GetString(reader, columnIndices["_vehicle_tags"]),
                                    Braking100To0 = GetDouble(reader, columnIndices["_braking_100_0"]).ToString(),
                                    QuarterMile = reader.IsDBNull(columnIndices["_quartermile_sec"]) ? string.Empty : GetDouble(reader, columnIndices["_quartermile_sec"]).ToString(),

                                    VehicleCategory = GetString(reader, columnIndices["_vehiclecategory_name"]),
                                    VehiclePrice = GetInt(reader, columnIndices["_price"]).ToString(),
                                    VehicleOdometerMetric = GetDouble(reader, columnIndices["_odometer_metric"]),
                                    VehicleOdometerImperial = GetDouble(reader, columnIndices["_odometer_imperial"]),
                                    
                                    IsActive = Convert.ToBoolean(GetString(reader, columnIndices["_is_active"])),
                                    IsOwned = Convert.ToBoolean(GetString(reader, columnIndices["_is_owned"])),
                                    IsPurchasable = Convert.ToBoolean(GetString(reader, columnIndices["_is_purchasable"])),
                                    IsReward = Convert.ToBoolean(GetString(reader, columnIndices["_is_reward"])),
                                    
                                    VehicleAccelerationStat = Convert.ToDouble(GetInt(reader, columnIndices["_stat_acc"])),
                                    VehicleSpeedStat = Convert.ToDouble(GetInt(reader, columnIndices["_stat_speed"])),
                                    VehicleBrakingStat = Convert.ToDouble(GetInt(reader, columnIndices["_stat_break"])),
                                    VehicleDifficultyStat = Convert.ToDouble(GetInt(reader, columnIndices["_difficulty"])),
                                    
                                    VehicleEngineDisplacement = GetInt(reader, columnIndices["_displacement"]).ToString(),
                                    EngineTypeName = GetString(reader, columnIndices["_engine_type_name"]),
                                    DriveName = GetString(reader, columnIndices["_drive_name"]),
                                    EnginePosition = GetString(reader, columnIndices["_engine_position_name"]),
                                    
                                    VehicleMaxPower = GetInt(reader, columnIndices["_power_bhp"]).ToString(),
                                    VehicleMaxTorque = GetInt(reader, columnIndices["_torque_nm"]).ToString(),
                                    VehicleMaxTorqueRPM = GetInt(reader, columnIndices["_torque_rpm"]).ToString(),
                                    VehicleMaxPowerRPM = GetInt(reader, columnIndices["_power_rpm"]).ToString(),

                                    GearboxName = GetString(reader, columnIndices["_gearbox_name"]),
                                    NbGears = GetInt(reader, columnIndices["_nb_gears"]).ToString(),
                                    VehicleTopSpeed = GetInt(reader, columnIndices["_max_theorical_speed"]).ToString(),
                                    Acceleration0To100Kph = GetDouble(reader, columnIndices["_acceleration_0_100_kph"]).ToString(),
                                    VehicleWeight = GetInt(reader, columnIndices["_mass"]).ToString(),

                                    VehicleImage = reader.IsDBNull(columnIndices["_vehicle_image"]) ? null : (byte[])reader["_vehicle_image"],
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

            // Set Vehicle Id
            VehicleIdTextBox.Text = selectedVehicle.id.ToString();

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
            VehicleActiveCheckBox.IsChecked = selectedVehicle.IsActive;
            VehicleOwnedCheckBox.IsChecked = selectedVehicle.IsOwned;
            VehiclePurchasableCheckBox.IsChecked = selectedVehicle.IsPurchasable;
            VehicleRewardCheckBox.IsChecked = selectedVehicle.IsReward;
            VehicleBrake100_0.Text = selectedVehicle.Braking100To0;
            VehicleQuartermile.Text = selectedVehicle.QuarterMile;

            AccelerationStatSlider.Value = selectedVehicle.VehicleAccelerationStat;
            SpeedStatSlider.Value = selectedVehicle.VehicleSpeedStat;
            BrakingStatSlider.Value = selectedVehicle.VehicleBrakingStat;
            DifficultyStatSlider.Value = selectedVehicle.VehicleDifficultyStat;

            EngineDisplacementTextBox.Text = selectedVehicle.VehicleEngineDisplacement;
            EngineTypeComboBox.Text = selectedVehicle.EngineTypeName;
            EngineDriveComboBox.Text = selectedVehicle.DriveName;
            EnginePositionComboBox.Text = selectedVehicle.EnginePosition;

            MaxTorqueTextBox.Text = selectedVehicle.VehicleMaxTorque;
            MaxTorqueRPMTextBox.Text = selectedVehicle.VehicleMaxTorqueRPM;
            MaxPowerTextBox.Text = selectedVehicle.VehicleMaxPower;
            MaxPowerRPMTextBox.Text = selectedVehicle.VehicleMaxPowerRPM;

            GearboxComboBox.Text = selectedVehicle.GearboxName;
            GearsComboBox.Text = selectedVehicle.NbGears;

            TopSpeedTextBox.Text = selectedVehicle.VehicleTopSpeed;
            AccelerationTextBox.Text = selectedVehicle.Acceleration0To100Kph;
            WeightTextBox.Text = selectedVehicle.VehicleWeight;
            VehicleTags.Text = selectedVehicle.VehicleTags;

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
                foreach (string error in errors)
                {
                    MessageBox.Show(error);
                }
                return;
            }

            if (VehicleComboBox.SelectedItem is VehicleManagement selectedVehicle)
            {
                SetVehicleDetails(selectedVehicle, mileage, price, topSpeed, accelerationStat,
                                  speedStat, brakingStat, difficultyStat, accelerationTime,
                                  maxPower, maxPowerRPM, maxTorque, maxTorqueRPM, weight);

                // Convert the image to a byte array
                byte[] imageBytes = ConvertImageToByteArray(UploadedImage.Source as BitmapImage);

                string query = @"
                        UPDATE vehicles SET 
                            _vehicle_name = @name,
                            _brand_name = @brand,
                            _modelfull_name = @model,
                            _vehiclecategory_name = @vehicleCategory,
                            _odometer_metric = @odometerMetric,
                            _odometer_imperial = @odometerImperial,
                            _is_owned = @isOwned,
                            _is_active = @isActive,
                            _is_purchasable = @isPurchasable,
                            _is_reward = @isReward,
                            _price = @price,
                            _stat_acc = @accelerationStat,
                            _stat_speed = @speedStat,
                            _stat_break = @brakingStat,
                            _difficulty = @difficultyStat,
                            _max_theorical_speed = @topSpeed,
                            _acceleration_0_100_kph = @accelerationTime,
                            _displacement = @engine,
                            _engine_position_name = @layout,
                            _gearbox_name = @gearbox,
                            _power_bhp = @maxPower,
                            _power_rpm = @maxPowerRPM,
                            _torque_nm = @maxTorque,
                            _torque_rpm = @maxTorqueRPM,
                            _mass = @weight,
                            _vehicle_image = @image
                        WHERE id = @id";

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
                    ResetControls(this);
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
        private List<string> ValidateInputs(out double mileage, out int price, out int topSpeed,
                                            out int accelerationStat, out int speedStat, out int brakingStat,
                                            out int difficultyStat, out double accelerationTime, out int maxPower,
                                            out int maxPowerRPM, out int maxTorque, out int maxTorqueRPM, out int weight)
        {
            // Initialize out parameters
            mileage = 0;
            price = 0;
            topSpeed = 0;
            accelerationStat = 0;
            speedStat = 0;
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
            bool isMileageValid = SI == "Metric"
                ? double.TryParse(_odometer_metric.Text, out mileage)
                : double.TryParse(_odometer_imperial.Text, out mileage);

            if (!isMileageValid || mileage < 0)
            {
                errorMessages.Add("Mileage must be 0 or greater.");
            }

            // Validate price
            if (string.IsNullOrWhiteSpace(VehiclePriceTextBox.Text) || !int.TryParse(VehiclePriceTextBox.Text, out price) || price < 0)
            {
                errorMessages.Add("Price is required and must be a valid positive number.");
            }

            // Validate top speed
            if (string.IsNullOrWhiteSpace(TopSpeedTextBox.Text) || !int.TryParse(TopSpeedTextBox.Text, out topSpeed) || topSpeed <= 0)
            {
                errorMessages.Add("Invalid or empty top speed.");
            }

            // Validate sliders for stats (acceleration, speed, braking, difficulty)
            if (AccelerationStatSlider.Value < 0 || !int.TryParse(AccelerationStatSlider.Value.ToString(), out accelerationStat))
            {
                errorMessages.Add("Invalid or empty acceleration stat.");
            }

            if (SpeedStatSlider.Value < 0 || !int.TryParse(SpeedStatSlider.Value.ToString(), out speedStat))
            {
                errorMessages.Add("Invalid or empty speed stat.");
            }

            if (BrakingStatSlider.Value < 0 || !int.TryParse(BrakingStatSlider.Value.ToString(), out brakingStat))
            {
                errorMessages.Add("Invalid or empty braking stat.");
            }

            if (DifficultyStatSlider.Value < 0 || !int.TryParse(DifficultyStatSlider.Value.ToString(), out difficultyStat))
            {
                errorMessages.Add("Invalid or empty difficulty stat.");
            }

            // Validate acceleration time
            if (string.IsNullOrWhiteSpace(AccelerationTextBox.Text) || !double.TryParse(AccelerationTextBox.Text, out accelerationTime) || accelerationTime <= 0)
            {
                errorMessages.Add("Invalid or empty acceleration time.");
            }

            // Validate max power
            if (string.IsNullOrWhiteSpace(MaxPowerTextBox.Text) || !int.TryParse(MaxPowerTextBox.Text, out maxPower) || maxPower <= 0)
            {
                errorMessages.Add("Invalid or empty max power.");
            }

            // Validate max power RPM
            if (string.IsNullOrWhiteSpace(MaxPowerRPMTextBox.Text) || !int.TryParse(MaxPowerRPMTextBox.Text, out maxPowerRPM) || maxPowerRPM <= 0)
            {
                errorMessages.Add("Invalid or empty max power RPM.");
            }

            // Validate max torque
            if (string.IsNullOrWhiteSpace(MaxTorqueTextBox.Text) || !int.TryParse(MaxTorqueTextBox.Text, out maxTorque) || maxTorque <= 0)
            {
                errorMessages.Add("Invalid or empty max torque.");
            }

            // Validate max torque RPM
            if (string.IsNullOrWhiteSpace(MaxTorqueRPMTextBox.Text) || !int.TryParse(MaxTorqueRPMTextBox.Text, out maxTorqueRPM) || maxTorqueRPM <= 0)
            {
                errorMessages.Add("Invalid or empty max torque RPM.");
            }

            // Validate weight
            if (string.IsNullOrWhiteSpace(WeightTextBox.Text) || !int.TryParse(WeightTextBox.Text, out weight) || weight <= 0)
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
                vehicle.VehicleOdometerImperial = Math.Round(mileage * 0.621371, 1);
            }
            else
            {
                vehicle.VehicleOdometerImperial = mileage;
                vehicle.VehicleOdometerMetric = Math.Round(mileage * 1.60934, 1);
            }

            vehicle.IsOwned = VehicleOwnedCheckBox.IsChecked == true ? true : false;
            vehicle.IsOwned = VehicleOwnedCheckBox.IsChecked == true ? true : false;
            vehicle.VehiclePrice = price.ToString();
            vehicle.VehicleAccelerationStat = accelerationStat;
            vehicle.VehicleSpeedStat = speedStat;
            vehicle.VehicleBrakingStat = brakingStat;
            vehicle.VehicleDifficultyStat = difficultyStat;
            vehicle.VehicleAccelerationTime = accelerationTime;
            vehicle.VehicleTopSpeed = topSpeed.ToString();
            vehicle.VehicleEngineDisplacement = EngineDisplacementTextBox.Text;
            vehicle.EnginePosition = EnginePositionComboBox.Text;
            vehicle.GearboxName = GearboxComboBox.Text;
            vehicle.VehicleMaxPower = maxPower.ToString();
            vehicle.VehicleMaxPowerRPM = maxPowerRPM.ToString();
            vehicle.VehicleMaxTorque = maxTorque.ToString();
            vehicle.VehicleMaxTorqueRPM = maxTorqueRPM.ToString();
            vehicle.VehicleWeight = weight.ToString();

        }

        private void AddParameters(SQLiteCommand cmd, VehicleManagement vehicle, byte[] image)
        {
            cmd.Parameters.AddWithValue("@id", vehicle.id);
            cmd.Parameters.AddWithValue("@name", vehicle.VehicleName ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@brand", vehicle.VehicleBrand ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@model", vehicle.VehicleModel ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@vehicleCategory", vehicle.VehicleCategory ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@odometerMetric", vehicle.VehicleOdometerMetric);
            cmd.Parameters.AddWithValue("@odometerImperial", vehicle.VehicleOdometerImperial);
            cmd.Parameters.AddWithValue("@isActive", vehicle.IsActive ? "true" : "false");
            cmd.Parameters.AddWithValue("@isOwned", vehicle.IsOwned ? "true" : "false");
            cmd.Parameters.AddWithValue("@isPurchasable", vehicle.IsPurchasable ? "true" : "false");
            cmd.Parameters.AddWithValue("@isReward", vehicle.IsReward ? "true" : "false");
            cmd.Parameters.AddWithValue("@price", vehicle.VehiclePrice);
            cmd.Parameters.AddWithValue("@accelerationStat", vehicle.VehicleAccelerationStat);
            cmd.Parameters.AddWithValue("@speedStat", vehicle.VehicleSpeedStat);
            cmd.Parameters.AddWithValue("@brakingStat", vehicle.VehicleBrakingStat);
            cmd.Parameters.AddWithValue("@difficultyStat", vehicle.VehicleDifficultyStat);
            cmd.Parameters.AddWithValue("@topSpeed", vehicle.VehicleTopSpeed);
            cmd.Parameters.AddWithValue("@accelerationTime", vehicle.VehicleAccelerationTime);
            cmd.Parameters.AddWithValue("@engine", vehicle.VehicleEngineDisplacement ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@layout", vehicle.EnginePosition ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@gearbox", vehicle.GearboxName ?? (object)DBNull.Value);
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
                foreach (string error in errors)
                {
                    MessageBox.Show(error);
                }
                return;
            }

            string brand = VehicleBrandComboBox.Visibility == Visibility.Visible ? VehicleBrandComboBox.Text : VehicleBrandTextBox.Text;
            string model = VehicleModelTextBox.Text;
            string name = $"{brand} {model}";
            string vehicleClass = VehicleClassComboBox.Text;

            double meter_Metric = SI == "Metric" ? double.Parse(_odometer_metric.Text) : Math.Round(double.Parse(_odometer_imperial.Text) * 1.60934, 1);
            double meter_Imperial = SI == "Metric" ? Math.Round(meter_Metric * 0.621371, 1) : double.Parse(_odometer_imperial.Text);

            price = Convert.ToInt32(VehiclePriceTextBox.Text);
            bool isActive = VehicleActiveCheckBox.IsChecked == true;
            bool isOwned = VehicleOwnedCheckBox.IsChecked == true;
            bool isPurchasable = VehiclePurchasableCheckBox.IsChecked == true;
            bool isReward = VehicleRewardCheckBox.IsChecked == true;

            // Convert the image to a byte array
            byte[] image = ConvertImageToByteArray(UploadedImage.Source as BitmapImage);

            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = @"
                        INSERT INTO vehicles 
                        (_vehicle_name, _brand_name, _modelfull_name, _vehiclecategory_name, _odometer_metric, 
                        _odometer_imperial, _price, _is_active, _is_owned, _is_purchasable, _is_reward, 
                        _stat_acc, _stat_speed, _stat_break, _difficulty, _max_theorical_speed, _acceleration_0_100_kph, 
                        _displacement, _engine_position_name, _gearbox_name, _torque_nm, _torque_rpm, _power_bhp, 
                        _power_rpm, _mass, _vehicle_image)
                        VALUES 
                        (@name, @brand, @model, @vehicleCategory, @odometerMetric, @odometerImperial, @price, 
                        @isActive, @isOwned, @isPurchasable, @isReward, @accelerationStat, @speedStat, @brakingStat, 
                        @difficultyStat, @topSpeed, @accelerationTime, @engine, @layout, @gearbox, @maxTorque, 
                        @maxTorqueRPM, @maxPower, @maxPowerRPM, @weight, @image)";

                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@brand", brand);
                    cmd.Parameters.AddWithValue("@model", model);
                    cmd.Parameters.AddWithValue("@vehicleCategory", vehicleClass);
                    cmd.Parameters.AddWithValue("@odometerMetric", meter_Metric);
                    cmd.Parameters.AddWithValue("@odometerImperial", meter_Imperial);
                    cmd.Parameters.AddWithValue("@price", price);
                    cmd.Parameters.AddWithValue("@isActive", isActive);
                    cmd.Parameters.AddWithValue("@isOwned", isOwned);
                    cmd.Parameters.AddWithValue("@isPurchasable", isPurchasable);
                    cmd.Parameters.AddWithValue("@isReward", isReward);
                    cmd.Parameters.AddWithValue("@accelerationStat", accelerationStat);
                    cmd.Parameters.AddWithValue("@speedStat", speedStat);
                    cmd.Parameters.AddWithValue("@brakingStat", brakingStat);
                    cmd.Parameters.AddWithValue("@difficultyStat", difficultyStat);
                    cmd.Parameters.AddWithValue("@topSpeed", topSpeed);
                    cmd.Parameters.AddWithValue("@accelerationTime", accelerationTime);
                    cmd.Parameters.AddWithValue("@engine", EngineDisplacementTextBox.Text);
                    cmd.Parameters.AddWithValue("@layout", EnginePositionComboBox.Text);
                    cmd.Parameters.AddWithValue("@gearbox", GearboxComboBox.Text);
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
            BindComboBoxes(); // Refresh ComboBoxes
            LoadVehicles();  // Refresh the list
            ResetControls(this);
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

        private void btn_BrandAdd_Click(object sender, RoutedEventArgs e)
        {
            if (VehicleBrandComboBox.Visibility == Visibility.Visible)
            {
                VehicleBrandTextBox.Visibility = Visibility.Visible;
                VehicleBrandComboBox.Visibility = Visibility.Collapsed;
            }
            else
            {
                VehicleBrandTextBox.Visibility = Visibility.Collapsed;
                VehicleBrandComboBox.Visibility = Visibility.Visible;
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