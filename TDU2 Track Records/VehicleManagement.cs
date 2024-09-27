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
        public string VehicleLevel { get; set; }
        public string EnginePositionName { get; set; }
        public string EngineTypeName { get; set; }
        public string GearboxName { get; set; }
        public string DriveName { get; set; }
        public string FrontBrakesDesc { get; set; }
        public string RearBrakesDesc { get; set; }
        public string HouseStoredNameIbiza { get; set; }
        public string HouseStoredNameOahu { get; set; }
        public string VehicleFrontTires { get; set; } 
        public string VehicleRearTires { get; set; }
        public string VehicleEnginePosition { get; set; }
        public string VehiclePrice { get; set; }
        public int EngineType { get; set; }
        public int VehicleEngineDisplacement { get; set; }
        public int VehicleGearboxType { get; set; }
        public int VehicleNbGears { get; set; }
        public int VehicleDealershipIdIbiza { get; set; }
        public int VehicleDealershipIdHawaii { get; set; }
        public int VehicleInGarage { get; set; }
        public int VehicleUpgradeLevel { get; set; }
        public int VehicleRacesRan { get; set; }
        public int VehicleAccelerationStat { get; set; }
        public int VehicleSpeedStat { get; set; }
        public int VehicleBrakingStat { get; set; }
        public int VehicleDifficultyStat { get; set; }
        public int VehicleTopSpeed { get; set; }
        public int VehicleMaxTorque { get; set; }
        public int VehicleMaxTorqueRPM { get; set; }
        public int VehicleMaxPower { get; set; }
        public int VehicleMaxPowerRPM { get; set; }
        public int VehicleWeight { get; set; }
        public int VehicleWheelbase { get; set; }

        public int VehicleBrakesDimFront { get; set; }
        public int VehicleBrakesDimRear { get; set; }

        public double VehicleOdometerMetric { get; set; }
        public double VehicleOdometerImperial { get; set; }
        public double VehicleAccelerationTime { get; set; }
        public double VehiclePowerWeightRatio { get; set; }
        public bool VehicleCanPaint { get; set; }
        public bool VehicleCanSticker { get; set; }
        public bool VehicleCanUpgrade { get; set; }
        public bool VehicleActive { get; set; }
        public bool VehiclePurchasable { get; set; }
        public bool VehicleOwned { get; set; }
        public byte[] VehicleImage { get; set; }
        public string Name { get; set; }
        public string CategoryName { get; set; }
        public string Price { get; set; }
        public string Acceleration0To100Kph { get; set; }
        public string Acceleration0To60Mph { get; set; }
        public string MaxTheoreticalSpeed { get; set; }
        public string Braking100To0 { get; set; }
        public string StatAcc { get; set; }
        public string StatSpeed { get; set; }
        public string StatBrake { get; set; }
        public string QuarterMileSec { get; set; }
        public string Difficulty { get; set; }
        public string GearboxId { get; set; }
        public string NbGears { get; set; }
        public string Displacement { get; set; }
        public string EnginePosition { get; set; }
        public string NbTurbos { get; set; }
        public string PowerBhp { get; set; }
        public string PowerRpm { get; set; }
        public string TorqueNm { get; set; }
        public string TorqueRpm { get; set; }
        public string RpmMax { get; set; }
        public string Drive { get; set; }
        public string Height { get; set; }
        public string Length { get; set; }
        public string Width { get; set; }
        public string TrackBack { get; set; }
        public string TrackFront { get; set; }
        public string Wheelbase { get; set; }
        public string Mass { get; set; }
        public string PowerWeightRatio { get; set; }
        public string BrakesCharacteristicsFront { get; set; }
        public string BrakesDimFront { get; set; }
        public string BrakesCharacteristicsRear { get; set; }
        public string BrakesDimRear { get; set; }
        public string OdometerMetric { get; set; }
        public string OdometerImperial { get; set; }
        public string RacesRan { get; set; }
        public bool CanPaint { get; set; }
        public bool CanSticker { get; set; }
        public bool CanUpgrade { get; set; }
        public bool IsActive { get; set; }
        public bool IsPurchasable { get; set; }
        public bool IsReward { get; set; }
        public bool IsOwned { get; set; }
        public string DealershipIdInIbiza { get; set; }
        public string DealershipIdInHawaii { get; set; }
        public string DealershipNameInIbiza { get; set; }
        public string DealershipNameInHawaii { get; set; }
        public string HouseStoredId { get; set; }
        public string HouseStoredSlot { get; set; }

    }

}
