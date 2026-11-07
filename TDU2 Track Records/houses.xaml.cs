using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TDU2_Track_Records.Properties;

namespace TDU2_Track_Records
{
    /// <summary>
    /// Interaction logic for houses.xaml
    /// </summary>
    public partial class houses : Window
    {
        readonly string connectionString = Settings.Default.connectionString;
        public string slotColumn;
        bool isUnlocked = true;
        private readonly LocationMenu locationMenu;

        public houses()
        {
            InitializeComponent();
            PreloadAvailableCars();
            var locations = LocationMenu.ReadLocations(connectionString,
                "SELECT Name, Island FROM houses ORDER BY id");
            locationMenu = new LocationMenu(LocationMenus, LocationSearchBox, LocationCountText,
                locations, "PROPERTIES", ShowLocation);
            locationMenu.SelectFirst();
        }
        // Preload the list of available cars once
        private List<dynamic> availableCars = new List<dynamic>();

        private void PreloadAvailableCars()
        {
            availableCars.Clear(); // Clear any previous data

            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT id, _vehicle_name FROM vehicles ";
                if (isUnlocked == false)
                {
                    query += "WHERE _is_purchasable = 'true' ORDER BY _vehicle_name ";
                }
                else
                {
                    query += " ORDER BY _vehicle_name ";
                }
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            availableCars.Add(new
                            {
                                VehicleName = reader["_vehicle_name"].ToString(),
                                VehicleId = reader["id"].ToString()
                            });
                        }
                    }
                }
            }
        }


        private void ShowLocation(LocationMenu.Location location)
        {
            LoadVehicles(location.Name);
            HouseName.Visibility = Visibility.Visible;
            VehiclesScrollViewer.Visibility = Visibility.Visible;
            VehiclesScrollViewer.ScrollToTop();
        }

        internal void RefreshMeasurementUnits()
        {
            if (locationMenu.Selected == null)
                return;

            double offset = VehiclesScrollViewer.VerticalOffset;
            LoadVehicles(locationMenu.Selected.Name);
            VehiclesScrollViewer.ScrollToVerticalOffset(offset);
        }

        private void LoadVehicles(string dealershipName)
        {
            VehiclesStackPanel.Children.Clear();
            int slotIndex = 0;
            string island = locationMenu.Selected.Island;

            // Initialize the flag to check if any vehicle is found
            bool hasVehicle = false;

            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM houses WHERE Island = @Island AND Name = @Name";
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", dealershipName);
                    cmd.Parameters.AddWithValue("@Island", island);
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Set house details
                            HouseNameText.Text = reader["Name"].ToString();
                            HouseRating.Text = reader["Stars"].ToString();
                            HousePrice.Text = "$" + Convert.ToInt32(reader["Price"]).ToString("N0");
                            int houseMaxSlot = Convert.ToInt32(reader["Slots"]);
                            HouseMaxSlots.Text = houseMaxSlot.ToString();

                            // Load house image from file system
                            string houseName = reader["Name"].ToString().ToLower();
                            if (houseName.Contains("yacht"))
                            {
                                houseName = island == "Ibiza" ? "yachtibiza" : "yachthawaii";
                            }
                            BitmapImage imagePath = new BitmapImage(new Uri("/Images/houses/" + houseName + ".png", UriKind.Relative));
                            HouseImage.Source = imagePath;

                            // Load vehicles in slots
                            for (int i = 1; i <= houseMaxSlot; i++)
                            {
                                string slotColumn = $"Slot{i}_VehicleId";
                                string vehicleId = reader[slotColumn]?.ToString();

                                if (string.IsNullOrEmpty(vehicleId))
                                {
                                    AddComboBoxToUI(dealershipName, slotIndex, slotColumn);
                                }
                                else
                                {
                                    var vehicle = GetVehicleDetails(vehicleId);
                                    if (vehicle != null)
                                    {
                                        AddVehicleToUI(vehicle, slotIndex, slotColumn);
                                        hasVehicle = true; // Mark that at least one vehicle is found
                                    }
                                }
                                slotIndex++;
                            }
                            // Set HouseOwned.Text based on whether any vehicle was found
                            HouseOwned.Text = hasVehicle ? "Yes" : "No";
                        }
                    }
                }
            }
        }



        private void AddComboBoxToUI(string dealershipName, int slotIndex, string slotColumn)
        {
            if (isUnlocked == true)
            {
                // Create the main GroupBox for the empty slot
                GroupBox emptySlotGroupBox = new GroupBox
                {
                    Header = $"Empty Slot {slotIndex + 1}",
                    Margin = new Thickness(10),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(150, 150, 150)),
                    BorderThickness = new Thickness(2),
                    Padding = new Thickness(10)
                };

                // Create a StackPanel to hold the ComboBox
                StackPanel contentStack = new StackPanel();

                // Create the ComboBox for selecting a car
                ComboBox availableCarsComboBox = new ComboBox
                {
                    Width = 150,
                    Tag = slotColumn // Store the actual slot column name in the Tag property
                };

                // Load the available cars into the ComboBox
                LoadAvailableCarsIntoComboBox(availableCarsComboBox);

                // Add a SelectionChanged event to handle car selection
                availableCarsComboBox.SelectionChanged += (sender, e) =>
                {
                    if (availableCarsComboBox.SelectedValue != null)
                    {
                        string selectedVehicleId = availableCarsComboBox.SelectedValue.ToString();
                        if (!string.IsNullOrEmpty(selectedVehicleId))
                        {
                            AddVehicleToSlot(dealershipName, selectedVehicleId, slotColumn);

                            // Refresh the UI after adding the car
                            LoadVehicles(dealershipName);
                        }
                    }
                };

                // Add the ComboBox to the contentStack
                contentStack.Children.Add(availableCarsComboBox);

                // Set the content of the GroupBox
                emptySlotGroupBox.Content = contentStack;

                // Ensure VehiclesStackPanel is a Grid (you may need to adjust based on your layout)
                Grid grid = VehiclesStackPanel as Grid;
                if (grid == null)
                {
                    // Initialize a new Grid if VehiclesStackPanel is not a Grid
                    grid = new Grid();
                    grid.ColumnDefinitions.Add(new ColumnDefinition());
                    grid.ColumnDefinitions.Add(new ColumnDefinition());
                    grid.ColumnDefinitions.Add(new ColumnDefinition());

                    // Define rows based on the number of slots
                    int rows = (8 + 1) / 2;
                    for (int i = 0; i < rows; i++)
                    {
                        grid.RowDefinitions.Add(new RowDefinition());
                    }

                    // Clear existing content and set the new Grid
                    VehiclesStackPanel.Children.Clear();
                    VehiclesStackPanel.Children.Add(grid);
                }

                // Determine the column and row for this empty slot
                int column = slotIndex % 3;
                int row = slotIndex / 3;

                // Set the position of the GroupBox in the grid
                Grid.SetColumn(emptySlotGroupBox, column);
                Grid.SetRow(emptySlotGroupBox, row);

                // Add the GroupBox to the grid
                grid.Children.Add(emptySlotGroupBox);
            }
        }


        private void LoadAvailableCarsIntoComboBox(ComboBox comboBox)
        {
            // Reuse the preloaded list of available cars
            foreach (var car in availableCars)
            {
                comboBox.Items.Add(car);
            }

            comboBox.DisplayMemberPath = "VehicleName";
            comboBox.SelectedValuePath = "VehicleId";
        }


        private void AddVehicleToSlot(string dealershipName, string vehicleId, string slotColumn)
        {
            string island = locationMenu.Selected.Island;
            string column = "";
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                // Update the slot with the selected vehicle ID
                string query = $"UPDATE houses SET {slotColumn} = @VehicleId WHERE Name = @Name and Island = @Island; ";
                query += $"UPDATE vehicles SET _is_owned = 'true' WHERE id = @VehicleId ; ";

                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@VehicleId", vehicleId);
                    cmd.Parameters.AddWithValue("@Name", dealershipName);
                    cmd.Parameters.AddWithValue("@Island", island);
                    cmd.ExecuteNonQuery();
                }
            }
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                if (island == "Ibiza")
                {
                    column = "ibiza";
                }
                else
                {
                    column = "hawaii";
                }
                // Update the slot with the selected vehicle ID
                string query = $"UPDATE vehicles SET _house_name_in_{column} = @Name WHERE id = @VehicleId";

                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@VehicleId", vehicleId);
                    cmd.Parameters.AddWithValue("@Name", dealershipName);
                    cmd.Parameters.AddWithValue("@Island", island);
                    cmd.ExecuteNonQuery();
                }
            }
        }


        private VehicleManagement GetVehicleDetails(string vehicleId)
        {
            VehicleManagement vehicle = null;
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = @"SELECT vehicles.*,
                                        CAST(_vehicle_image AS BLOB) AS _vehicle_image_bytes
                                 FROM vehicles
                                 WHERE Id = @Id";
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", vehicleId);
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            vehicle = new VehicleManagement
                            {
                                id = Convert.ToInt32(reader["id"]),
                                VehicleName = reader["_vehicle_name"].ToString(),
                                VehicleCategory = reader["_vehiclecategory_name"].ToString(),
                                VehicleLevel = reader["_upgrade_level"].ToString(),
                                IsOwned = Convert.ToBoolean(reader["_is_owned"].ToString()),
                                VehicleTopSpeed = reader["_max_theorical_speed"] == DBNull.Value ? string.Empty : reader["_max_theorical_speed"].ToString(),
                                Acceleration0To100Kph = reader["_acceleration_0_100_kph"] == DBNull.Value ? string.Empty : reader["_acceleration_0_100_kph"].ToString(),
                                VehicleOdometerMetric = reader["_odometer_metric"] == DBNull.Value ? 0d : Convert.ToDouble(reader["_odometer_metric"]),
                                VehicleOdometerImperial = reader["_odometer_imperial"] == DBNull.Value ? 0d : Convert.ToDouble(reader["_odometer_imperial"]),
                                VehicleImage = reader["_vehicle_image_bytes"] != DBNull.Value
                                    ? (byte[])reader["_vehicle_image_bytes"]
                                    : null

                            };
                        }
                    }
                }
            }
            return vehicle;
        }


        private void AddVehicleToUI(VehicleManagement vehicle, int slotIndex, string slotColumn)
        {
            Border vehicleCard = new Border
            {
                Margin = new Thickness(6),
                Background = new SolidColorBrush(Color.FromRgb(26, 28, 32)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(59, 62, 69)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(2)
            };

            Grid card = new Grid();
            card.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            card.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            card.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            card.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            Grid header = new Grid { Background = new SolidColorBrush(Color.FromRgb(37, 39, 44)) };
            header.ColumnDefinitions.Add(new ColumnDefinition());
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            header.Children.Add(new TextBlock
            {
                Text = $"SLOT {slotIndex + 1:00}",
                Foreground = new SolidColorBrush(Color.FromRgb(255, 180, 170)),
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(10, 6, 10, 6)
            });
            Border tuneChip = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(229, 37, 33)),
                Padding = new Thickness(8, 2, 8, 2), Margin = new Thickness(5),
                Child = new TextBlock { Text = "STAGE " + vehicle.VehicleLevel, Foreground = Brushes.White, FontSize = 11, FontWeight = FontWeights.Bold }
            };
            Grid.SetColumn(tuneChip, 1);
            header.Children.Add(tuneChip);
            card.Children.Add(header);

            Grid identity = new Grid { Margin = new Thickness(10) };
            identity.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(145) });
            identity.ColumnDefinitions.Add(new ColumnDefinition());
            Border imageBorder = new Border { Height = 92, ClipToBounds = true, BorderBrush = new SolidColorBrush(Color.FromRgb(59, 62, 69)), BorderThickness = new Thickness(1) };
            imageBorder.Child = new Image { Source = LoadImage(vehicle.VehicleImage), Stretch = Stretch.UniformToFill };
            identity.Children.Add(imageBorder);
            StackPanel identityText = new StackPanel { Margin = new Thickness(12, 3, 0, 0) };
            identityText.Children.Add(new TextBlock { Text = vehicle.VehicleName.ToUpperInvariant(), Foreground = Brushes.White, FontSize = 17, FontWeight = FontWeights.Bold, TextWrapping = TextWrapping.Wrap });
            identityText.Children.Add(new TextBlock { Text = "CLASS " + vehicle.VehicleCategory, Foreground = new SolidColorBrush(Color.FromRgb(255, 180, 170)), FontSize = 12, Margin = new Thickness(0, 4, 0, 0) });
            double odometer = MeasurementUnits.IsImperial ? vehicle.VehicleOdometerImperial : vehicle.VehicleOdometerMetric;
            identityText.Children.Add(new TextBlock { Text = $"ODO: {odometer:F2} {Settings.Default.distance}", Foreground = new SolidColorBrush(Color.FromRgb(210, 210, 215)), FontSize = 13, Margin = new Thickness(0, 4, 0, 0) });
            Grid.SetColumn(identityText, 1);
            identity.Children.Add(identityText);
            Grid.SetRow(identity, 1);
            card.Children.Add(identity);

            Grid metrics = new Grid { Margin = new Thickness(10, 0, 10, 8), Background = new SolidColorBrush(Color.FromRgb(12, 14, 18)) };
            metrics.ColumnDefinitions.Add(new ColumnDefinition()); metrics.ColumnDefinitions.Add(new ColumnDefinition()); metrics.ColumnDefinitions.Add(new ColumnDefinition());
            string topSpeed = MeasurementUnits.TryParse(vehicle.VehicleTopSpeed, out double speedKph)
                ? $"{MeasurementUnits.FromMetric(speedKph, MeasurementUnits.MilesPerKilometer):0.##} {Settings.Default.speed}"
                : "—";
            metrics.Children.Add(CreateHouseMetric("TOP SPEED", topSpeed, 0));
            metrics.Children.Add(CreateHouseMetric("0-100", string.IsNullOrWhiteSpace(vehicle.Acceleration0To100Kph) ? "—" : vehicle.Acceleration0To100Kph + " s", 1));
            metrics.Children.Add(CreateHouseMetric("CLASS", vehicle.VehicleCategory, 2));
            Grid.SetRow(metrics, 2); card.Children.Add(metrics);

            Grid actions = new Grid { Margin = new Thickness(10, 0, 10, 10) };
            actions.ColumnDefinitions.Add(new ColumnDefinition()); actions.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            Button removeButton = new Button { Content = "REMOVE FROM SLOT", Tag = slotColumn, Padding = new Thickness(10, 5, 10, 5) };
            removeButton.Click += DeleteVehicleButton_Click;
            actions.Children.Add(removeButton);
            Button viewDetailsButton = new Button { Content = "CARD / VIEW SPECS", Tag = vehicle.id, Padding = new Thickness(10, 5, 10, 5), Margin = new Thickness(6, 0, 0, 0) };
            viewDetailsButton.Click += ViewDetailsButton_Click;
            Grid.SetColumn(viewDetailsButton, 1); actions.Children.Add(viewDetailsButton);
            Grid.SetRow(actions, 3); card.Children.Add(actions);

            vehicleCard.Child = card;
            Grid grid = VehiclesStackPanel;
            const int columns = 3;
            while (grid.ColumnDefinitions.Count < columns) grid.ColumnDefinitions.Add(new ColumnDefinition());
            int rows = (slotIndex + columns) / columns;
            while (grid.RowDefinitions.Count < rows) grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            Grid.SetColumn(vehicleCard, slotIndex % columns);
            Grid.SetRow(vehicleCard, slotIndex / columns);
            grid.Children.Add(vehicleCard);
        }

        private static Border CreateHouseMetric(string label, string value, int column)
        {
            Border cell = new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromRgb(59, 62, 69)),
                BorderThickness = new Thickness(column == 0 ? 0 : 1, 0, 0, 0),
                Padding = new Thickness(6, 5, 6, 5),
                Child = new StackPanel
                {
                    Children =
                    {
                        new TextBlock { Text = label, Foreground = new SolidColorBrush(Color.FromRgb(170, 174, 183)), FontSize = 9, HorizontalAlignment = HorizontalAlignment.Center },
                        new TextBlock { Text = value, Foreground = Brushes.White, FontSize = 13, FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center }
                    }
                }
            };
            Grid.SetColumn(cell, column);
            return cell;
        }
        // Method to update the vehicle owned status in the database
        private void UpdateIsOwnedStatus(int vehicleId, bool isOwned)
        {
            // SQL update command to update the 'IsOwned' column in the database
            string query = "UPDATE vehicles SET _is_owned = @isOwned WHERE id = @vehicleId";

            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@isOwned", isOwned ? "true" : "false");
                    cmd.Parameters.AddWithValue("@vehicleId", vehicleId);
                    cmd.ExecuteNonQuery();
                }
                conn.Close();
            }
        }

        private void ViewDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                if (button?.Tag == null || !int.TryParse(button.Tag.ToString(), out int vehicleId))
                {
                    MessageBox.Show("Invalid Vehicle ID");
                    return;
                }
                else
                {
                    // Get the vehicle ID from the button's Tag
                    vehicleId = Convert.ToInt32(button.Tag);
                    // Open the vehicleCard window and pass the vehicle ID
                    vehicleCard vehicleWindow = new vehicleCard(vehicleId);
                    vehicleWindow.ShowDialog();
                }
            }
        }

        // Event handler for the Delete button
        private void DeleteVehicleButton_Click(object sender, RoutedEventArgs e)
        {
            Button deleteButton = sender as Button;
            if (deleteButton != null)
            {
                // Retrieve the actual column name from the Tag property
                string slotColumn = deleteButton.Tag.ToString();
                // Remove the vehicle from the dealership using the column name
                RemoveVehicleFromSlot(slotColumn);

                // Refresh the UI after deletion
                LoadVehicles(locationMenu.Selected.Name);
            }
        }

        private void RemoveVehicleFromSlot(string slotColumn)
        {
            string dealershipName = locationMenu.Selected.Name;
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                // Use the column name directly in the SQL query to set it to NULL
                string query = $"UPDATE houses SET {slotColumn} = NULL WHERE Name = @Name AND Island = @Island";

                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", dealershipName);
                    cmd.Parameters.AddWithValue("@Island", locationMenu.Selected.Island);
                    cmd.ExecuteNonQuery();
                }
            }
        }


        private BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0)
            {
                // Return a placeholder image if imageData is null or empty
                var placeholderImage = new BitmapImage();
                placeholderImage.BeginInit();
                placeholderImage.UriSource = new Uri("pack://application:,,,/Images/houses/placeholder.png", UriKind.Absolute);
                placeholderImage.DecodePixelWidth = 300; // Set max width
                placeholderImage.DecodePixelHeight = 150; // Set max height
                placeholderImage.CacheOption = BitmapCacheOption.OnLoad;
                placeholderImage.EndInit();
                return placeholderImage;
            }

            using (var ms = new System.IO.MemoryStream(imageData))
            {
                BitmapImage img = new BitmapImage();
                img.BeginInit();
                img.StreamSource = ms;
                img.DecodePixelWidth = 300; // Set max width
                img.DecodePixelHeight = 150; // Set max height
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.EndInit();
                return img;
            }
        }

        private void LocationMenus_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

            if (isUnlocked == false)
            {
                isUnlocked = true;
            }
            else
            {
                isUnlocked = false;
            }
            PreloadAvailableCars();
        }

        private void dealerships_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Window.GetWindow(e.OriginalSource as DependencyObject) != this)
                return;

            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
        private void Settings_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Prompt the user before closing the parent window
            MessageBoxResult result = MessageBox.Show(
                "Results will not take effect until you re-open this window.",
                "Warning",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            var settingsWindow = new SettingsWindow();

            settingsWindow.Left = this.Left + this.Width + 10;
            settingsWindow.Top = this.Top;
            settingsWindow.ShowDialog();
        }

        private void Minimize_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Window.GetWindow(this).WindowState = WindowState.Minimized;
        }

        private void Close_Button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }
    }
}
