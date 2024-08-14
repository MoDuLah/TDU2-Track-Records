using System.Collections.ObjectModel;
using System.ComponentModel;

namespace TDU2_Track_Records.ViewModels
{
    public class PowerLapViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Lap> _laps;

        public ObservableCollection<Lap> Laps
        {
            get => _laps;
            set
            {
                _laps = value;
                OnPropertyChanged(nameof(Laps));
            }
        }

        public PowerLapViewModel()
        {
            Laps = new ObservableCollection<Lap>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}