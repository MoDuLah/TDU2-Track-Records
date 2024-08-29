using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using TDU2_Track_Records.Properties;

namespace TDU2_Track_Records
{
    public partial class SettingsWindow : Window
    {
        public delegate void MeasurementSystemChangedEventHandler(object sender, EventArgs e);

        public SettingsWindow()
        {
            InitializeComponent();
            LoadSettings();
            UnitSlider.ValueChanged += UnitSlider_ValueChanged;
        }

        private void LoadSettings()
        {
            OpacitySlider.Value = Settings.Default.MainWindowOpacity;
            UnitSlider.Value = Settings.Default.system == "Imperial" ? 1 : 0;
        }

        private void UnitSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int sliderValue = (int)UnitSlider.Value;
            if (e.NewValue == 1)
            {
                Settings.Default.system = "Imperial";
                Settings.Default.speed = "mph";
                Settings.Default.weight = "lbs";
                Settings.Default.distance = "mi";
                Settings.Default.torque = "lb⋅ft";
            }
            else
            {
                Settings.Default.system = "Metric";
                Settings.Default.speed = "km/h";
                Settings.Default.weight = "kg";
                Settings.Default.distance = "km";
                Settings.Default.torque = "N⋅m";
            }
            Settings.Default.Save();
            // Close the main window and reopen it
            Application.Current.MainWindow.Close();
            MainWindow mainWindow = new MainWindow();
            Application.Current.MainWindow = mainWindow;
            mainWindow.Show();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }
}
