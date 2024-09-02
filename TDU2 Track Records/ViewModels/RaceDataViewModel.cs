using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SQLite;
using System.Runtime.CompilerServices;
using TDU2_Track_Records.Properties;

namespace TDU2_Track_Records.ViewModels
{
    public class LapViewModel
    {
        public string LapName { get; set; }
        public string Minutes { get; set; }
        public string Seconds { get; set; }
        public string Milliseconds { get; set; }
        public string AverageSpeed { get; set; }
        public string Speed { get; set; }

        public string distance = Settings.Default.distance;
        public string speed = Settings.Default.speed;
        readonly string SI = Settings.Default.system;
    }

    public class RaceDataViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<LapViewModel> Laps { get; set; }

        private string _averageLapTime;
        public string AverageLapTime
        {
            get => _averageLapTime;
            set { _averageLapTime = value; OnPropertyChanged(); }
        }

        private string _averageSpeed;
        public string AverageSpeed
        {
            get => _averageSpeed;
            set { _averageSpeed = value; OnPropertyChanged(); }
        }

        private string _totalTime;
        public string TotalTime
        {
            get => _totalTime;
            set { _totalTime = value; OnPropertyChanged(); }
        }

        private string _timestamp;
        public string Timestamp
        {
            get => _timestamp;
            set { _timestamp = value; OnPropertyChanged(); }
        }

        public RaceDataViewModel()
        {
            Laps = new ObservableCollection<LapViewModel>();
            LoadLapsFromDatabase();
        }

        private void LoadLapsFromDatabase()
        {

            string connectionString = Settings.Default.connectionString;
            string speed = Settings.Default.speed;

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = $"SELECT Laps FROM Tracks"; // Update this query based on your table structure
                using (var command = new SQLiteCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int numberOfLaps = reader.GetInt32(0);
                            for (int i = 1; i <= numberOfLaps; i++)
                            {
                                Laps.Add(new LapViewModel
                                {
                                    LapName = $"Lap {i}",
                                    Minutes = "00",
                                    Seconds = "00",
                                    Milliseconds = "000",
                                    AverageSpeed = "0.0",
                                    Speed = speed
                                });
                            }
                        }
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
