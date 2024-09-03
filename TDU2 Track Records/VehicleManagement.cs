//namespace TDU2_Track_Records
//{
//public class VehicleManagement
//{
//    public int Id { get; set; }
//    public string Name { get; set; }
//    public string Brand { get; set; }
//    public string Model { get; set; }
//    public string Class { get; set; }
//    public int RacesRan { get; set; }
//    public double Mileage { get; set; }
//    public double OdometerMetric { get; set; }
//    public double OdometerImperial { get; set; }
//    public int Active { get; set; }
//    public int Owned { get; set; }
//    public string Price { get; set; }
//    public int AccelerationStat { get; set; }
//    public int SpeedStat { get; set; }
//    public int BrakingStat { get; set; }
//    public int DifficultyStat { get; set; }
//    public double AccelerationTime { get; set; }
//    public int TopSpeed { get; set; }
//    public string Engine { get; set; }
//    public string EngineLayout { get; set; }
//    public string Gearbox { get; set; }
//    public int MaxTorque { get; set; }
//    public int MaxTorqueRPM { get; set; }
//    public int MaxPower { get; set; }
//    public int MaxPowerRPM { get; set; }
//    public int Weight { get; set; }
//    public byte[] Image { get; set; } // Add this property to hold the image data

//    }
//}
namespace TDU2_Track_Records
{
    public class VehicleManagement
    {
        public int Id { get; set; }
        public int UpgradeLevel { get; set; }
        public int RacesRan { get; set; }
        public int Active { get; set; }
        public int Owned { get; set; }
        public int AccelerationStat { get; set; }
        public int SpeedStat { get; set; }
        public int BrakingStat { get; set; }
        public int DifficultyStat { get; set; }
        public int TopSpeed { get; set; }
        public int MaxTorque { get; set; }
        public int MaxTorqueRPM { get; set; }
        public int MaxPower { get; set; }
        public int MaxPowerRPM { get; set; }
        public int Weight { get; set; }
        public int InGarage { get; set; } // New property added (consider using int or string depending on your use case)
        public int CanVehiclePaint { get; set; } // New property added (consider using int or string)
        public int CanVehicleSticker { get; set; } // New property added (consider using int or string)
        public int CanVehicleUpgrade { get; set; } // New property added (consider using int or string)
        public string Name { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string Class { get; set; }
        public string Price { get; set; }
        public string Engine { get; set; }
        public string EngineLayout { get; set; }
        public string EnginePosition { get; set; } // New property added
        public string Gearbox { get; set; }
        public string GearboxType { get; set; } // New property added
        public string GearboxMTAT { get; set; } // New property added
        public string Tag { get; set; } // New property added
        public string RegionTag { get; set; } // New property added
        public string DealershipIdIbiza { get; set; } // New property added
        public string DealershipIdHawaii { get; set; } // New property added
        public string FrontTyres { get; set; } // New property added
        public string RearTyres { get; set; } // New property added
        public string WheelDrive { get; set; } // New property added
        public string FrontBrakes { get; set; } // New property added
        public string RearBrakes { get; set; } // New property added
        public double AccelerationTime { get; set; }
        public double Mileage { get; set; }
        public double PowerWeightRatio { get; set; } // New property added
        public double OdometerMetric { get; set; }
        public double OdometerImperial { get; set; }
        public byte[] Image { get; set; } // Already present in both classes
    }
}
