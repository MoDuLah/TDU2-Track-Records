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
        public string EngineDrive { get; set; }
        public string EnginePositionName { get; set; }
        public string EngineTypeName { get; set; }
        public string GearboxName { get; set; }
        public string DriveName { get; set; }
        public string FrontBrakesDesc { get; set; }
        public string RearBrakesDesc { get; set; }
        public string HouseStoredName { get; set; }
        public string VehicleFrontTires { get; set; } 
        public string VehicleRearTires { get; set; }
        public string VehicleEnginePosition { get; set; }
        public int VehiclePrice { get; set; }
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
        public int VehicleTrackBack { get; set; }
        public int VehicleTrackFront { get; set; }
        public int VehicleWheelbase { get; set; }
        public int VehicleNbSeats { get; set; }
        public int VehicleNbDoors { get; set; }
        public int VehicleDefaultRimsDiameterF { get; set; }
        public int VehicleDefaultRimsDiameterR { get; set; }
        public int VehicleDefaultRimsHeightF { get; set; }
        public int VehicleDefaultRimsHeightR { get; set; }
        public int VehicleDefaultRimsWidthF { get; set; }
        public int VehicleDefaultRimsWidthR { get; set; }
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
        public bool VehicleAvailable { get; set; }
        public bool VehicleOwned { get; set; }
        public byte[] VehicleImage { get; set; }
    }
}
