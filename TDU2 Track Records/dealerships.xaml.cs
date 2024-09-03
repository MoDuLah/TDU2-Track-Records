using System;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using TDU2_Track_Records.Properties;
using System.Windows.Input;

namespace TDU2_Track_Records
{
    public partial class dealerships : Window
    {
        readonly string connectionString = Settings.Default.connectionString;
        public string distance, speed;
        readonly string SI = Settings.Default.system;

        public dealerships()
        {
            InitializeComponent();
        }

        private void IslandComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedIsland = IslandComboBox.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(selectedIsland))
            {
                LoadDealerships(selectedIsland);
            }
        }


        private void LoadDealerships(string island)
        {
            DealershipListBox.Items.Clear();

            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM dealerships WHERE Island = @Island";
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Island", island);
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
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
            }
        }

        private void LoadVehicles(string dealershipName)
        {
            VehiclesStackPanel.Children.Clear();
            int slotIndex = 0;

            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM dealerships WHERE Name = @Name";
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", dealershipName);
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            for (int i = 1; i <= 8; i++)
                            {
                                string vehicleId = reader[$"slot{i}_vehicleId"].ToString();
                                if (!string.IsNullOrEmpty(vehicleId))
                                {
                                    var vehicle = GetVehicleDetails(vehicleId);
                                    if (vehicle != null)
                                    {
                                        AddVehicleToUI(vehicle, slotIndex);
                                        slotIndex++;
                                    }
                                }
                            }
                        }
                    }
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
                                Id = Convert.ToInt32(reader["Id"]),
                                Name = reader["Name"].ToString(),
                                Brand = reader["Brand"].ToString(),
                                Model = reader["Model"].ToString(),
                                Price = reader["Price"].ToString(),
                                Class = reader["Class"].ToString(),
                                // Check if the "Image" column is DBNull before casting
                                Image = reader["Image"] != DBNull.Value ? (byte[])reader["Image"] : null
                            };
                        }
                    }
                }
            }
            return vehicle;
        }


        //private void AddVehicleToUI(VehicleManagement vehicle)
        //{
        //    // Create the main GroupBox for the vehicle
        //    GroupBox vehicleGroupBox = new GroupBox
        //    {
        //        Header = vehicle.Name, // Set the vehicle name as the header
        //        Margin = new Thickness(10),
        //        BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(100, 100, 100)),
        //        BorderThickness = new Thickness(2),
        //        Padding = new Thickness(10)
        //    };

        //    // Create a StackPanel to hold all vehicle information
        //    StackPanel contentStack = new StackPanel();

        //    // Add the vehicle image
        //    Image vehicleImage = new Image
        //    {
        //        Width = 300,
        //        Source = LoadImage(vehicle.Image) // Convert byte[] to ImageSource
        //    };
        //    contentStack.Children.Add(vehicleImage);

        //    // Create a StackPanel for the vehicle info
        //    StackPanel infoStack = new StackPanel
        //    {
        //        Orientation = Orientation.Horizontal,
        //        HorizontalAlignment = HorizontalAlignment.Center
        //    };

        //    // Vehicle Price
        //    GroupBox priceBox = new GroupBox
        //    {
        //        Header = "Price $",
        //        Width = 75
        //    };
        //    priceBox.Content = new TextBlock
        //    {
        //        Text = vehicle.Price,
        //        HorizontalAlignment = HorizontalAlignment.Center,
        //        FontSize = 18
        //    };
        //    infoStack.Children.Add(priceBox);

        //    // Vehicle Class
        //    GroupBox classBox = new GroupBox
        //    {
        //        Header = "Class",
        //        Width = 75
        //    };
        //    Image classImage = new Image
        //    {
        //        Width = 60,
        //        Source = new BitmapImage(new Uri("/Images/carClasses/" + vehicle.Class + ".png", UriKind.Relative))
        //    };
        //    classBox.Content = classImage;
        //    infoStack.Children.Add(classBox);

        //    // Vehicle Status
        //    GroupBox statusBox = new GroupBox
        //    {
        //        Header = "Status",
        //        Width = 75
        //    };
        //    CheckBox ownedCheckBox = new CheckBox
        //    {
        //        IsChecked = vehicle.Owned == 1,
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

        //    // Add the infoStack to contentStack
        //    contentStack.Children.Add(infoStack);

        //    // Set the content of the GroupBox
        //    vehicleGroupBox.Content = contentStack;

        //    // Add the GroupBox to the main StackPanel
        //    VehiclesStackPanel.Children.Add(vehicleGroupBox);
        //}
        private void AddVehicleToUI(VehicleManagement vehicle, int slotIndex)
        {
            // Create the main GroupBox for the vehicle
            GroupBox vehicleGroupBox = new GroupBox
            {
                Header = vehicle.Name, // Set the vehicle name as the header
                Margin = new Thickness(10),
                BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(100, 100, 100)),
                BorderThickness = new Thickness(2),
                Padding = new Thickness(10)
            };

            // Create a StackPanel to hold all vehicle information
            StackPanel contentStack = new StackPanel();

            // Add the vehicle image
            Image vehicleImage = new Image
            {
                Width = 300,
                Source = LoadImage(vehicle.Image) // Convert byte[] to ImageSource
            };
            contentStack.Children.Add(vehicleImage);

            // Create a StackPanel for the vehicle info
            StackPanel infoStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // Vehicle Price
            GroupBox priceBox = new GroupBox
            {
                Header = "Price $",
                Width = 75
            };
            priceBox.Content = new TextBlock
            {
                Text = vehicle.Price,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 18
            };
            infoStack.Children.Add(priceBox);

            // Vehicle Class
            GroupBox classBox = new GroupBox
            {
                Header = "Class",
                Width = 75
            };
            Image classImage = new Image
            {
                Width = 60,
                Source = new BitmapImage(new Uri("/Images/carClasses/" + vehicle.Class + ".png", UriKind.Relative))
            };
            classBox.Content = classImage;
            infoStack.Children.Add(classBox);

            // Vehicle Status
            GroupBox statusBox = new GroupBox
            {
                Header = "Status",
                Width = 75
            };
            CheckBox ownedCheckBox = new CheckBox
            {
                IsChecked = vehicle.Owned == 1,
                VerticalAlignment = VerticalAlignment.Center
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



        private BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0)
            {
                // Return a placeholder image if imageData is null or empty
                var placeholderImage = new BitmapImage();
                placeholderImage.BeginInit();
                placeholderImage.UriSource = new Uri("pack://application:,,,/Images/Offline.png", UriKind.Absolute);
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


        private void dealerships_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

    }
}
