using System;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using TDU2_Track_Records.Properties;
using System.Windows.Input;
using System.Collections.Generic;
using System.IO;

namespace TDU2_Track_Records
{
    public partial class dealerships : Window
    {
        readonly string connectionString = Settings.Default.connectionString;
        public string distance, speed;
        public string slotColumn;
        readonly string SI = Settings.Default.system;
        public int SelectedMaxSlots = Settings.Default.dealershipSlots;
        public int dealershipColumn = Settings.Default.dealershipColumns;
        private int DealerCardColumns => Math.Max(1, dealershipColumn);
        private static double lastcardLeft;
        private static double lastcardTop;
        bool isUnlocked = false;
        private readonly LocationMenu locationMenu;

        public dealerships()
        {
            InitializeComponent();
            PreloadAvailableCars();
            var locations = LocationMenu.ReadLocations(connectionString,
                "SELECT Name, Island FROM dealerships ORDER BY id", separateCasinoShowrooms: true);
            locationMenu = new LocationMenu(LocationMenus, LocationSearchBox, LocationCountText,
                locations, "LOCATIONS", ShowLocation, includeCasino: true);
            locationMenu.SelectFirst();
        }

        private static TextBlock CreateGroupHeader(string text, double fontSize)
        {
            return new TextBlock
            {
                Text = text,
                FontSize = fontSize,
                FontWeight = FontWeights.SemiBold,
                TextTrimming = TextTrimming.CharacterEllipsis
            };
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
                if (isUnlocked == false) { 
                    query += "WHERE _is_purchasable = 'true' ORDER BY _vehicle_name ";
                } else {
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
            VehiclesScrollViewer.Visibility = Visibility.Visible;
            VehiclesScrollViewer.ScrollToTop();
        }

        private void LoadVehicles(string dealershipName)
        {
            VehiclesStackPanel.Children.Clear(); // Clear existing UI elements before loading new ones
            VehiclesStackPanel.ColumnDefinitions.Clear();
            VehiclesStackPanel.RowDefinitions.Clear();
            for (int columnIndex = 0; columnIndex < DealerCardColumns; columnIndex++)
                VehiclesStackPanel.ColumnDefinitions.Add(new ColumnDefinition());
            int slotIndex = 0;
            string island = locationMenu.Selected.Island;

            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM dealerships WHERE Island = @Island AND Name = @Name";
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", dealershipName);
                    cmd.Parameters.AddWithValue("@Island", island);
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Dealership.Visibility = Visibility.Visible;
                            DealershipTitleText.Text = dealershipName.ToUpperInvariant();
                            DealershipIslandText.Text = island.ToUpperInvariant();
                            DealershipSubtitleText.Text = $"Selected showroom on {island}. Browse the vehicles currently assigned to this location.";

                            int inStockCount = 0;
                            for (int stockIndex = 1; stockIndex <= SelectedMaxSlots; stockIndex++)
                            {
                                if (!string.IsNullOrWhiteSpace(reader[$"Slot{stockIndex}_VehicleId"]?.ToString()))
                                    inStockCount++;
                            }
                            DealershipStockText.Text = $"{inStockCount} CARS";

                            try
                            {
                                BitmapImage imagePath = new BitmapImage(new Uri($"/Images/dealerships/{island}/{dealershipName}.png", UriKind.Relative));
                                DealershipImage.Source = imagePath;
                            }
                            catch (Exception)
                            {
                                // Handle case where image is not found
                                BitmapImage fallbackImage = new BitmapImage(new Uri("/Images/dealerships/default.png", UriKind.Relative)); // Fallback image
                                DealershipImage.Source = fallbackImage;
                            }

                            // Loop through all 20 slots
                            for (int i = 1; i <= SelectedMaxSlots; i++) // Increased from 8 to 20 slots
                            {
                                string slotColumn = $"Slot{i}_VehicleId";
                                string vehicleId = reader[slotColumn]?.ToString();

                                if (string.IsNullOrEmpty(vehicleId))
                                {
                                    // If the slot is empty, add a ComboBox for selecting a vehicle
                                    AddComboBoxToUI(dealershipName, slotIndex, slotColumn);
                                }
                                else
                                {
                                    // Load the vehicle details for non-empty slots
                                    var vehicle = GetVehicleDetails(vehicleId);
                                    if (vehicle != null)
                                    {
                                        AddVehicleToUI(vehicle, slotIndex, slotColumn);
                                    }
                                }

                                // Increment the slotIndex to ensure proper positioning in the grid
                                slotIndex++;
                            }
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
                    Header = CreateGroupHeader($"Empty Slot {slotIndex + 1}", 20),
                    Margin = new Thickness(5, 0, 5, 0),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(150, 150, 150)),
                    BorderThickness = new Thickness(2),
                    Padding = new Thickness(5, 0, 5, 0)
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
                            // Add the selected vehicle to the slot
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

                // Ensure VehiclesStackPanel is a Grid
                Grid grid = VehiclesStackPanel as Grid;
                if (grid == null)
                {
                    // Initialize a new Grid if VehiclesStackPanel is not a Grid
                    grid = new Grid();
                    VehiclesStackPanel.Children.Clear();
                    VehiclesStackPanel.Children.Add(grid);
                }

                // Determine the number of columns (make it consistent with LoadVehicles)
                int columns = DealerCardColumns;

                // Add columns dynamically if needed
                while (grid.ColumnDefinitions.Count < columns)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition());
                }

                // Calculate the number of rows needed based on the number of vehicles
                int totalSlots = slotIndex + 1;
                int rows = (totalSlots + columns - 1) / columns;

                // Add rows dynamically if needed
                while (grid.RowDefinitions.Count < rows)
                {
                    grid.RowDefinitions.Add(new RowDefinition());
                }

                // Determine the column and row for this empty slot
                int column = slotIndex % columns;
                int row = slotIndex / columns;

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
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                // Update the slot with the selected vehicle ID
                string query = $"UPDATE dealerships SET {slotColumn} = @VehicleId WHERE Name = @Name and Island = @Island";

                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@VehicleId", vehicleId);
                    cmd.Parameters.AddWithValue("@Name", dealershipName);
                    cmd.Parameters.AddWithValue("@Island", island);
                    cmd.ExecuteNonQuery();
                }
            }
            if (island != "Ibiza" && island != "Hawaii")
                return;

            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string column = island.ToLowerInvariant();
                // Update the slot with the selected vehicle ID
                string query = $"UPDATE vehicles SET _dealership_name_in_{column} = @Name WHERE id = @VehicleId";

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
                                VehiclePrice = reader["_price"].ToString(),
                                VehicleCategory = reader["_vehiclecategory_name"].ToString(),
                                VehicleLevel = reader["_upgrade_level"].ToString(),
                                IsOwned = Convert.ToBoolean(reader["_is_owned"].ToString()),
                                IsPurchasable = Convert.ToBoolean(reader["_is_purchasable"]),
                                IsReward = Convert.ToBoolean(reader["_is_reward"]),
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

            Grid cardLayout = new Grid();
            cardLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            cardLayout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(150) });
            cardLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            cardLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            Grid header = new Grid { Background = new SolidColorBrush(Color.FromRgb(37, 39, 44)) };
            header.ColumnDefinitions.Add(new ColumnDefinition());
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            TextBlock name = new TextBlock
            {
                Text = vehicle.VehicleName.ToUpperInvariant(),
                Foreground = Brushes.White,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(12, 8, 8, 8)
            };
            header.Children.Add(name);
            Border classChip = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(229, 37, 33)),
                Margin = new Thickness(5),
                Padding = new Thickness(8, 3, 8, 3),
                VerticalAlignment = VerticalAlignment.Center,
                Child = new TextBlock { Text = vehicle.VehicleCategory, Foreground = Brushes.White, FontWeight = FontWeights.Bold }
            };
            Grid.SetColumn(classChip, 1);
            header.Children.Add(classChip);
            cardLayout.Children.Add(header);

            Grid imageSurface = new Grid { Background = Brushes.Black, Margin = new Thickness(10, 8, 10, 0), ClipToBounds = true };
            Image vehicleImage = new Image { Source = LoadImage(vehicle.VehicleImage), Stretch = Stretch.UniformToFill };
            imageSurface.Children.Add(vehicleImage);
            if (decimal.TryParse(vehicle.VehiclePrice, out decimal price))
            {
                Border priceBadge = new Border
                {
                    Background = new SolidColorBrush(Color.FromArgb(230, 12, 14, 18)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(229, 37, 33)),
                    BorderThickness = new Thickness(1),
                    Padding = new Thickness(8, 4, 8, 4),
                    Margin = new Thickness(8),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Child = new TextBlock { Text = "$" + price.ToString("N0"), Foreground = new SolidColorBrush(Color.FromRgb(255, 180, 170)), FontWeight = FontWeights.Bold }
                };
                imageSurface.Children.Add(priceBadge);
            }
            Grid.SetRow(imageSurface, 1);
            cardLayout.Children.Add(imageSurface);

            Grid metrics = new Grid { Margin = new Thickness(10, 8, 10, 8), Background = new SolidColorBrush(Color.FromRgb(12, 14, 18)) };
            metrics.ColumnDefinitions.Add(new ColumnDefinition());
            metrics.ColumnDefinitions.Add(new ColumnDefinition());
            metrics.ColumnDefinitions.Add(new ColumnDefinition());
            metrics.Children.Add(CreateDealerMetric("VALUE", decimal.TryParse(vehicle.VehiclePrice, out decimal parsedPrice) ? "$" + parsedPrice.ToString("N0") : "—", 0));
            metrics.Children.Add(CreateDealerMetric("TUNE", "STAGE " + vehicle.VehicleLevel, 1));
            metrics.Children.Add(CreateDealerMetric("STATUS", vehicle.IsOwned ? "OWNED" : "AVAILABLE", 2));
            Grid.SetRow(metrics, 2);
            cardLayout.Children.Add(metrics);

            Grid actions = new Grid { Margin = new Thickness(10, 0, 10, 10) };
            actions.ColumnDefinitions.Add(new ColumnDefinition());
            actions.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            CheckBox ownedCheckBox = new CheckBox
            {
                IsChecked = vehicle.IsOwned,
                Content = "OWNED",
                Foreground = new SolidColorBrush(Color.FromRgb(255, 180, 170)),
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.SemiBold
            };
            ownedCheckBox.Checked += (sender, e) => UpdateIsOwnedStatus(vehicle.id, true);
            ownedCheckBox.Unchecked += (sender, e) => UpdateIsOwnedStatus(vehicle.id, false);
            actions.Children.Add(ownedCheckBox);
            Button viewDetailsButton = new Button
            {
                Content = "CARD / VIEW SPECS",
                FontSize = 14,
                Padding = new Thickness(12, 6, 12, 6),
                Tag = vehicle.id
            };
            viewDetailsButton.Click += ViewDetailsButton_Click;
            Grid.SetColumn(viewDetailsButton, 1);
            actions.Children.Add(viewDetailsButton);
            Grid.SetRow(actions, 3);
            cardLayout.Children.Add(actions);

            vehicleCard.Child = cardLayout;

            Grid grid = VehiclesStackPanel;
            int columns = DealerCardColumns;
            while (grid.ColumnDefinitions.Count < columns)
                grid.ColumnDefinitions.Add(new ColumnDefinition());
            int rows = (slotIndex + columns) / columns;
            while (grid.RowDefinitions.Count < rows)
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            Grid.SetColumn(vehicleCard, slotIndex % columns);
            Grid.SetRow(vehicleCard, slotIndex / columns);
            grid.Children.Add(vehicleCard);
        }

        private static Border CreateDealerMetric(string label, string value, int column)
        {
            Border cell = new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromRgb(59, 62, 69)),
                BorderThickness = new Thickness(column == 0 ? 0 : 1, 0, 0, 0),
                Padding = new Thickness(8, 6, 8, 6),
                Child = new StackPanel
                {
                    Children =
                    {
                        new TextBlock { Text = label, Foreground = new SolidColorBrush(Color.FromRgb(170, 174, 183)), FontSize = 10, HorizontalAlignment = HorizontalAlignment.Center },
                        new TextBlock { Text = value, Foreground = Brushes.White, FontSize = 14, FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center }
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
                else { 
                // Get the vehicle ID from the button's Tag
                vehicleId = Convert.ToInt32(button.Tag);
                    // Open the vehicleCard window and pass the vehicle ID
                    vehicleCard vehicleWindow = new vehicleCard(vehicleId);
                    // Get the screen dimensions
                    var screenWidth = SystemParameters.PrimaryScreenWidth;
                    var screenHeight = SystemParameters.PrimaryScreenHeight;

                    // Set the position to the stored values, or default to the main window position if not set
                    double targetLeft = lastcardLeft != 0 ? lastcardLeft : this.Left - 10;
                    double targetTop = lastcardTop != 0 ? lastcardTop : this.Top + (this.Height - vehicleWindow.Height) / 2;

                    // Adjust if the window would be positioned outside the screen bounds
                    if (targetLeft + vehicleWindow.Width > screenWidth)
                    {
                        targetLeft = screenWidth - vehicleWindow.Width;
                    }
                    if (targetTop + vehicleWindow.Height > screenHeight)
                    {
                        targetTop = screenHeight - vehicleWindow.Height;
                    }
                    if (targetLeft < 0)
                    {
                        targetLeft = 0;
                    }
                    if (targetTop < 0)
                    {
                        targetTop = 0;
                    }

                    // Set the window position
                    vehicleWindow.Left = targetLeft;
                    vehicleWindow.Top = targetTop;


                vehicleWindow.Closed += cardWindow_Closed;
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
            string island = locationMenu.Selected.Island;
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                // Use the column name directly in the SQL query to set it to NULL
                string query = $"UPDATE dealerships SET {slotColumn} = NULL WHERE Name = @Name AND Island = @Island";

                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Island", island);
                    cmd.Parameters.AddWithValue("@Name", dealershipName);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void cardWindow_Closed(object sender, EventArgs e)
        {
            var cardWindow = sender as Window;
            // Refresh the UI after deletion
            if (cardWindow != null)
            {
                lastcardLeft = cardWindow.Left;
                lastcardTop = cardWindow.Top;
            }
            LoadVehicles(locationMenu.Selected.Name);
        }
        private BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0)
            {
                // Return a placeholder image if imageData is null or empty
                var placeholderImage = new BitmapImage();
                placeholderImage.BeginInit();
                placeholderImage.UriSource = new Uri("pack://application:,,,/Images/vehicles/placeholder.png", UriKind.Absolute);
                placeholderImage.DecodePixelWidth = 300; // Set max width
                placeholderImage.DecodePixelHeight = 150; // Set max height
                placeholderImage.CacheOption = BitmapCacheOption.OnLoad;
                placeholderImage.EndInit();
                return placeholderImage;
            }

            using (var ms = new MemoryStream(imageData))
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

            if(isUnlocked == false) { 
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
