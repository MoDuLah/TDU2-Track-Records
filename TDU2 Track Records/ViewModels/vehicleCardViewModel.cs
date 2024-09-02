using System.Collections.ObjectModel;
using TDU2_Track_Records.Classes;

namespace TDU2_Track_Records.ViewModels
{
    internal class vehicleCardViewModel
    {
        public ObservableCollection<DVLA> Vehicles { get; set; }

        public vehicleCardViewModel()
        {
            // Initialize with dummy data
            Vehicles = new ObservableCollection<DVLA>
            {
                new DVLA
                {
                    Id = 1,
                    Name = "Lamborghini Aventador",
                    Brand = "Lamborghini",
                    Model = "Aventador LP 780-4 Ultimae",
                    Class = "S",
                    RacesRan = 20,
                    OdometerMetric = 15000.3,
                    OdometerImperial = 9320.7,
                    Active = true,
                    Owned = true,
                    Price = "$400,000",
                    AccelerationStat = 92,
                    SpeedStat = 98,
                    BrakingStat = 89,
                    DifficultyStat = 78,
                    TopSpeed = 355,
                    AccelerationTime = 2.8,
                    Engine = "6.5L V12",
                    EngineLayout = "Mid-engine",
                    EnginePlacement = "Rear",
                    Gearbox = "7-speed",
                    GearboxType = "ISR",
                    GearboxMTAT = "Automatic",
                    MaxTorque = 720,
                    MaxTorqueRPM = 5500,
                    MaxPower = 780,
                    MaxPowerRPM = 8500,
                    Weight = 1550,
                    PowerWeightRatio = 0.50,
                    Image = null,
                    Tag = "Supercar",
                    RegionTag = "EU",
                    InGarage = "Yes",
                    DealershipIdIbiza = 2,
                    DealershipIdHawaii = 4,
                    CanVehiclePaint = true,
                    CanVehicleSticker = true,
                    CanVehicleUpgrade = true,
                    FrontTyres = "255/30 ZR20",
                    RearTyres = "355/25 ZR21",
                    WheelDrive = "AWD",
                    EnginePosition = "Rear",
                    FrontBrakes = "Carbon-ceramic",
                    RearBrakes = "Carbon-ceramic"
                }
            };
        }
    }
}
