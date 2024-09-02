using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SQLite;
using TDU2_Track_Records.Classes;
using TDU2_Track_Records.Properties;

namespace TDU2_Track_Records.ViewModels
{
    internal class vehicleCardViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<DVLA> Vehicles { get; set; }
        private readonly string connectionString;

        public vehicleCardViewModel()
        {
            connectionString = Settings.Default.connectionString;
            Vehicles = new ObservableCollection<DVLA>();
            // Example: Load a vehicle with a specific ID
            LoadVehicleById(192);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void LoadVehicleById(int vehicleId)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT * FROM vehicles WHERE id = @id";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", vehicleId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var vehicle = new DVLA
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("id")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Brand = reader.GetString(reader.GetOrdinal("Brand")),
                                Model = reader.GetString(reader.GetOrdinal("Model")),
                                Class = reader.GetString(reader.GetOrdinal("Class")),
                                RacesRan = reader.GetString(reader.GetOrdinal("Races_Ran")),
                                OdometerMetric = reader.GetString(reader.GetOrdinal("Odometer_Metric")),
                                OdometerImperial = reader.GetString(reader.GetOrdinal("Odometer_Imperial")),
                                Active = reader.GetString(reader.GetOrdinal("Active")) == "1" ? "Yes" : "No",
                                Owned = reader.GetString(reader.GetOrdinal("Owned")) == "1" ? "Yes" : "No",
                                Price = reader.GetString(reader.GetOrdinal("Price")),
                                AccelerationStat = reader.GetString(reader.GetOrdinal("Acceleration_Stat")),
                                SpeedStat = reader.GetString(reader.GetOrdinal("Speed_Stat")),
                                BrakingStat = reader.GetString(reader.GetOrdinal("Braking_Stat")),
                                DifficultyStat = reader.GetString(reader.GetOrdinal("Difficulty_Stat")),
                                TopSpeed = reader.GetString(reader.GetOrdinal("Top_Speed")),
                                AccelerationTime = reader.GetString(reader.GetOrdinal("Acceleration_Time")),
                                Engine = reader.IsDBNull(reader.GetOrdinal("Engine")) ? null : reader.GetString(reader.GetOrdinal("Engine")),
                                EngineLayout = reader.IsDBNull(reader.GetOrdinal("Engine_Layout")) ? null : reader.GetString(reader.GetOrdinal("Engine_Layout")),
                                EnginePlacement = reader.IsDBNull(reader.GetOrdinal("EnginePlacement")) ? null : reader.GetString(reader.GetOrdinal("EnginePlacement")),
                                Gearbox = reader.IsDBNull(reader.GetOrdinal("Gearbox")) ? null : reader.GetString(reader.GetOrdinal("Gearbox")),
                                GearboxType = reader.IsDBNull(reader.GetOrdinal("GearboxType")) ? null : reader.GetString(reader.GetOrdinal("GearboxType")),
                                GearboxMTAT = reader.IsDBNull(reader.GetOrdinal("GearboxMTAT")) ? null : reader.GetString(reader.GetOrdinal("GearboxMTAT")),
                                MaxTorque = reader.IsDBNull(reader.GetOrdinal("Max_Torque")) ? null : reader.GetString(reader.GetOrdinal("Max_Torque")),
                                MaxTorqueRPM = reader.IsDBNull(reader.GetOrdinal("Max_TorqueRPM")) ? null : reader.GetString(reader.GetOrdinal("Max_TorqueRPM")),
                                MaxPower = reader.IsDBNull(reader.GetOrdinal("Max_Power")) ? null : reader.GetString(reader.GetOrdinal("Max_Power")),
                                MaxPowerRPM = reader.IsDBNull(reader.GetOrdinal("Max_PowerRPM")) ? null : reader.GetString(reader.GetOrdinal("Max_PowerRPM")),
                                Weight = reader.IsDBNull(reader.GetOrdinal("Weight")) ? null : reader.GetString(reader.GetOrdinal("Weight")),
                                PowerWeightRatio = reader.IsDBNull(reader.GetOrdinal("PowerWeightRatio")) ? null : reader.GetString(reader.GetOrdinal("PowerWeightRatio")),
                                Image = reader["Image"] as byte[], // Handle BLOB data
                                Tag = reader.IsDBNull(reader.GetOrdinal("Tag")) ? null : reader.GetString(reader.GetOrdinal("Tag")),
                                RegionTag = reader.IsDBNull(reader.GetOrdinal("RegionTag")) ? null : reader.GetString(reader.GetOrdinal("RegionTag")),
                                InGarage = reader.GetString(reader.GetOrdinal("InGarage")) == "1" ? "Yes" : "No",
                                DealershipIdIbiza = reader.IsDBNull(reader.GetOrdinal("DealershipIdIbiza")) ? null : reader.GetString(reader.GetOrdinal("DealershipIdIbiza")),
                                DealershipIdHawaii = reader.IsDBNull(reader.GetOrdinal("DealershipIdHawaii")) ? null : reader.GetString(reader.GetOrdinal("DealershipIdHawaii")),
                                CanVehiclePaint = reader.GetString(reader.GetOrdinal("CanVehiclePaint")) == "1" ? "Yes" : "No",
                                CanVehicleSticker = reader.GetString(reader.GetOrdinal("CanVehicleSticker")) == "1" ? "Yes" : "No",
                                CanVehicleUpgrade = reader.GetString(reader.GetOrdinal("CanVehicleUpgrade")) == "1" ? "Yes" : "No",
                                FrontTyres = reader.IsDBNull(reader.GetOrdinal("FrontTyres")) ? null : reader.GetString(reader.GetOrdinal("FrontTyres")),
                                RearTyres = reader.IsDBNull(reader.GetOrdinal("RearTyres")) ? null : reader.GetString(reader.GetOrdinal("RearTyres")),
                                WheelDrive = reader.IsDBNull(reader.GetOrdinal("WheelDrive")) ? null : reader.GetString(reader.GetOrdinal("WheelDrive")),
                                EnginePosition = reader.IsDBNull(reader.GetOrdinal("EnginePosition")) ? null : reader.GetString(reader.GetOrdinal("EnginePosition")),
                                FrontBrakes = reader.IsDBNull(reader.GetOrdinal("FrontBrakes")) ? null : reader.GetString(reader.GetOrdinal("FrontBrakes")),
                                RearBrakes = reader.IsDBNull(reader.GetOrdinal("RearBrakes")) ? null : reader.GetString(reader.GetOrdinal("RearBrakes")),
                            };

                            Vehicles.Clear();
                            Vehicles.Add(vehicle);
                        }
                        else
                        {
                            // Handle case where no vehicle was found for the given ID
                            Console.WriteLine("Vehicle not found.");
                        }
                    }
                }
            }
        }


    }
}
