using System;

namespace TDU2_Track_Records
{
    public class VehicleManagement
    {
        public int id { get; set; }
        public string VehicleName { get; set; }
        public string VehicleBrand { get; set; }
        public string VehicleModel { get; set; }
        public string VehicleTags { get; set; }
        public string VehicleCategory { get; set; }
        public string Braking100To0 { get; set; }
        public string QuarterMile {  get; set; }
        public string VehicleLevel { get; set; }
        public string EngineTypeName { get; set; }
        public string GearboxName { get; set; }
        public string DriveName { get; set; }
        public string HouseStoredNameIbiza { get; set; }
        public string HouseStoredNameHawaii { get; set; }
        public string VehicleFrontTires { get; set; } 
        public string VehicleRearTires { get; set; }
        public string VehiclePrice { get; set; }
        public string VehicleEngineDisplacement { get; set; }
        public string VehicleGearboxType { get; set; }
        public string VehicleNbGears { get; set; }
        public string VehicleUpgradeLevel { get; set; }
        public string RacesRan { get; set; }
        public string VehicleTopSpeed { get; set; }
        public string VehicleMaxTorque { get; set; }
        public string VehicleMaxTorqueRPM { get; set; }
        public string VehicleMaxPower { get; set; }
        public string VehicleMaxPowerRPM { get; set; }
        public string VehicleWeight { get; set; }
        public double VehicleOdometerMetric { get; set; }
        public double VehicleOdometerImperial { get; set; }
        public double VehicleAccelerationTime { get; set; }
        public double VehiclePowerWeightRatio { get; set; }
        public string Acceleration0To100Kph { get; set; }
        public string Acceleration0To60Mph { get; set; }
        public string MaxTheoreticalSpeed { get; set; }
        public string StatAcc { get; set; }
        public string StatSpeed { get; set; }
        public string StatBrake { get; set; }
        public string Difficulty { get; set; }
        public string NbGears { get; set; }
        public string Displacement { get; set; }
        public string EnginePosition { get; set; }
        public string PowerBhp { get; set; }
        public string PowerRpm { get; set; }
        public string TorqueNm { get; set; }
        public string TorqueRpm { get; set; }
        public string Length { get; set; }
        public string Width { get; set; }
        public string Mass { get; set; }
        public string PowerWeightRatio { get; set; }
        public string BrakesCharacteristicsFront { get; set; }
        public string BrakesCharacteristicsRear { get; set; }
        public bool CanPaint { get; set; }
        public bool CanSticker { get; set; }
        public bool CanUpgrade { get; set; }
        public bool IsActive { get; set; }
        public bool IsPurchasable { get; set; }
        public bool IsReward { get; set; }
        public bool IsOwned { get; set; }
        public string DealershipNameInIbiza { get; set; }
        public string DealershipNameInHawaii { get; set; }
        public double VehicleAccelerationStat { get; set; }
        public double VehicleSpeedStat { get; set; }
        public double VehicleBrakingStat { get; set; }
        public double VehicleDifficultyStat { get; set; }
        public byte[] VehicleImage { get; set; }
    }
}
