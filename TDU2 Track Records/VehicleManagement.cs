namespace TDU2_Track_Records
{
public class VehicleManagement
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Brand { get; set; }
    public string Model { get; set; }
    public string Class { get; set; }
    public int RacesRan { get; set; }
    public double Mileage { get; set; }
    public double OdometerMetric { get; set; }
    public double OdometerImperial { get; set; }
    public int Active { get; set; }
    public int Owned { get; set; }
    public int Price { get; set; }
    public int AccelerationRating { get; set; }
    public int SpeedRating { get; set; }
    public int BrakingRating { get; set; }
    public int DifficultyRating { get; set; }
    public int AccelerationStat { get; set; }
    public int SpeedStat { get; set; }
    public int BrakingStat { get; set; }
    public int DifficultyStat { get; set; }
    public double AccelerationTime { get; set; }
    public int TopSpeed { get; set; }
    public double Acceleration { get; set; }
    public string Engine { get; set; }
    public string EngineLayout { get; set; }
    public string Gearbox { get; set; }
    public int MaxTorque { get; set; }
    public int MaxTorqueRPM { get; set; }
    public int MaxPower { get; set; }
    public int MaxPowerRPM { get; set; }
    public int Weight { get; set; }
    public byte[] Image { get; set; } // Add this property to hold the image data

    }
}