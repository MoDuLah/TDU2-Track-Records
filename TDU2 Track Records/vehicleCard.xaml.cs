using System.Data.SQLite;
using System;
using System.Windows;
using System.Windows.Input;
using TDU2_Track_Records.Properties;
using System.IO;
using System.Windows.Media.Imaging;

namespace TDU2_Track_Records
{
    /// <summary>
    /// Interaction logic for vehicleCard.xaml
    /// </summary>
    public partial class vehicleCard : Window
    {
        readonly string connectionString = Settings.Default.connectionString;
        public string distance, speed;
        readonly string SI = Settings.Default.system;
        double conversionSpeedFactor;
        double conversionWeightFactor;
        double conversionTorqueFactor;
        string conversionAccImage;
        bool isCalculated = false;

        bool canUpgrade;

        public vehicleCard(int vehicleId)
        {
            InitializeComponent();
            conversionSpeedFactor = SI == "Imperial" ? 0.621371 : 1.0;
            conversionWeightFactor = SI == "Imperial" ? 2.20462 : 1.0;
            conversionTorqueFactor = SI == "Imperial" ? 0.737562 : 1.0;
            conversionAccImage = SI == "Imperial" ? "M" : "K";

            // Query the database to get all the details of the vehicle using the vehicle ID
            VehicleManagement vehicle = GetVehicleDetails(vehicleId);
            if (vehicle != null)
            {
                this.DataContext = vehicle;
                VehiclerClass.Source = new BitmapImage(new Uri("/Images/carClasses/" + vehicle.VehicleCategory + ".png", UriKind.Relative));
                VehiclerLevel.Source = new BitmapImage(new Uri($"/Images/vehicleCard/Tune{vehicle.VehicleLevel}.png", UriKind.Relative)); //" + vehicle.VehicleLevel + "
                AccTimeImage.Source = new BitmapImage(new Uri($"/Images/vehicleCard/acc{conversionAccImage}ph.png", UriKind.Relative));
                VehiclerLevel.Tag = vehicle.id;
                SetEnginePositionVisibility(vehicle.EnginePosition);
                SetDriveTypeVisibility(vehicle.DriveName);
                SetDifficultyImages(vehicle.Difficulty);
                SetPaintStickerUpgradeVisibility(vehicle.CanPaint,vehicle.CanSticker,vehicle.CanUpgrade);
                UpdateVehicleDetails(vehicle.id);

            }
            else
            {
                MessageBox.Show("No vehicle data found.");
            }
        }
        private VehicleManagement GetVehicleDetails(int vehicleId)
        {
            VehicleManagement vehicle = null;
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM vehicles WHERE Id = @Id";
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", vehicleId);
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            vehicle = new VehicleManagement
                            {
                                id = Convert.ToInt32(reader["id"]),
                                // Header banner
                                VehicleName = reader["_vehicle_name"].ToString(),
                                VehicleImage = reader["_vehicle_image"] != DBNull.Value ? (byte[])reader["_vehicle_image"] : null,
                                VehicleCategory = reader["_vehiclecategory_name"].ToString(),
                                VehicleLevel = reader["_upgrade_level"].ToString(),
                                // Statistics
                                StatAcc = reader["_stat_acc"].ToString(),
                                StatSpeed = reader["_stat_speed"].ToString(),
                                StatBrake = reader["_stat_break"].ToString(),
                                Difficulty = reader["_difficulty"].ToString(),
                                CanPaint = Convert.ToBoolean(reader["_vehicle_can_paint"]),
                                CanSticker = Convert.ToBoolean(reader["_vehicle_can_sticker"]),
                                CanUpgrade = Convert.ToBoolean(reader["_vehicle_can_upgrade"]),
                                // Chassis
                                EnginePosition = reader["_engine_position_name"].ToString(),
                                DriveName = reader["_drive_name"].ToString(),
                                Length = reader["_length"].ToString() + "mm",
                                Width = reader["_width"].ToString() + "mm",
                                VehicleFrontTires = reader["_tires_front"] != DBNull.Value ? reader["_tires_front"].ToString() : "N/A",
                                VehicleRearTires = reader["_tires_rear"] != DBNull.Value ? reader["_tires_rear"].ToString() : "N/A",
                                BrakesCharacteristicsFront = reader["_brakes_characteristics_front"].ToString() + " " + reader["_brakes_dim_front"].ToString() + "mm",
                                BrakesCharacteristicsRear = reader["_brakes_characteristics_rear"].ToString() + " " + reader["_brakes_dim_rear"].ToString() + "mm",
                                // Engine
                                Displacement = reader["_displacement"].ToString(),
                                EngineTypeName = reader["_engine_type_name"].ToString(),
                                TorqueNm = reader["_torque_nm"].ToString(),
                                TorqueRpm = reader["_torque_rpm"].ToString(),
                                PowerBhp = reader["_power_bhp"].ToString(),
                                PowerRpm = reader["_power_rpm"].ToString(),
                                // Performance
                                NbGears = reader["_nb_gears"].ToString() + " Gears",
                                GearboxName = reader["_gearbox_name"].ToString(),
                                MaxTheoreticalSpeed = reader["_max_theorical_speed"].ToString(),
                                Acceleration0To100Kph = reader["_acceleration_0_100_kph"].ToString(),
                                Acceleration0To60Mph = reader["_acceleration_0_60_mph"].ToString(),
                                Braking100To0 = reader["_braking_100_0"].ToString(),
                                // Information
                                Mass = reader["_mass"].ToString(),
                                PowerWeightRatio = reader["_masspower_ratio"].ToString(),
                                DealershipNameInIbiza = reader["_dealership_name_in_ibiza"].ToString(),
                                DealershipNameInHawaii = reader["_dealership_name_in_hawaii"].ToString(),
                                VehiclePrice = reader["_price"].ToString(),
                                IsOwned = Convert.ToBoolean(reader["_is_owned"].ToString()),
                                HouseStoredNameIbiza = reader["_house_name_in_ibiza"].ToString(),
                                HouseStoredNameOahu = reader["_house_name_in_hawaii"].ToString()
                            };
                        }
                    }
                }
            }
            
            if(vehicle.CanUpgrade == false) { canUpgrade = false; } else { canUpgrade = true; }
            return vehicle;
        }
        private void ChangeMeasuringSystem(string speed, string mass, string torque)
        {


        }

        private void SetPaintStickerUpgradeVisibility(bool canPaints, bool canStickers, bool canUpgrades)
        {
            SetVisibility(CanPaint, CantPaint, canPaints);
            SetVisibility(CanSticker, CantSticker, canStickers);
            SetVisibility(CanUpgrade, CantUpgrade, canUpgrades);
        }
        private void SetVisibility(UIElement canElement, UIElement cantElement, bool condition)
        {
            cantElement.Visibility = condition ? Visibility.Hidden : Visibility.Visible;
            canElement.Visibility = condition ? Visibility.Visible : Visibility.Hidden;
        }
        private void SetEnginePositionVisibility(string enginePosition)
        {
            stackpanelFENG.Visibility = enginePosition == "Front" ? Visibility.Visible : Visibility.Hidden;
            stackpanelMENG.Visibility = enginePosition == "Central" ? Visibility.Visible : Visibility.Hidden;
            stackpanelRENG.Visibility = enginePosition == "Rear" ? Visibility.Visible : Visibility.Hidden;
        }
        private void SetDriveTypeVisibility(string driveType)
        {
            stackpanelFWD.Visibility = driveType == "FWD" ? Visibility.Visible : Visibility.Hidden;
            stackpanelAWD.Visibility = driveType == "AWD" ? Visibility.Visible : Visibility.Hidden;
            stackpanelRWD.Visibility = driveType == "RWD" ? Visibility.Visible : Visibility.Hidden;
        }
        private void SetDifficultyImages(string difficulty)
        {
            string[] wheelImages = new string[5];
            string onColor;

            if (difficulty == "1" || difficulty == "2")
            {
                onColor = "green";
            }
            else if (difficulty == "3")
            {
                onColor = "yellow";
            }
            else if (difficulty == "4" || difficulty == "5")
            {
                onColor = "red";
            }
            else
            {
                onColor = "off";
            }

            for (int i = 0; i < 5; i++)
            {
                wheelImages[i] = i < int.Parse(difficulty) ? $"/Images/vehicleCard/wheel_{onColor}.png" : "/Images/vehicleCard/wheel_off.png";
            }

            VehicleDifficulty1.Source = new BitmapImage(new Uri(wheelImages[0], UriKind.Relative));
            VehicleDifficulty2.Source = new BitmapImage(new Uri(wheelImages[1], UriKind.Relative));
            VehicleDifficulty3.Source = new BitmapImage(new Uri(wheelImages[2], UriKind.Relative));
            VehicleDifficulty4.Source = new BitmapImage(new Uri(wheelImages[3], UriKind.Relative));
            VehicleDifficulty5.Source = new BitmapImage(new Uri(wheelImages[4], UriKind.Relative));
        }

        private void Minimize_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Window.GetWindow(this).WindowState = WindowState.Minimized;
        }

        private void Close_Button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }

        private void VehiclerLevel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (canUpgrade == false) { return; }
            int vehicleId = Convert.ToInt32(VehiclerLevel.Tag);

            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                // Get the current upgrade level
                int currentLevel = GetCurrentUpgradeLevel(vehicleId, conn);
                if (currentLevel < 4) // Max level is 4
                {
                    // Increment the upgrade level
                    int newLevel = currentLevel + 1;
                    UpdateVehicleLevel(vehicleId, newLevel, conn);
                    UpdateVehicleDetails(vehicleId); // Refresh the UI with new stats
                }
            }
        }

        private void VehiclerLevel_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (canUpgrade == false) { return; }
            int vehicleId = Convert.ToInt32(VehiclerLevel.Tag);

            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                // Get the current upgrade level
                int currentLevel = GetCurrentUpgradeLevel(vehicleId, conn);
                if (currentLevel > 0) // Min level is 0
                {
                    // Decrement the upgrade level
                    int newLevel = currentLevel - 1;
                    UpdateVehicleLevel(vehicleId, newLevel, conn);
                    UpdateVehicleDetails(vehicleId); // Refresh the UI with new stats
                }
            }
        }
        private int GetCurrentUpgradeLevel(int vehicleId, SQLiteConnection conn)
        {
            string query = "SELECT _upgrade_level FROM vehicles WHERE Id = @Id";
            using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", vehicleId);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
        private void UpdateVehicleLevel(int vehicleId, int newLevel, SQLiteConnection conn)
        {
            string query = "UPDATE vehicles SET _upgrade_level = @newLevel WHERE Id = @Id";
            using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@newLevel", newLevel);
                cmd.Parameters.AddWithValue("@Id", vehicleId);
                cmd.ExecuteNonQuery();
            }
        }

        private void UpdateVehicleDetails(int vehicleId)
        {

            VehicleManagement vehicle = GetVehicleDetails(vehicleId);
            if (vehicle != null)
            {
                // Upgrade level factors for each stat (from 0 to 4)
                double[] accelerationFactors = { 1.0, 0.98, 0.96, 0.94, 0.92 };
                double[] maxSpeedFactors = { 1.0, 1.02, 1.04, 1.06, 1.08 };
                double[] statAccFactors = { 1.0, 1.02, 1.04, 1.06, 1.08 };
                double[] statSpeedFactors = { 1.0, 1.02, 1.04, 1.06, 1.08 };
                double[] statBrakeFactors = { 1.0, 0.98, 0.96, 0.94, 0.92 };
                double[] powerFactors = { 1.0, 1.02, 1.04, 1.06, 1.08 };
                double[] massReductionFactors = { 1.0, 0.99, 0.98, 0.97, 0.96 };
                double[] priceIncreaseFactors = { 1.0, 1.13, 1.27, 1.40, 1.60 };

                try
                {
                    int upgradeLevel = int.Parse(vehicle.VehicleLevel);
                    double basePower = Convert.ToDouble(vehicle.PowerBhp);
                    double baseAccel = SI == "Metric" ? Convert.ToDouble(vehicle.Acceleration0To100Kph) : Convert.ToDouble(vehicle.Acceleration0To60Mph);
                    double baseBraking100To0 = Convert.ToDouble(vehicle.Braking100To0);
                    double baseMaxSpeed = Convert.ToDouble(vehicle.MaxTheoreticalSpeed);
                    double baseWeight = Convert.ToDouble(vehicle.Mass);
                    //double baseStatAcc = Convert.ToDouble(vehicle.StatAcc);
                    //double baseStatSpeed = Convert.ToDouble(vehicle.StatSpeed);
                    //double baseStatBrake = Convert.ToDouble(vehicle.StatBrake);
                    double basePrice = Convert.ToDouble(vehicle.VehiclePrice);
                    double baseTorque = Convert.ToDouble(vehicle.TorqueNm);

                    // Calculate new stats based on upgrade level
                    double finalStatAcc = Math.Floor(100 - (((baseAccel / statAccFactors[upgradeLevel]) - 2.25) / 9.75 * 100));
                    double finalStatSpeed = Math.Floor(((baseMaxSpeed * statSpeedFactors[upgradeLevel]) - 70) / 400 * 100);
                    double finalStatBrake = Math.Floor(100 - (((baseBraking100To0 * statBrakeFactors[upgradeLevel]) - 90) / 80 * 100));
                    double finalAccel = Math.Round(baseAccel * accelerationFactors[upgradeLevel], 2);
                    double finalMaxSpeed = Math.Round((baseMaxSpeed * maxSpeedFactors[upgradeLevel]) * conversionSpeedFactor, 0);
                    double finalWeight = Math.Round((baseWeight * massReductionFactors[upgradeLevel]) * conversionWeightFactor, 0);
                    double finalPower = Math.Round(basePower * powerFactors[upgradeLevel], 0);
                    double finalPrice = Math.Round(basePrice * priceIncreaseFactors[upgradeLevel], 0);
                    double finalTorque = Math.Round(baseTorque * conversionTorqueFactor, 0);
                    double finalMassPower = Math.Round(finalWeight / finalPower, 3);

                    // Update vehicle stats
                    vehicle.Acceleration0To100Kph = finalAccel.ToString();
                    vehicle.TorqueNm = finalTorque.ToString();
                    vehicle.MaxTheoreticalSpeed = finalMaxSpeed.ToString();
                    vehicle.Mass = finalWeight.ToString();
                    vehicle.StatAcc = finalStatAcc.ToString();
                    vehicle.StatSpeed = finalStatSpeed.ToString();
                    vehicle.StatBrake = finalStatBrake.ToString();
                    vehicle.PowerBhp = finalPower.ToString();
                    vehicle.PowerWeightRatio = finalMassPower.ToString();
                    vehicle.VehiclePrice = finalPrice.ToString();

                    // Update the UI
                    this.DataContext = vehicle;
                    VehiclerLevel.Source = new BitmapImage(new Uri($"/Images/vehicleCard/Tune{vehicle.VehicleLevel}.png", UriKind.Relative));
                }
                catch (FormatException ex)
                {
                    // Handle format exception (e.g., log error or show message)
                    MessageBox.Show("Error parsing vehicle details: " + ex.Message);
                }
                catch (Exception ex)
                {
                    // Handle any other exception
                    MessageBox.Show("An unexpected error occurred: " + ex.Message);
                }
            }
        }
        public double ConvertToZeroToSixtyTime(double zeroToHundredKmhTime)
        {
            // Define the constants for the conversion
            const double scaleFactor = 0.9656; // 60 mph / 100 km/h
            const double exponent = 0.8; // Empirical constant for non-linear acceleration

            // Apply the conversion formula
            double zeroToSixtyMphTime = zeroToHundredKmhTime * Math.Pow(scaleFactor, exponent);

            return Math.Round(zeroToSixtyMphTime, 2); // Round to 2 decimal places
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }
}
