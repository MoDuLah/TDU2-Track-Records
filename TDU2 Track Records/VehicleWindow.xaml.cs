using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using TDU2_Track_Records.Properties;

namespace TDU2_Track_Records
{
    /// <summary>
    /// Interaction logic for Vehicle.xaml
    /// </summary>
    public partial class Vehicle : Window
    {
        readonly string connectionString = Settings.Default.connectionString;
        public string distance, speed;
        string SI = Settings.Default.system;

        public Vehicle()
        {
            InitializeComponent();
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
        private void PopulateVehicleFields(VehicleManagement vehicle)
        {
            CarBrandTextBox.Text = vehicle.Name;
            VehicleClass.Text = vehicle.Class;
            CarMileageTextBox.Text = vehicle.Mileage.ToString();
            CarPriceTextBox.Text = vehicle.Price.ToString();
            CarActiveCheckBox.IsChecked = vehicle.Active;
            AccelerationSlider.Value = vehicle.Acceleration;
            SpeedRatingSlider.Value = vehicle.SpeedRating;
            BrakingRatingSlider.Value = vehicle.BrakingRating;
            DifficultyRatingSlider.Value = vehicle.DifficultyRating;

            // Correctly access the selected unit in ComboBox
            string selectedSpeedUnit = Settings.Default.speed as string;
            string selectedWeightUnit = Settings.Default.weight as string;

            TopSpeedTextBox.Text = ConvertSpeedToSelectedUnit(vehicle.TopSpeed, selectedSpeedUnit).ToString();
            AccelerationTextBox.Text = vehicle.AccelerationValue.ToString();
            PowerTextBox.Text = vehicle.Power.ToString();
            EngineSizeTextBox.Text = vehicle.EngineSize.ToString();
            EngineLayoutTextBox.Text = vehicle.EngineLayout;
            GearboxComboBox.Text = vehicle.Gearbox;
            MaxTorqueTextBox.Text = vehicle.MaxTorque.ToString();
            MaxTorqueRPMTextBox.Text = vehicle.MaxTorqueRPM.ToString();
            MaxPowerTextBox.Text = vehicle.MaxPower.ToString();
            MaxPowerRPMTextBox.Text = vehicle.MaxPowerRPM.ToString();
            WeightTextBox.Text = ConvertWeightToSelectedUnit(vehicle.Weight, selectedWeightUnit).ToString();
        }

        private void ExecuteNonQuery(string query, Action<SQLiteCommand> setParameters)
        {
            using (var conn = new SQLiteConnection(Settings.Default.connectionString))
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
            if (EditVehicleListBox.SelectedItem is VehicleManagement selectedVehicle)
            {
                // Update vehicle details from input fields
                string name;
                if (CarBrandComboBox.Visibility == Visibility.Visible)
                {
                    selectedVehicle.Name = CarBrandComboBox.Text;
                }
                else
                {
                    selectedVehicle.Name = CarBrandTextBox.Text;
                }
                selectedVehicle.Class = VehicleClass.Text;
                selectedVehicle.Mileage = double.Parse(CarMileageTextBox.Text);
                selectedVehicle.Price = double.Parse(CarPriceTextBox.Text);
                selectedVehicle.Active = CarActiveCheckBox.IsChecked.GetValueOrDefault();
                selectedVehicle.Acceleration = AccelerationSlider.Value;
                selectedVehicle.SpeedRating = SpeedRatingSlider.Value;
                selectedVehicle.BrakingRating = BrakingRatingSlider.Value;
                selectedVehicle.DifficultyRating = DifficultyRatingSlider.Value;
                selectedVehicle.TopSpeed = double.Parse(TopSpeedTextBox.Text);
                selectedVehicle.AccelerationValue = double.Parse(AccelerationTextBox.Text);
                selectedVehicle.Power = double.Parse(PowerTextBox.Text);
                selectedVehicle.EngineSize = double.Parse(EngineSizeTextBox.Text);
                selectedVehicle.EngineLayout = EngineLayoutTextBox.Text;
                selectedVehicle.Gearbox = GearboxComboBox.Text;
                selectedVehicle.MaxTorque = double.Parse(MaxTorqueTextBox.Text);
                selectedVehicle.MaxTorqueRPM = double.Parse(MaxTorqueRPMTextBox.Text);
                selectedVehicle.MaxPower = double.Parse(MaxPowerTextBox.Text);
                selectedVehicle.MaxPowerRPM = double.Parse(MaxPowerRPMTextBox.Text);
                selectedVehicle.Weight = double.Parse(WeightTextBox.Text);

                string query = @"
            UPDATE cars SET 
            Name = @name, Class = @class, Mileage = @mileage, Price = @price, Active = @active, 
            Acceleration = @acceleration, SpeedRating = @speedRating, BrakingRating = @brakingRating, 
            DifficultyRating = @difficultyRating, TopSpeed = @topSpeed, AccelerationValue = @accelerationValue, 
            Power = @power, EngineSize = @engineSize, EngineLayout = @engineLayout, Gearbox = @gearbox, 
            MaxTorque = @maxTorque, MaxTorqueRPM = @maxTorqueRPM, MaxPower = @maxPower, MaxPowerRPM = @maxPowerRPM, 
            Weight = @weight WHERE Id = @id";

                ExecuteNonQuery(query, cmd =>
                {
                    cmd.Parameters.AddWithValue("@id", selectedVehicle.Id);
                    cmd.Parameters.AddWithValue("@name", selectedVehicle.Name);
                    cmd.Parameters.AddWithValue("@class", selectedVehicle.Class);
                    cmd.Parameters.AddWithValue("@mileage", selectedVehicle.Mileage);
                    cmd.Parameters.AddWithValue("@price", selectedVehicle.Price);
                    cmd.Parameters.AddWithValue("@active", selectedVehicle.Active);
                    cmd.Parameters.AddWithValue("@acceleration", selectedVehicle.Acceleration);
                    cmd.Parameters.AddWithValue("@speedRating", selectedVehicle.SpeedRating);
                    cmd.Parameters.AddWithValue("@brakingRating", selectedVehicle.BrakingRating);
                    cmd.Parameters.AddWithValue("@difficultyRating", selectedVehicle.DifficultyRating);
                    cmd.Parameters.AddWithValue("@topSpeed", selectedVehicle.TopSpeed);
                    cmd.Parameters.AddWithValue("@accelerationValue", selectedVehicle.AccelerationValue);
                    cmd.Parameters.AddWithValue("@power", selectedVehicle.Power);
                    cmd.Parameters.AddWithValue("@engineSize", selectedVehicle.EngineSize);
                    cmd.Parameters.AddWithValue("@engineLayout", selectedVehicle.EngineLayout);
                    cmd.Parameters.AddWithValue("@gearbox", selectedVehicle.Gearbox);
                    cmd.Parameters.AddWithValue("@maxTorque", selectedVehicle.MaxTorque);
                    cmd.Parameters.AddWithValue("@maxTorqueRPM", selectedVehicle.MaxTorqueRPM);
                    cmd.Parameters.AddWithValue("@maxPower", selectedVehicle.MaxPower);
                    cmd.Parameters.AddWithValue("@maxPowerRPM", selectedVehicle.MaxPowerRPM);
                    cmd.Parameters.AddWithValue("@weight", selectedVehicle.Weight);
                });

                MessageBox.Show("Vehicle details updated");
                LoadVehicles();  // Refresh the list after saving
            }
        }

        private void AddCarButton_Click(object sender, RoutedEventArgs e)
        {
            string name;
            if (CarBrandComboBox.Visibility == Visibility.Visible)
            {
                name = CarBrandComboBox.Text;
            }
            else
            {
                name = CarBrandTextBox.Text;
            }
            string carClass = VehicleClass.Text;
            double mileage = double.Parse(CarMileageTextBox.Text);
            double price = double.Parse(CarPriceTextBox.Text);
            bool isActive = CarActiveCheckBox.IsChecked.GetValueOrDefault();

            double acceleration = AccelerationSlider.Value;
            double speedRating = SpeedRatingSlider.Value;
            double brakingRating = BrakingRatingSlider.Value;
            double difficultyRating = DifficultyRatingSlider.Value;
            double topSpeed = double.Parse(TopSpeedTextBox.Text);
            double accelerationVal = double.Parse(AccelerationTextBox.Text);
            double power = double.Parse(PowerTextBox.Text);
            double engineSize = double.Parse(EngineSizeTextBox.Text);
            string engineLayout = EngineLayoutTextBox.Text;
            string gearbox = GearboxComboBox.Text;
            double maxTorque = double.Parse(MaxTorqueTextBox.Text);
            double maxTorqueRPM = double.Parse(MaxTorqueRPMTextBox.Text);
            double maxPower = double.Parse(MaxPowerTextBox.Text);
            double maxPowerRPM = double.Parse(MaxPowerRPMTextBox.Text);
            double weight = double.Parse(WeightTextBox.Text);

            using (var conn = new SQLiteConnection(Settings.Default.connectionString))
            {
                conn.Open();
                string query = @"
            INSERT INTO cars 
            (Name, Class, Mileage, Price, Active, Acceleration, SpeedRating, BrakingRating, 
            DifficultyRating, TopSpeed, AccelerationValue, Power, EngineSize, EngineLayout, 
            Gearbox, MaxTorque, MaxTorqueRPM, MaxPower, MaxPowerRPM, Weight) 
            VALUES 
            (@name, @class, @mileage, @price, @active, @acceleration, @speedRating, 
            @brakingRating, @difficultyRating, @topSpeed, @accelerationValue, @power, 
            @engineSize, @engineLayout, @gearbox, @maxTorque, @maxTorqueRPM, @maxPower, 
            @maxPowerRPM, @weight)";

                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@class", carClass);
                    cmd.Parameters.AddWithValue("@mileage", mileage);
                    cmd.Parameters.AddWithValue("@price", price);
                    cmd.Parameters.AddWithValue("@active", isActive);
                    cmd.Parameters.AddWithValue("@acceleration", acceleration);
                    cmd.Parameters.AddWithValue("@speedRating", speedRating);
                    cmd.Parameters.AddWithValue("@brakingRating", brakingRating);
                    cmd.Parameters.AddWithValue("@difficultyRating", difficultyRating);
                    cmd.Parameters.AddWithValue("@topSpeed", topSpeed);
                    cmd.Parameters.AddWithValue("@accelerationValue", accelerationVal);
                    cmd.Parameters.AddWithValue("@power", power);
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
        }

        private void DeleteCarButton_Click(object sender, RoutedEventArgs e)
        {
            if (VehicleListBox.SelectedItem == null) return;

            var selectedVehicle = (VehicleManagement)VehicleListBox.SelectedItem;  // Assume you have a Vehicle class

            using (var conn = new SQLiteConnection(Settings.Default.connectionString))
            {
                conn.Open();
                string query = "DELETE FROM cars WHERE Id = @id";
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", selectedVehicle.Id);  // Assuming your Vehicle class has an Id property
                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Vehicle Deleted");
            LoadVehicles();  // Refresh the list after deletion
        }

        private void LoadVehicles()
        {
            using (var conn = new SQLiteConnection(Settings.Default.connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM cars";
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        var vehicles = new List<VehicleManagement>();  // Vehicle is a custom class representing a vehicle entity

                        while (reader.Read())
                        {
                            var vehicle = new VehicleManagement
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Class = reader.GetString(2),
                                Mileage = reader.GetDouble(3),
                                Price = reader.GetDouble(4),
                                Active = reader.GetBoolean(5),
                                Acceleration = reader.GetDouble(6),
                                SpeedRating = reader.GetDouble(7),
                                BrakingRating = reader.GetDouble(8),
                                DifficultyRating = reader.GetDouble(9),
                                TopSpeed = reader.GetDouble(10),
                                AccelerationValue = reader.GetDouble(11),
                                Power = reader.GetDouble(12),
                                EngineSize = reader.GetDouble(13),
                                EngineLayout = reader.GetString(14),
                                Gearbox = reader.GetString(15),
                                MaxTorque = reader.GetDouble(16),
                                MaxTorqueRPM = reader.GetDouble(17),
                                MaxPower = reader.GetDouble(18),
                                MaxPowerRPM = reader.GetDouble(19),
                                Weight = reader.GetDouble(20)
                            };
                            vehicles.Add(vehicle);
                        }

                        VehicleListBox.ItemsSource = vehicles;
                    }
                }
            }
        }

        private void VehicleListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (VehicleListBox.SelectedItem == null) return;

            var selectedVehicle = (Vehicle)VehicleListBox.SelectedItem;

            // Populate the fields with the selected vehicle's details if needed
        }

        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SaveToDbButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void LoadFromDbButton_Click(object sender, RoutedEventArgs e)
        {

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
            if(CarBrandComboBox.Visibility == Visibility.Visible)
            {
                btn_BrandCancel.Visibility = Visibility.Visible;
                CarBrandTextBox.Visibility = Visibility.Visible;
                CarBrandComboBox.Visibility = Visibility.Collapsed;
                btn_BrandAdd.Visibility = Visibility.Collapsed;
            }
            else
            {
                btn_BrandCancel.Visibility = Visibility.Collapsed;
                CarBrandTextBox.Visibility = Visibility.Collapsed;
                CarBrandComboBox.Visibility = Visibility.Visible;
                btn_BrandAdd.Visibility = Visibility.Visible;
            }
        }

        private void EditVehicleListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EditVehicleListBox.SelectedItem == null) return;

            var selectedVehicle = (VehicleManagement)EditVehicleListBox.SelectedItem;

            // Populate the fields with the selected vehicle's details
            if(selectedVehicle == null) return;

            if (CarBrandComboBox.Visibility == Visibility.Visible)
            {
                CarBrandComboBox.Text = selectedVehicle.Name;
            }
            else
            {
                CarBrandTextBox.Text = selectedVehicle.Name;
            }
            VehicleClass.Text = selectedVehicle.Class;
            CarMileageTextBox.Text = selectedVehicle.Mileage.ToString();
            CarPriceTextBox.Text = selectedVehicle.Price.ToString();
            CarActiveCheckBox.IsChecked = selectedVehicle.Active;
            AccelerationSlider.Value = selectedVehicle.Acceleration;
            SpeedRatingSlider.Value = selectedVehicle.SpeedRating;
            BrakingRatingSlider.Value = selectedVehicle.BrakingRating;
            DifficultyRatingSlider.Value = selectedVehicle.DifficultyRating;
            TopSpeedTextBox.Text = selectedVehicle.TopSpeed.ToString();
            AccelerationTextBox.Text = selectedVehicle.AccelerationValue.ToString();
            PowerTextBox.Text = selectedVehicle.Power.ToString();
            EngineSizeTextBox.Text = selectedVehicle.EngineSize.ToString();
            EngineLayoutTextBox.Text = selectedVehicle.EngineLayout;
            GearboxComboBox.Text = selectedVehicle.Gearbox;
            MaxTorqueTextBox.Text = selectedVehicle.MaxTorque.ToString();
            MaxTorqueRPMTextBox.Text = selectedVehicle.MaxTorqueRPM.ToString();
            MaxPowerTextBox.Text = selectedVehicle.MaxPower.ToString();
            MaxPowerRPMTextBox.Text = selectedVehicle.MaxPowerRPM.ToString();
            WeightTextBox.Text = selectedVehicle.Weight.ToString();
        }

    }
}
