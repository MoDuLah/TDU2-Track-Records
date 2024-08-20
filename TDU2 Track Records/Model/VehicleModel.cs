using System.ComponentModel;

public class VehicleModel : INotifyPropertyChanged, IDataErrorInfo
{
    private string _carBrand;
    private string _carModel;
    private string _vehicleClass;
    private string _odometerMetric;
    private string _carPrice;
    private string _engineSize;
    private string _maxTorque;
    private string _maxPower;
    private string _topSpeed;
    private string _acceleration;
    private string _weight;

    public string CarBrand
    {
        get => _carBrand;
        set { _carBrand = value; OnPropertyChanged(nameof(CarBrand)); }
    }

    public string CarModel
    {
        get => _carModel;
        set { _carModel = value; OnPropertyChanged(nameof(CarModel)); }
    }

    public string VehicleClass
    {
        get => _vehicleClass;
        set { _vehicleClass = value; OnPropertyChanged(nameof(VehicleClass)); }
    }

    public string OdometerMetric
    {
        get => _odometerMetric;
        set { _odometerMetric = value; OnPropertyChanged(nameof(OdometerMetric)); }
    }

    public string CarPrice
    {
        get => _carPrice;
        set { _carPrice = value; OnPropertyChanged(nameof(CarPrice)); }
    }

    public string EngineSize
    {
        get => _engineSize;
        set { _engineSize = value; OnPropertyChanged(nameof(EngineSize)); }
    }

    public string MaxTorque
    {
        get => _maxTorque;
        set { _maxTorque = value; OnPropertyChanged(nameof(MaxTorque)); }
    }

    public string MaxPower
    {
        get => _maxPower;
        set { _maxPower = value; OnPropertyChanged(nameof(MaxPower)); }
    }

    public string TopSpeed
    {
        get => _topSpeed;
        set { _topSpeed = value; OnPropertyChanged(nameof(TopSpeed)); }
    }

    public string Acceleration
    {
        get => _acceleration;
        set { _acceleration = value; OnPropertyChanged(nameof(Acceleration)); }
    }

    public string Weight
    {
        get => _weight;
        set { _weight = value; OnPropertyChanged(nameof(Weight)); }
    }

    public string Error => null;

    public string this[string columnName]
    {
        get
        {
            string result = null;

            switch (columnName)
            {
                case nameof(CarBrand):
                    if (string.IsNullOrWhiteSpace(CarBrand))
                        result = "Brand is required";
                    break;
                case nameof(CarModel):
                    if (string.IsNullOrWhiteSpace(CarModel))
                        result = "Model is required";
                    break;
                case nameof(VehicleClass):
                    if (string.IsNullOrWhiteSpace(VehicleClass))
                        result = "Vehicle class is required";
                    break;
                case nameof(OdometerMetric):
                    if (string.IsNullOrWhiteSpace(OdometerMetric))
                        result = "Odometer is required";
                    else if (!double.TryParse(OdometerMetric, out _))
                        result = "Odometer must be a number";
                    break;
                case nameof(CarPrice):
                    if (string.IsNullOrWhiteSpace(CarPrice))
                        result = "Price is required";
                    else if (!decimal.TryParse(CarPrice, out _))
                        result = "Price must be a valid number";
                    break;
                case nameof(EngineSize):
                    if (string.IsNullOrWhiteSpace(EngineSize))
                        result = "Engine size is required";
                    break;
                case nameof(MaxTorque):
                    if (string.IsNullOrWhiteSpace(MaxTorque))
                        result = "Max torque is required";
                    break;
                case nameof(MaxPower):
                    if (string.IsNullOrWhiteSpace(MaxPower))
                        result = "Max power is required";
                    break;
                case nameof(TopSpeed):
                    if (string.IsNullOrWhiteSpace(TopSpeed))
                        result = "Top speed is required";
                    break;
                case nameof(Acceleration):
                    if (string.IsNullOrWhiteSpace(Acceleration))
                        result = "Acceleration is required";
                    break;
                case nameof(Weight):
                    if (string.IsNullOrWhiteSpace(Weight))
                        result = "Weight is required";
                    break;
            }

            return result;
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
