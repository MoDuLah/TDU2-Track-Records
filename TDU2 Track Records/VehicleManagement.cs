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
        public string VehicleName { get; set; }
        public string VehicleBrand { get; set; }
        public string VehicleModel { get; set; }
        public string VehicleClass { get; set; }
        public string VehiclePrice { get; set; }
        public string VehicleEngineDisplacement {  get; set; }
        public string VehicleEngineType { get; set; }
        public string VehicleEngineLayout { get; set; }
        public string VehicleEnginePosition { get; set; } 
        public string VehicleGearbox { get; set; }
        public string VehicleGearboxType { get; set; } 
        public string VehicleGearboxMTAT { get; set; } 
        public string VehicleTag { get; set; } 
        public string VehicleRegionTag { get; set; } 
        public string VehicleDealershipIdIbiza { get; set; } 
        public string VehicleDealershipIdHawaii { get; set; } 
        public string VehicleFrontTyres { get; set; } 
        public string VehicleRearTyres { get; set; }
        public string VehicleWheelDrive { get; set; } 
        public string VehicleFrontBrakesDim { get; set; } 
        public string VehicleFrontBrakesDesc { get; set; }
        public string VehicleRearBrakesDim { get; set; }
        public string VehicleRearBrakesDesc { get; set; }
        public string VehicleWidth { get; set; }
        public string VehicleLength { get; set; }
        public string VehicleHeight { get; set; }
        public int VehicleUpgradeLevel { get; set; }
        public int VehicleRacesRan { get; set; }
        public int VehicleActive { get; set; }
        public int VehicleOwned { get; set; }
        public int VehicleAccelerationStat { get; set; }
        public int VehicleSpeedStat { get; set; }
        public int VehicleBrakingStat { get; set; }
        public int VehicleDifficultyStat { get; set; }
        public int VehicleGearNumbers {  get; set; }
        public int VehicleTopSpeed { get; set; }
        public int VehicleMaxTorque { get; set; }
        public int VehicleMaxTorqueRPM { get; set; }
        public int VehicleMaxPower { get; set; }
        public int VehicleMaxPowerRPM { get; set; }
        public int VehicleWeight { get; set; }
        public int VehicleInGarage { get; set; } 
        public int VehicleCanPaint { get; set; } 
        public int VehicleCanSticker { get; set; } 
        public int VehicleCanUpgrade { get; set; } 
        public double VehicleAccelerationTime { get; set; }
        public double VehicleQuarterMileTime { get; set; }
        public double VehiclePowerWeightRatio { get; set; } 
        public double VehicleOdometerMetric { get; set; }
        public double VehicleOdometerImperial { get; set; }
        public byte[] VehicleImage { get; set; } 
    }
}
