using System;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using TDU2_Track_Records.Properties;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq; // For .Take() method in LINQ

namespace TDU2_Track_Records
{
    public partial class dealerships : Window
    {
        readonly string connectionString = Settings.Default.connectionString;
        public string distance, speed;
        public string slotColumn;
        readonly string SI = Settings.Default.system;
        public int dealershipColumn = Settings.Default.dealershipColumns;
        private static double lastcardLeft;
        private static double lastcardTop;
        bool isUnlocked = false;
        public dealerships()
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


        private void LoadDealerships(string island)
        {
            DealershipListBox.Items.Clear();

            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM dealerships WHERE Island = @Island ORDER BY id ASC";
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
                VehiclesScrollViewer.Visibility = Visibility.Visible;
                VehiclesScrollViewer.ScrollToTop();
            }
        }

        private void LoadVehicles(string dealershipName)
        {
            VehiclesStackPanel.Children.Clear(); // Clear existing UI elements before loading new ones
            int slotIndex = 0;
            string island = IslandComboBox.SelectedValue?.ToString();

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
                            // Loop through all 20 slots
                            for (int i = 1; i <= 20; i++) // Increased from 8 to 20 slots
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


        //private void AddComboBoxToUI(string dealershipName, int slotIndex, string slotColumn)
        //{
        //    if (isUnlocked == true)
        //    {
        //        // Create the main GroupBox for the empty slot
        //        GroupBox emptySlotGroupBox = new GroupBox
        //        {
        //            Header = $"Empty Slot {slotIndex + 1}",
        //            Margin = new Thickness(10),
        //            BorderBrush = new SolidColorBrush(Color.FromRgb(150, 150, 150)),
        //            BorderThickness = new Thickness(2),
        //            Padding = new Thickness(10)
        //        };

        //        // Create a StackPanel to hold the ComboBox
        //        StackPanel contentStack = new StackPanel();

        //        // Create the ComboBox for selecting a car
        //        ComboBox availableCarsComboBox = new ComboBox
        //        {
        //            Width = 150,
        //            Tag = slotColumn // Store the actual slot column name in the Tag property
        //        };

        //        // Load the available cars into the ComboBox
        //        LoadAvailableCarsIntoComboBox(availableCarsComboBox);

        //        // Add a SelectionChanged event to handle car selection
        //        availableCarsComboBox.SelectionChanged += (sender, e) =>
        //        {
        //            if (availableCarsComboBox.SelectedValue != null)
        //            {
        //                string selectedVehicleId = availableCarsComboBox.SelectedValue.ToString();
        //                if (!string.IsNullOrEmpty(selectedVehicleId))
        //                {
        //                    AddVehicleToSlot(dealershipName, selectedVehicleId, slotColumn);

        //                    // Refresh the UI after adding the car
        //                    LoadVehicles(dealershipName);
        //                }
        //            }
        //        };

        //        // Add the ComboBox to the contentStack
        //        contentStack.Children.Add(availableCarsComboBox);

        //        // Set the content of the GroupBox
        //        emptySlotGroupBox.Content = contentStack;

        //        // Ensure VehiclesStackPanel is a Grid (you may need to adjust based on your layout)
        //        Grid grid = VehiclesStackPanel as Grid;
        //        if (grid == null)
        //        {
        //            // Initialize a new Grid if VehiclesStackPanel is not a Grid
        //            grid = new Grid();
        //            grid.ColumnDefinitions.Add(new ColumnDefinition());
        //            grid.ColumnDefinitions.Add(new ColumnDefinition());

        //            // Define rows based on the number of slots
        //            int rows = (8 + 1) / 2;
        //            for (int i = 0; i < rows; i++)
        //            {
        //                grid.RowDefinitions.Add(new RowDefinition());
        //            }

        //            // Clear existing content and set the new Grid
        //            VehiclesStackPanel.Children.Clear();
        //            VehiclesStackPanel.Children.Add(grid);
        //        }

        //        // Determine the column and row for this empty slot
        //        int column = slotIndex % 2;
        //        int row = slotIndex / 2;

        //        // Set the position of the GroupBox in the grid
        //        Grid.SetColumn(emptySlotGroupBox, column);
        //        Grid.SetRow(emptySlotGroupBox, row);

        //        // Add the GroupBox to the grid
        //        grid.Children.Add(emptySlotGroupBox);
        //    }
        //}
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
                int columns = dealershipColumn; // You can set this to any number of columns you want per row

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
            string island = IslandComboBox.SelectedValue?.ToString();
            string column = "";
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
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                if (island == "Ibiza") {
                    column = "ibiza";
                }
                else
                {
                    column = "hawaii";
                }
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
                                VehicleBrand = reader["_brand_name"].ToString(),
                                VehicleModel = reader["_modelfull_name"].ToString(),
                                VehiclePrice = reader["_price"].ToString(),
                                VehicleCategory = reader["_vehiclecategory_name"].ToString(),
                                VehicleLevel = reader["_upgrade_level"].ToString(),
                                VehicleOwned = Convert.ToBoolean(reader["_is_owned"].ToString()),
                                IsPurchasable = Convert.ToBoolean(reader["_is_purchasable"]),
                                IsReward = Convert.ToBoolean(reader["_is_reward"]),
                                // Check if the "Image" column is DBNull before casting
                                VehicleImage = reader["_vehicle_image"] != DBNull.Value ? (byte[])reader["_vehicle_image"] : null

                            };
                        }
                    }
                }
            }
            return vehicle;
        }


        //private void AddVehicleToUI(VehicleManagement vehicle, int slotIndex, string slotColumn)
        //{
        //    // Create the main GroupBox for the vehicle (existing code)
        //    GroupBox vehicleGroupBox = new GroupBox
        //    {
        //        Header = vehicle.VehicleName, // Set the vehicle name as the header
        //        Margin = new Thickness(10),
        //        BorderBrush = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
        //        BorderThickness = new Thickness(2),
        //        Padding = new Thickness(10)
        //    };

        //    // Create a StackPanel to hold all vehicle information
        //    StackPanel contentStack = new StackPanel();

        //    // Add the vehicle image
        //    Image vehicleImage = new Image
        //    {
        //        Width = 300,
        //        Source = LoadImage(vehicle.VehicleImage) // Convert byte[] to ImageSource
        //    };
        //    contentStack.Children.Add(vehicleImage);

        //    // Create a StackPanel for the vehicle info
        //    StackPanel infoStack = new StackPanel
        //    {
        //        Orientation = Orientation.Horizontal,
        //        Margin = new Thickness(10),
        //        HorizontalAlignment = HorizontalAlignment.Center

        //    };

        //    // Vehicle Price
        //    GroupBox priceBox = new GroupBox
        //    {
        //        Header = "Price $",
        //    };
        //    priceBox.Content = new TextBlock
        //    {
        //        Text = vehicle.VehiclePrice.ToString(),
        //        HorizontalAlignment = HorizontalAlignment.Center,
        //        FontSize = 18
        //    };
        //    infoStack.Children.Add(priceBox);

        //    // Vehicle Class
        //    GroupBox classBox = new GroupBox
        //    {
        //        Header = "Class",
        //    };
        //    Image classImage = new Image
        //    {
        //        Width = 32,
        //        Height = 32,
        //        Source = new BitmapImage(new Uri("/Images/carClasses/" + vehicle.VehicleCategory + ".png", UriKind.Relative))
        //    };
        //    classBox.Content = classImage;
        //    infoStack.Children.Add(classBox);

        //    // Vehicle Status
        //    GroupBox statusBox = new GroupBox
        //    {
        //        Header = "Status",
        //    };
        //    CheckBox ownedCheckBox = new CheckBox
        //    {
        //        IsChecked = vehicle.VehicleOwned == true,
        //        VerticalAlignment = VerticalAlignment.Center
        //    };
        //    TextBlock ownedText = new TextBlock
        //    {
        //        Text = "Owned",
        //        FontSize = 16,
        //        VerticalAlignment = VerticalAlignment.Center
        //    };

        //    DockPanel statusPanel = new DockPanel();
        //    statusPanel.Children.Add(ownedCheckBox);
        //    statusPanel.Children.Add(ownedText);
        //    statusBox.Content = statusPanel;
        //    infoStack.Children.Add(statusBox);

        //    // Add a button to open the vehicle card window
        //    Button viewDetailsButton = new Button
        //    {
        //        Content = "Card",
        //        VerticalAlignment = VerticalAlignment.Center,
        //        Margin = new Thickness(10),
        //        Tag = vehicle.id // Store the vehicle object in the button's Tag
        //    };
        //    viewDetailsButton.Click += ViewDetailsButton_Click;
        //    infoStack.Children.Add(viewDetailsButton);

        //    if(isUnlocked == true) { 
        //    // Add a delete button for each vehicle and store the column name in the Tag property
        //    Button deleteButton = new Button
        //    {
        //        Content = "-",
        //        Margin = new Thickness(10),
        //        Tag = slotColumn // Store the actual column name (e.g., "Slot1_VehicleId")
        //    };
        //    deleteButton.Click += DeleteVehicleButton_Click;

        //    // Add the delete button to the infoStack
        //    infoStack.Children.Add(deleteButton);
        //    }
        //    // Add the infoStack to contentStack
        //    contentStack.Children.Add(infoStack);

        //    // Set the content of the GroupBox
        //    vehicleGroupBox.Content = contentStack;

        //    // Ensure VehiclesStackPanel is a Grid
        //    Grid grid = VehiclesStackPanel as Grid;
        //    if (grid == null)
        //    {
        //        // Initialize a new Grid if VehiclesStackPanel is not a Grid
        //        grid = new Grid();

        //        // Define two columns in the Grid
        //        grid.ColumnDefinitions.Add(new ColumnDefinition());
        //        grid.ColumnDefinitions.Add(new ColumnDefinition());

        //        // Define rows based on the number of vehicles
        //        int rows = (8 + 1) / 2; // 8 slots, 2 columns per row
        //        for (int i = 0; i < rows; i++)
        //        {
        //            grid.RowDefinitions.Add(new RowDefinition());
        //        }

        //        // Clear existing content and set the new Grid
        //        VehiclesStackPanel.Children.Clear();
        //        VehiclesStackPanel.Children.Add(grid);
        //    }

        //    // Determine the column and row for this vehicle
        //    int column = slotIndex % 2;
        //    int row = slotIndex / 2;

        //    // Set the position of the GroupBox
        //    Grid.SetColumn(vehicleGroupBox, column);
        //    Grid.SetRow(vehicleGroupBox, row);

        //    grid.Children.Add(vehicleGroupBox);
        //}
        private void AddVehicleToUI(VehicleManagement vehicle, int slotIndex, string slotColumn)
        {
            // Create the main GroupBox for the vehicle
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
                HorizontalAlignment = HorizontalAlignment.Center
            };
            if (vehicle.IsPurchasable)
            {
                // Vehicle Price
                GroupBox priceBox = new GroupBox
                {
                    Header = "Price $",
                };
                priceBox.Content = new TextBlock
                {
                    Text = vehicle.VehiclePrice.ToString(),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    FontSize = 18
                };
                infoStack.Children.Add(priceBox);
            }
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
                IsChecked = vehicle.VehicleOwned == true,
                VerticalAlignment = VerticalAlignment.Center
            }; 
            // Add event handlers for CheckBox checked and unchecked events
            ownedCheckBox.Checked += (sender, e) =>
            {
                UpdateVehicleOwnedStatus(vehicle.id, true);  // Update database when checked
            };

            ownedCheckBox.Unchecked += (sender, e) =>
            {
                UpdateVehicleOwnedStatus(vehicle.id, false);  // Update database when unchecked
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

            // Add a button to open the vehicle card window
            Button viewDetailsButton = new Button
            {
                Content = "Card",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10),
                Tag = vehicle.id // Store the vehicle ID in the button's Tag
            };
            viewDetailsButton.Click += ViewDetailsButton_Click;
            infoStack.Children.Add(viewDetailsButton);

            // If unlocked, add a delete button for each vehicle
            if (isUnlocked)
            {
                Button deleteButton = new Button
                {
                    Content = "Remove",
                    Margin = new Thickness(5),
                    VerticalAlignment = VerticalAlignment.Center,
                    Tag = slotColumn // Store the actual column name (e.g., "Slot1_VehicleId")
                    
                };
                deleteButton.Click += DeleteVehicleButton_Click;
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
                grid = new Grid();
                VehiclesStackPanel.Children.Clear();
                VehiclesStackPanel.Children.Add(grid);
            }

            // Determine the number of columns dynamically
            int columns = dealershipColumn; // You can set this to any number of columns you want per row

            // Add columns dynamically if needed
            while (grid.ColumnDefinitions.Count < columns)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            // Calculate the number of rows needed based on the number of vehicles
            int totalVehicles = slotIndex + 1;
            int rows = (totalVehicles + columns - 1) / columns;

            // Add rows dynamically if needed
            while (grid.RowDefinitions.Count < rows)
            {
                grid.RowDefinitions.Add(new RowDefinition());
            }

            // Determine the column and row for this vehicle
            int column = slotIndex % columns;
            int row = slotIndex / columns;

            // Set the position of the GroupBox
            Grid.SetColumn(vehicleGroupBox, column);
            Grid.SetRow(vehicleGroupBox, row);

            // Add the vehicleGroupBox to the grid
            grid.Children.Add(vehicleGroupBox);
        }
        // Method to update the vehicle owned status in the database
        private void UpdateVehicleOwnedStatus(int vehicleId, bool isOwned)
        {
            // SQL update command to update the 'VehicleOwned' column in the database
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
                string query = $"UPDATE dealerships SET {slotColumn} = NULL WHERE Name = @Name";

                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
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
            LoadVehicles(DealershipListBox.SelectedItem.ToString());
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
