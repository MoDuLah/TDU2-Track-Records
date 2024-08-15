using System.ComponentModel;

namespace TDU2_Track_Records.Models
{
    public class Lap : INotifyPropertyChanged
    {
        private string _carName;
        private string _carClass;
        private string _lapTime;
        private string _weatherConditions;
        private string _orientation;
        private string _weatherImageSource;
        private bool _isWeatherImageVisible;
        private string _orientationImageSource;
        private bool _isOrientationImageVisible;
        private string _measurementUnit;
 
        public string MeasurementUnit
        {
            get { return _measurementUnit; }
            set
            {
                if (_measurementUnit != value)
                {
                    _measurementUnit = value;
                    OnPropertyChanged(nameof(MeasurementUnit));
                }
            }
        }


        public string WeatherImageSource
        {
            get => _weatherImageSource;
            set
            {
                _weatherImageSource = value;
                OnPropertyChanged(nameof(WeatherImageSource));
            }
        }

        public bool IsWeatherImageVisible
        {
            get => _isWeatherImageVisible;
            set
            {
                _isWeatherImageVisible = value;
                OnPropertyChanged(nameof(IsWeatherImageVisible));
            }
        }

        public string OrientationImageSource
        {
            get => _orientationImageSource;
            set
            {
                _orientationImageSource = value;
                OnPropertyChanged(nameof(OrientationImageSource));
            }
        }

        public bool IsOrientationImageVisible
        {
            get => _isOrientationImageVisible;
            set
            {
                _isOrientationImageVisible = value;
                OnPropertyChanged(nameof(IsOrientationImageVisible));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string carName
        {
            get => _carName;
            set
            {
                _carName = value;
                OnPropertyChanged(nameof(carName));
            }
        }

        public string carClass
        {
            get => _carClass;
            set
            {
                _carClass = value;
                OnPropertyChanged(nameof(carClass));
            }
        }

        public string LapTime
        {
            get => _lapTime;
            set
            {
                _lapTime = value;
                OnPropertyChanged(nameof(LapTime));
            }
        }

        public string WeatherConditions
        {
            get => _weatherConditions;
            set
            {
                _weatherConditions = value;
                OnPropertyChanged(nameof(WeatherConditions));
            }
        }

        public string Orientation
        {
            get => _orientation;
            set
            {
                _orientation = value;
                OnPropertyChanged(nameof(Orientation));
            }
        }

    }
}
