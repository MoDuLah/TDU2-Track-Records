namespace TDU2_Track_Records.Classes
{
    public class DVLA
    {
        // Properties
        public int Id { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string Class { get; set; }
        public int RacesRan { get; set; }
        public double OdometerMetric { get; set; }
        public double OdometerImperial { get; set; }
        public bool Active { get; set; }
        public bool Owned { get; set; }
        public string Price { get; set; }
        public int AccelerationStat { get; set; }
        public int SpeedStat { get; set; }
        public int BrakingStat { get; set; }
        public int DifficultyStat { get; set; }
        public int TopSpeed { get; set; }
        public double AccelerationTime { get; set; }
        public string Engine { get; set; }
        public string EngineLayout { get; set; }
        public string EnginePlacement { get; set; }
        public string Gearbox { get; set; }
        public string GearboxType { get; set; }
        public string GearboxMTAT { get; set; }
        public int MaxTorque { get; set; }
        public int MaxTorqueRPM { get; set; }
        public int MaxPower { get; set; }
        public int MaxPowerRPM { get; set; }
        public int Weight { get; set; }
        public double PowerWeightRatio { get; set; }
        public byte[] Image { get; set; } // For BLOB data
        public string Tag { get; set; }
        public string RegionTag { get; set; }
        public string InGarage { get; set; }
        public int DealershipIdIbiza { get; set; }
        public int DealershipIdHawaii { get; set; }
        public bool CanVehiclePaint { get; set; }
        public bool CanVehicleSticker { get; set; }
        public bool CanVehicleUpgrade { get; set; }
        public string FrontTyres { get; set; }
        public string RearTyres { get; set; }
        public string WheelDrive { get; set; }
        public string EnginePosition { get; set; }
        public string FrontBrakes { get; set; }
        public string RearBrakes { get; set; }
    }
}
