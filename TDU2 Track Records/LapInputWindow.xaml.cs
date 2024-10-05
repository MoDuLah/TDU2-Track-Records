using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace TDU2_Track_Records
{
    public partial class LapInputWindow : Window
    {
        public List<(int Minutes, int Seconds, int Milliseconds)> LapTimes { get; private set; }

        public LapInputWindow()
        {
            InitializeComponent();
            LapTimes = new List<(int, int, int)>();
        }

        private void Txt_Laps_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Clear previous fields
            lapFieldsPanel.Children.Clear();

            // Check if the input is a valid integer between 1 and 20
            if (int.TryParse(txt_Laps.Text, out int laps) && laps >= 1 && laps <= 20)
            {
                for (int i = 1; i <= laps; i++)
                {
                    var lapPanel = new StackPanel { Orientation = Orientation.Horizontal };

                    lapPanel.Children.Add(new TextBlock { Text = $"Lap {i} (mm:ss.ms): ", VerticalAlignment = VerticalAlignment.Center });
                    var txtMinutes = new TextBox { MinWidth = 60, MaxLength = 2, Name = $"txt_Minutes_{i}" };
                    var txtSeconds = new TextBox { MinWidth = 60, MaxLength = 2, Name = $"txt_Seconds_{i}" };
                    var txtMilliseconds = new TextBox { MinWidth = 60, MaxLength = 2, Name = $"txt_Milliseconds_{i}" };

                    // Add event handlers for formatting
                    txtMinutes.PreviewTextInput += NumericOnly_PreviewTextInput;
                    txtMinutes.TextChanged += NavigateToNextTextbox;
                    txtMinutes.LostFocus += FormatTimeInput;
                    txtSeconds.PreviewTextInput += NumericOnly_PreviewTextInput;
                    txtSeconds.LostFocus += FormatTimeInput;
                    txtSeconds.TextChanged += NavigateToNextTextbox;
                    txtMilliseconds.PreviewTextInput += NumericOnly_PreviewTextInput;
                    txtMilliseconds.LostFocus += FormatTimeInput;
                    txtMilliseconds.TextChanged += NavigateToNextTextbox;

                    lapPanel.Children.Add(txtMinutes);
                    lapPanel.Children.Add(new TextBlock { Text = ":",Focusable = false, VerticalAlignment = VerticalAlignment.Center });
                    lapPanel.Children.Add(txtSeconds);
                    lapPanel.Children.Add(new TextBlock { Text = ":", Focusable = false, VerticalAlignment = VerticalAlignment.Center });
                    lapPanel.Children.Add(txtMilliseconds);

                    lapFieldsPanel.Children.Add(lapPanel);
                }
            }
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            LapTimes.Clear(); // Clear any previous times

            int totalMinutes = 0;
            int totalSeconds = 0;
            int totalMilliseconds = 0;

            foreach (var child in lapFieldsPanel.Children)
            {
                if (child is StackPanel lapPanel)
                {
                    var minutesBox = lapPanel.Children[1] as TextBox;
                    var secondsBox = lapPanel.Children[3] as TextBox;
                    var millisecondsBox = lapPanel.Children[5] as TextBox;

                    // Validate and parse input from each TextBox
                    if (int.TryParse(minutesBox.Text, out int minutes) &&
                        int.TryParse(secondsBox.Text, out int seconds) &&
                        int.TryParse(millisecondsBox.Text, out int milliseconds))
                    {
                        // Add to totals
                        totalMinutes += minutes;
                        totalSeconds += seconds;
                        totalMilliseconds += milliseconds;

                        // Convert milliseconds to seconds if needed
                        if (totalMilliseconds >= 1000)
                        {
                            totalSeconds += totalMilliseconds / 1000;
                            totalMilliseconds %= 1000;
                        }
                    }
                    else
                    {
                        // Handle invalid input if necessary
                        MessageBox.Show("Please enter valid lap time values.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return; // Exit the method if there's an error
                    }
                }
            }

            // Convert total seconds to minutes if needed
            if (totalSeconds >= 60)
            {
                totalMinutes += totalSeconds / 60;
                totalSeconds %= 60;
            }

            // Assuming Race_Min, Race_Sec, Race_Ms are public properties in your MainWindow
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                // Format minutes and seconds to always display two digits
                mainWindow.Race_Min.Text = totalMinutes.ToString("D2");
                mainWindow.Race_Sec.Text = totalSeconds.ToString("D2");
                mainWindow.Race_Ms.Text = (totalMilliseconds / 10).ToString("D2"); // Display milliseconds in two digits
            }

            this.DialogResult = true; // Close the window and indicate success
            this.Close();
        }


        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false; // Close the window without setting values
            this.Close();
        }

        private void NumericOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow only numeric input
            e.Handled = !int.TryParse(e.Text, out _);
        }

        private void FormatTimeInput(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                int value; // Declare the value variable here

                // Ensure two digits for minutes, seconds, and milliseconds
                if (textBox.Name.StartsWith("txt_Minutes") ||
                    textBox.Name.StartsWith("txt_Seconds") ||
                    textBox.Name.StartsWith("txt_Milliseconds"))
                {
                    textBox.Text = int.TryParse(textBox.Text, out value) ? value.ToString("D2") : "00";
                }
            }
        }

        private void NavigateToNextTextbox(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.Text.Length == textBox.MaxLength)
            {
                // Focus on the next TextBox in the same StackPanel
                var lapPanel = FindParent<StackPanel>(textBox);
                if (lapPanel != null)
                {
                    // Find the index of the next TextBox
                    for (int i = 0; i < lapPanel.Children.Count; i++)
                    {
                        if (lapPanel.Children[i] == textBox)
                        {
                            // Iterate to find the next TextBox
                            for (int j = i + 1; j < lapPanel.Children.Count; j++)
                            {
                                if (lapPanel.Children[j] is TextBox nextTextBox)
                                {
                                    FocusNextTextBox(nextTextBox);
                                    break; // Exit the loop once the next TextBox is found
                                }
                            }
                            break; // Exit the loop once the current TextBox is found
                        }
                    }
                }
            }
        }
        private async void FocusNextTextBox(TextBox nextTextBox)
        {
            await Task.Delay(10); // Small delay to ensure UI updates
            nextTextBox.Focus();
        }

        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            if (parentObject is T parent) return parent;
            return FindParent<T>(parentObject);
        }
    }
}
