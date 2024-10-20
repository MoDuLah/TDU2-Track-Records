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
        public string distance, speed;
        public string slotColumn;
        readonly string SI = Settings.Default.system;
        bool isUnlocked = true;
        public houses()
        {
            InitializeComponent();
            PreloadAvailableCars(); // Preload cars once when initializing
        }

        private void IslandComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (DealershipListBox.Visibility == Visibility.Collapsed) { DealershipListBox.Visibility = Visibility.Visible; }
                string selectedIsland = IslandComboBox.SelectedValue?.ToString();
                if (!string.IsNullOrEmpty(selectedIsland))
                {
                    LoadDealerships(selectedIsland);
                    VehiclesScrollViewer.Visibility = Visibility.Hidden;
                }

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


        private void LoadDealerships(string island)
        {
            DealershipListBox.Items.Clear();

            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM houses WHERE Island = @Island ORDER BY id ASC";
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Island", island);
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string dealershipId = reader["Id"].ToString();
                            string dealershipName = reader["Name"].ToString();
                            DealershipListBox.Items.Add(dealershipName);
                        }
                    }
                }
            }
        }


        private void DealershipListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DealershipListBox.SelectedItem != null)
            {
                string selectedDealership = DealershipListBox.SelectedItem.ToString();
                LoadVehicles(selectedDealership);
                HouseName.Visibility = Visibility.Visible;
                VehiclesScrollViewer.Visibility = Visibility.Visible;
                VehiclesScrollViewer.ScrollToTop();
            }
            else
            {
                HouseName.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadVehicles(string dealershipName)
        {
            VehiclesStackPanel.Children.Clear();
            int slotIndex = 0;
            string island = IslandComboBox.SelectedValue?.ToString();

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
                            HouseName.Header = reader["Name"].ToString();
                            HouseRating.Text = reader["Stars"].ToString();
                            HousePrice.Text = reader["Price"].ToString();
                            int houseMaxSlot = Convert.ToInt32(reader["Slots"]);
                            HouseMaxSlots.Text = houseMaxSlot.ToString();

                            // Load house image from file system
                            string houseName = reader["Name"].ToString().ToLower();
                            if (houseName.Contains("yacht"))
                            {
                                houseName = IslandComboBox.Text == "Ibiza" ? "yachtibiza" : "yachthawaii";
                            }
                            BitmapImage imagePath = new BitmapImage(new Uri("/Images/houses/" + houseName + ".png", UriKind.Relative));
                            HouseImage.Source = imagePath;
                            HouseImage.Width = imagePath.Width * 0.65;
                            HouseImage.Height = imagePath.Height * 0.65;

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
                int column = slotIndex % 2;
                int row = slotIndex / 2;

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
            string island = IslandComboBox.SelectedValue?.ToString();
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
                string query = "SELECT * FROM vehicles WHERE Id = @Id";
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
                                // Check if the "Image" column is DBNull before casting
                                VehicleImage = reader["_vehicle_image"] != DBNull.Value ? (byte[])reader["_vehicle_image"] : null

                            };
                        }
                    }
                }
            }
            return vehicle;
        }


        private void AddVehicleToUI(VehicleManagement vehicle, int slotIndex, string slotColumn)
        {
            // Create the main GroupBox for the vehicle (existing code)
            GroupBox vehicleGroupBox = new GroupBox
            {
                Header = vehicle.VehicleName, // Set the vehicle name as the header
                Margin = new Thickness(10),
                BorderBrush = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
                BorderThickness = new Thickness(2),
                Padding = new Thickness(10)
            };

            // Create a StackPanel to hold all vehicle information
            StackPanel contentStack = new StackPanel();

            // Add the vehicle image
            Image vehicleImage = new Image
            {
                Width = 300,
                Source = LoadImage(vehicle.VehicleImage) // Convert byte[] to ImageSource
            };
            contentStack.Children.Add(vehicleImage);

            // Create a StackPanel for the vehicle info
            StackPanel infoStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            // Vehicle Class
            GroupBox classBox = new GroupBox
            {
                Header = "Class",
            };
            Image classImage = new Image
            {
                Width = 32,
                Height = 32,
                Source = new BitmapImage(new Uri("/Images/carClasses/" + vehicle.VehicleCategory + ".png", UriKind.Relative))
            };
            classBox.Content = classImage;
            infoStack.Children.Add(classBox);

            // Vehicle Class
            GroupBox levelBox = new GroupBox
            {
                Header = "Tune",
            };
            Image tuneImage = new Image
            {
                Width = 32,
                Height = 32,
                Source = new BitmapImage(new Uri($"/Images/vehicleCard/Tune{vehicle.VehicleLevel}.png", UriKind.Relative))
            };
            levelBox.Content = tuneImage;
            infoStack.Children.Add(levelBox);
 
            // Vehicle Status
            if (vehicle.IsPurchasable || vehicle.IsReward)
            {
                GroupBox statusBox = new GroupBox
                {
                    Header = "Status",
                };

                CheckBox ownedCheckBox = new CheckBox
                {
                    IsChecked = vehicle.IsOwned == true,
                    VerticalAlignment = VerticalAlignment.Center
                };
                // Add event handlers for CheckBox checked and unchecked events
                ownedCheckBox.Checked += (sender, e) =>
                {
                    UpdateIsOwnedStatus(vehicle.id, true);  // Update database when checked
                };

                ownedCheckBox.Unchecked += (sender, e) =>
                {
                    UpdateIsOwnedStatus(vehicle.id, false);  // Update database when unchecked
                };

                TextBlock ownedText = new TextBlock
                {
                    Text = "Owned",
                    FontSize = 16,
                    VerticalAlignment = VerticalAlignment.Center
                };

                DockPanel statusPanel = new DockPanel();
                statusPanel.Children.Add(ownedCheckBox);
                statusPanel.Children.Add(ownedText);

                statusBox.Content = statusPanel;
                infoStack.Children.Add(statusBox);
            }
            // Vehicle Id
            TextBlock vehicleIdd = new TextBlock
            {
                Text = vehicle.id.ToString(),
                Tag = slotColumn,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Visibility = Visibility.Collapsed
            };
            infoStack.Children.Add(vehicleIdd);
            // Add a button to open the vehicle card window
            Button viewDetailsButton = new Button
            {
                Content = "Card",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5),
                Tag = vehicle.id // Store the vehicle object in the button's Tag
            };
            viewDetailsButton.Click += ViewDetailsButton_Click;
            infoStack.Children.Add(viewDetailsButton);

            if (isUnlocked)
            {
                // Add a delete button for each vehicle and store the column name in the Tag property
                Button deleteButton = new Button
                {
                    Content = "Remove",
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(5),
                    Tag = slotColumn // Store the actual column name (e.g., "Slot1_VehicleId")
                };
                deleteButton.Click += DeleteVehicleButton_Click;

                // Add the delete button to the infoStack
                infoStack.Children.Add(deleteButton);
            }
            // Add the infoStack to contentStack
            contentStack.Children.Add(infoStack);

            // Set the content of the GroupBox
            vehicleGroupBox.Content = contentStack;

            // Ensure VehiclesStackPanel is a Grid
            Grid grid = VehiclesStackPanel as Grid;
            if (grid == null)
            {
                // Initialize a new Grid if VehiclesStackPanel is not a Grid
                grid = new Grid();

                // Define two columns in the Grid
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.ColumnDefinitions.Add(new ColumnDefinition());

                // Define rows based on the number of vehicles
                int rows = (8 + 1) / 2; // 8 slots, 2 columns per row
                for (int i = 0; i < rows; i++)
                {
                    grid.RowDefinitions.Add(new RowDefinition());
                }

                // Clear existing content and set the new Grid
                VehiclesStackPanel.Children.Clear();
                VehiclesStackPanel.Children.Add(grid);
            }

            // Determine the column and row for this vehicle
            int column = slotIndex % 2;
            int row = slotIndex / 2;

            // Set the position of the GroupBox
            Grid.SetColumn(vehicleGroupBox, column);
            Grid.SetRow(vehicleGroupBox, row);

            grid.Children.Add(vehicleGroupBox);
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
                LoadVehicles(DealershipListBox.SelectedItem.ToString());
            }
        }

        private void RemoveVehicleFromSlot(string slotColumn)
        {
            string dealershipName = DealershipListBox.SelectedItem.ToString();
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                // Use the column name directly in the SQL query to set it to NULL
                string query = $"UPDATE houses SET {slotColumn} = NULL WHERE Name = @Name";
                    
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", dealershipName);
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

        private void IslandComboBox_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
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
