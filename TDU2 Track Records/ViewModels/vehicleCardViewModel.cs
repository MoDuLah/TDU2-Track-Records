//using System;
//using System.Collections.ObjectModel;
//using System.ComponentModel;
//using System.Data.SQLite;
//using System.Windows;
//using TDU2_Track_Records.Classes;
//using TDU2_Track_Records.Properties;

//namespace TDU2_Track_Records.ViewModels
//{
//    internal class SKATACardViewModel
//    {
//        public ObservableCollection<DVLA> SKATA{ get; set; }
//        private readonly string connectionString;

//        public SKATACardViewModel()
//        {
//            connectionString = Settings.Default.connectionString;
//            SKATA = new ObservableCollection<DVLA>();
//            // Example: Load a vehicle with a specific ID
//        }

//        public void LoadSKATAById(int SKATAId)
//        {
//            using (var connection = new SQLiteConnection(connectionString))
//            {
//                connection.Open();

//                string query = "SELECT * FROM vehicles WHERE id = @id";
//                using (var command = new SQLiteCommand(query, connection))
//                {
//                    command.Parameters.AddWithValue("@id", SKATAId);

//                    using (var reader = command.ExecuteReader())
//                    {
//                        if (reader.Read())
//                        {
//                            var vehicle = new DVLA();

//                            // Define an array of column names
//                            string[] columns = {
//    "Name", "Brand", "Model", "Class", "Races_Ran", "Odometer_Metric",
//    "Odometer_Imperial", "Price", "Acceleration_Stat", "Speed_Stat",
//    "Braking_Stat", "Difficulty_Stat", "Top_Speed", "Acceleration_Time",
//    "Engine", "Engine_Layout", "EnginePlacement", "Gearbox",
//    "GearboxType", "GearboxMTAT", "Max_Torque", "Max_TorqueRPM",
//    "Max_Power", "Max_PowerRPM", "Weight", "PowerWeightRatio",
//    "Tag", "RegionTag", "DealershipIdIbiza", "DealershipIdHawaii",
//    "FrontTyres", "RearTyres", "WheelDrive", "EnginePosition",
//    "FrontBrakes", "RearBrakes"
//};

//                            // Use a loop to retrieve the values
//                            foreach (var column in columns)
//                            {
//                                // Retrieve the value as a string or handle DBNull
//                                var value = reader.IsDBNull(reader.GetOrdinal(column)) ? string.Empty : reader.GetString(reader.GetOrdinal(column));

//                                // Set the property on the vehicle object using a switch
//                                switch (column)
//                                {
//                                    case "Name": vehicle.Name = value; break;
//                                    case "Brand": vehicle.Brand = value; break;
//                                    case "Model": vehicle.Model = value; break;
//                                    case "Class": vehicle.Class = value; break;
//                                    case "Races_Ran": vehicle.RacesRan = value; break;
//                                    case "Odometer_Metric": vehicle.OdometerMetric = value; break;
//                                    case "Odometer_Imperial": vehicle.OdometerImperial = value; break;
//                                    case "Price": vehicle.Price = value; break;
//                                    case "Acceleration_Stat": vehicle.AccelerationStat = value; break;
//                                    case "Speed_Stat": vehicle.SpeedStat = value; break;
//                                    case "Braking_Stat": vehicle.BrakingStat = value; break;
//                                    case "Difficulty_Stat": vehicle.DifficultyStat = value; break;
//                                    case "Top_Speed": vehicle.TopSpeed = value; break;
//                                    case "Acceleration_Time": vehicle.AccelerationTime = value; break;
//                                    case "Engine": vehicle.Engine = value; break;
//                                    case "Engine_Layout": vehicle.EngineLayout = value; break;
//                                    case "EnginePlacement": vehicle.EnginePlacement = value; break;
//                                    case "Gearbox": vehicle.Gearbox = value; break;
//                                    case "GearboxType": vehicle.GearboxType = value; break;
//                                    case "GearboxMTAT": vehicle.GearboxMTAT = value; break;
//                                    case "Max_Torque": vehicle.MaxTorque = value; break;
//                                    case "Max_TorqueRPM": vehicle.MaxTorqueRPM = value; break;
//                                    case "Max_Power": vehicle.MaxPower = value; break;
//                                    case "Max_PowerRPM": vehicle.MaxPowerRPM = value; break;
//                                    case "Weight": vehicle.Weight = value; break;
//                                    case "PowerWeightRatio": vehicle.PowerWeightRatio = value; break;
//                                    case "Tag": vehicle.Tag = value; break;
//                                    case "RegionTag": vehicle.RegionTag = value; break;
//                                    case "DealershipIdIbiza": vehicle.DealershipIdIbiza = value; break;
//                                    case "DealershipIdHawaii": vehicle.DealershipIdHawaii = value; break;
//                                    case "FrontTyres": vehicle.FrontTyres = value; break;
//                                    case "RearTyres": vehicle.RearTyres = value; break;
//                                    case "WheelDrive": vehicle.WheelDrive = value; break;
//                                    case "EnginePosition": vehicle.EnginePosition = value; break;
//                                    case "FrontBrakes": vehicle.FrontBrakes = value; break;
//                                    case "RearBrakes": vehicle.RearBrakes = value; break;
//                                }
//                            }

//                            // Handle special cases outside of the loop
//                            vehicle.Id = reader.GetInt32(reader.GetOrdinal("id"));
//                            vehicle.Active = reader.GetString(reader.GetOrdinal("Active")) == "1" ? "Yes" : "No";
//                            vehicle.Owned = reader.GetString(reader.GetOrdinal("Owned")) == "1" ? "Yes" : "No";
//                            vehicle.InGarage = reader.GetString(reader.GetOrdinal("InGarage")) == "1" ? "Yes" : "No";
//                            vehicle.CanVehiclePaint = reader.GetString(reader.GetOrdinal("CanVehiclePaint")) == "1" ? "Yes" : "No";
//                            vehicle.CanVehicleSticker = reader.GetString(reader.GetOrdinal("CanVehicleSticker")) == "1" ? "Yes" : "No";
//                            vehicle.CanVehicleUpgrade = reader.GetString(reader.GetOrdinal("CanVehicleUpgrade")) == "1" ? "Yes" : "No";

//                            // Handle BLOB data for Image
//                            vehicle.Image = reader["Image"] as byte[];

//                            SKATA.Clear();
//                            SKATA.Add(vehicle);

//                        }
//                        else
//                        {
//                            // Handle case where no vehicle was found for the given ID
//                            MessageBox.Show("Vehicle not found.");
//                        }
//                    }
//                }
//            }
//        }
//    }
//}
