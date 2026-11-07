using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using TDU2_Track_Records.Properties;

namespace TDU2_Track_Records
{
    public partial class SettingsWindow : Window
    {
        private readonly string connectionString = Settings.Default.connectionString;
        private bool _profileSelectionChanging;
        private bool _profileSyncInProgress;

        public SettingsWindow()
        {
            InitializeComponent();
            LoadSettings();
            LoadSaveProfiles();
            UnitSlider.ValueChanged += UnitSlider_ValueChanged;
        }

        private void LoadSettings()
        {
            OpacitySlider.Value = Settings.Default.MainWindowOpacity;
        }

        private void LoadSaveProfiles(string selectedProfileKey = null)
        {
            try
            {
                List<SaveProfileInfo> profiles = SaveGameGarageImport.RefreshDiscoveredProfiles(connectionString);
                string key = selectedProfileKey ?? profiles.FirstOrDefault(profile => profile.IsActive)?.ProfileKey;
                _profileSelectionChanging = true;
                ProfileComboBox.ItemsSource = profiles;
                ProfileComboBox.DisplayMemberPath = nameof(SaveProfileInfo.DisplayName);
                ProfileComboBox.SelectedValuePath = nameof(SaveProfileInfo.ProfileKey);
                ProfileComboBox.SelectedValue = key;
                if (ProfileComboBox.SelectedIndex < 0 && profiles.Count > 0)
                    ProfileComboBox.SelectedIndex = 0;
                _profileSelectionChanging = false;
                SyncSelectedProfileButton.IsEnabled = ProfileComboBox.SelectedItem is SaveProfileInfo selectedProfile && selectedProfile.SourceAvailable;
            }
            catch (Exception ex)
            {
                _profileSelectionChanging = false;
                MessageBox.Show($"Save profiles could not be discovered.\n\n{ex.Message}",
                    "Profile discovery failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ProfileComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_profileSelectionChanging || !(ProfileComboBox.SelectedItem is SaveProfileInfo profile))
                return;
            try
            {
                SaveGameGarageImport.ActivateProfile(connectionString, profile.ProfileKey);
                SyncSelectedProfileButton.IsEnabled = profile.SourceAvailable;
                if (profile.SourceAvailable)
                {
                    Settings.Default.liveSavePath = profile.DataPath;
                    Settings.Default.Save();
                }
                (Application.Current.MainWindow as MainWindow)?.RefreshActiveSaveProfile();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"The profile could not be selected.\n\n{ex.Message}",
                    "Profile selection failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SyncSelectedProfile_Click(object sender, RoutedEventArgs e)
        {
            if (_profileSyncInProgress || !(ProfileComboBox.SelectedItem is SaveProfileInfo profile))
                return;
            if (!profile.SourceAvailable || string.IsNullOrWhiteSpace(profile.DataPath) || !File.Exists(profile.DataPath))
            {
                MessageBox.Show($"The selected profile's PLAYERSAVE\\DATA file is not available.\n\n{profile.DataPath}",
                    "Save profile unavailable", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadSaveProfiles(profile.ProfileKey);
                return;
            }
            if (MessageBox.Show(
                    $"Sync only {profile.ProfileName} now?\n\n" +
                    "TDU2 may remain open, but wait for its save indicator to finish first. The import is rolled back if DATA changes while it is read.",
                    "Sync selected save profile", MessageBoxButton.OKCancel, MessageBoxImage.Information) != MessageBoxResult.OK)
                return;

            _profileSyncInProgress = true;
            object originalContent = SyncSelectedProfileButton.Content;
            SyncSelectedProfileButton.Content = $"Syncing {profile.ProfileName}…";
            SyncSelectedProfileButton.IsEnabled = false;
            try
            {
                GarageImportResult result = SaveGameGarageImport.ImportLiveData(connectionString, profile.DataPath);
                Settings.Default.liveSavePath = Path.GetFullPath(profile.DataPath);
                Settings.Default.Save();
                LoadSaveProfiles(result.ProfileKey);
                (Application.Current.MainWindow as MainWindow)?.RefreshActiveSaveProfile();
                SyncSelectedProfileButton.ToolTip =
                    $"Last synced {result.Profile}: {result.ImportedVehicles} cars from DATA modified {result.SourceModifiedLocal:G}";
                MessageBox.Show(
                    $"Imported {result.ImportedVehicles} cars for {result.Profile}.\n" +
                    $"Catalogue matches: {result.CatalogueMatches}\n" +
                    $"Unknown models: {result.UnknownModels}\n" +
                    $"Live DATA modified: {result.SourceModifiedLocal:G}\n" +
                    $"Source SHA-256: {result.SourceHash.Substring(0, 12)}…",
                    "Selected profile synced", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"The selected save profile was not imported.\n\n{ex.Message}",
                    "Mileage sync failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SyncSelectedProfileButton.Content = originalContent;
                SyncSelectedProfileButton.IsEnabled = ProfileComboBox.SelectedItem is SaveProfileInfo selected && selected.SourceAvailable;
                _profileSyncInProgress = false;
            }
        }

        private void UnitSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            bool imperial = e.NewValue == 1;
            if (imperial == MeasurementUnits.IsImperial)
                return;

            Settings.Default.speed = imperial ? "mph" : "km/h";
            Settings.Default.weight = imperial ? "lbs" : "kg";
            Settings.Default.distance = imperial ? "mi" : "km";
            Settings.Default.torque = imperial ? "lb⋅ft" : "N⋅m";
            Settings.Default.system = imperial ? "Imperial" : "Metric";
            Settings.Default.Save();

            foreach (Window window in Application.Current.Windows.Cast<Window>().ToArray())
            {
                switch (window)
                {
                    case MainWindow mainWindow:
                        mainWindow.RefreshMeasurementUnits();
                        break;
                    case Vehicle vehicleEditor:
                        vehicleEditor.RefreshMeasurementUnits();
                        break;
                    case OdometerWindow odometerEditor:
                        odometerEditor.RefreshMeasurementUnits();
                        break;
                    case tracks eventEditor:
                        eventEditor.RefreshMeasurementUnits();
                        break;
                    case houses houseView:
                        houseView.RefreshMeasurementUnits();
                        break;
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(sender as DependencyObject) is MainWindow mainWindow)
            {
                mainWindow.ShowEventsView();
                return;
            }

            Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Window.GetWindow(e.OriginalSource as DependencyObject) != this)
                return;

            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings.Default.Save();
        }

    }
}
