namespace TDU2_Track_Records
{
    internal class VehicleManagement
    {
        public int Id { get; set; } // Unique identifier for each vehicle
        public string Name { get; set; } // Vehicle name
        public string Class { get; set; } // Vehicle class
        public double Mileage { get; set; } // Vehicle mileage
        public double Price { get; set; } // Vehicle price
        public bool Active { get; set; } // Whether the vehicle is active
        public double Acceleration { get; set; } // Acceleration rating
        public double SpeedRating { get; set; } // Speed rating
        public double BrakingRating { get; set; } // Braking rating
        public double DifficultyRating { get; set; } // Difficulty rating
        public double TopSpeed { get; set; } // Top speed
        public double AccelerationValue { get; set; } // Acceleration value
        public double Power { get; set; } // Power
        public double EngineSize { get; set; } // Engine size
        public string EngineLayout { get; set; } // Engine layout
        public string Gearbox { get; set; } // Gearbox type
        public double MaxTorque { get; set; } // Maximum torque
        public double MaxTorqueRPM { get; set; } // RPM at maximum torque
        public double MaxPower { get; set; } // Maximum power
        public double MaxPowerRPM { get; set; } // RPM at maximum power
        public double Weight { get; set; } // Vehicle weight
    }
}