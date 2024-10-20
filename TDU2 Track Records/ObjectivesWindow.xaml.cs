using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using TDU2_Track_Records.Properties;
using System.Windows.Controls.Primitives;

namespace TDU2_Track_Records
{
    public partial class ObjectivesWindow : Window
    {
        // Initialize variables to hold points
        public int competitionMaxPoints = 0, socialMaxPoints = 0, discoveryMaxPoints = 0, collectionMaxPoints = 0;
        public int competitionCurrentPoints = 0, socialCurrentPoints = 0, discoveryCurrentPoints = 0, collectionCurrentPoints = 0;
        public int globalMaxPoints = 0, globalCurrentPoints = 0;
        // Dictionary to hold category progress (category => (currentPoints, maxPoints))
        public Dictionary<string, (int currentPoints, int maxPoints)> categoryProgress = new Dictionary<string, (int currentPoints, int maxPoints)>();

        // Dictionary to hold subcategory progress (subcategory => (currentPoints, maxPoints))
        public Dictionary<string, (int currentPoints, int maxPoints)> subcategoryProgress = new Dictionary<string, (int currentPoints, int maxPoints)>();

        readonly string connectionString = Settings.Default.connectionString;
        public string category, subcategory;

        public ObjectivesWindow()
        {
            InitializeComponent();
            LoadProgressFromDatabase();
            UpdatePointsAndRecalculateProgress();

            LoadSubCategories(Tab1.Text, Tab1Type.Text, Tab1SubCategoryGrid);
            LoadSubCategories(Tab2.Text, Tab2Type.Text, Tab2SubCategoryGrid);
            LoadSubCategories(Tab3.Text, Tab3Type.Text, Tab3SubCategoryGrid);
            LoadSubCategories(Tab4.Text, Tab4Type.Text, Tab4SubCategoryGrid);
            LoadSubCategories(Tab5.Text, Tab5Type.Text, Tab5SubCategoryGrid);
            LoadSubCategories(Tab6.Text, Tab6Type.Text, Tab6SubCategoryGrid);
            LoadSubCategories(Tab7.Text, Tab7Type.Text, Tab7SubCategoryGrid);
            LoadSubCategories(Tab8.Text, Tab8Type.Text, Tab8SubCategoryGrid);
        }

        private void LoadPointsIntoDictionaries()
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // Populate categoryProgress dictionary
                string categoryQuery = @"SELECT ParentCategory, SUM(Points) AS CurrentPoints, SUM(MaximumPoints) AS MaxPoints 
                                 FROM objectives_list 
                                 GROUP BY ParentCategory;";
                using (var command = new SQLiteCommand(categoryQuery, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string category = reader.GetString(0);
                            int currentPoints = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                            int maxPoints = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                            categoryProgress[category] = (currentPoints, maxPoints);
                        }
                    }
                }

                // Populate subcategoryProgress dictionary
                string subcategoryQuery = @"SELECT Subcategory, SUM(Points) AS CurrentPoints, SUM(MaximumPoints) AS MaxPoints 
                                    FROM objectives_list 
                                    GROUP BY Subcategory;";
                using (var command = new SQLiteCommand(subcategoryQuery, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string subcategory = reader.GetString(0);
                            int currentPoints = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                            int maxPoints = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                            subcategoryProgress[subcategory] = (currentPoints, maxPoints);
                        }
                    }
                }
            }
        }
        private void LoadProgressFromDatabase()
        {

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // Query to get the maximum points for each subcategory
                string maxPointsQuery = @"SELECT 
                                       ParentCategory, 
                                       SUM(MaximumPoints) AS TotalPoints
                                       FROM objectives_list
                                       GROUP BY ParentCategory;";

                using (var command = new SQLiteCommand(maxPointsQuery, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string parentCategory = reader.GetString(0);
                            int totalPoints = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);

                            // Assign the total points based on the parent category
                            switch (parentCategory)
                            {
                                case "Competition":
                                    competitionMaxPoints = totalPoints;
                                    break;
                                case "Social":
                                    socialMaxPoints = totalPoints;
                                    break;
                                case "Discovery":
                                    discoveryMaxPoints = totalPoints;
                                    break;
                                case "Collection":
                                    collectionMaxPoints = totalPoints;
                                    break;
                            }
                        }
                    }
                }

                // Query to get the current points for each subcategory
                string currentPointsQuery = @"SELECT 
                                       ParentCategory, 
                                       SUM(Points) AS TotalPoints
                                       FROM objectives_list
                                       GROUP BY ParentCategory";

                using (var command = new SQLiteCommand(currentPointsQuery, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string parentCategory = reader.GetString(0);
                            int points = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);

                            // Assign the current points based on the parent category
                            switch (parentCategory)
                            {
                                case "Competition":
                                    competitionCurrentPoints = points;
                                    break;
                                case "Social":
                                    socialCurrentPoints = points;
                                    break;
                                case "Discovery":
                                    discoveryCurrentPoints = points;
                                    break;
                                case "Collection":
                                    collectionCurrentPoints = points;
                                    break;
                            }
                        }
                    }
                }

                // Query to get global points
                string globalMaxPointsQuery = @"SELECT SUM(MaximumPoints) AS TotalPoints FROM objectives_list;";
                using (var command = new SQLiteCommand(globalMaxPointsQuery, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            globalMaxPoints = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                        }
                    }
                }

                // Query to get global current points
                string globalCurrentPointsQuery = @"SELECT SUM(Points) AS TotalPoints FROM objectives_list;";
                using (var command = new SQLiteCommand(globalCurrentPointsQuery, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            globalCurrentPoints = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                        }
                    }
                }

                // Calculate and update progress
                competitionProgress.Progress = competitionMaxPoints > 0 ? Math.Round((competitionCurrentPoints * 100.0 / competitionMaxPoints),0) : 0;
                socialProgress.Progress = socialMaxPoints > 0 ? Math.Round((socialCurrentPoints * 100.0 / socialMaxPoints),0) : 0;
                discoveryProgress.Progress = discoveryMaxPoints > 0 ? Math.Round((discoveryCurrentPoints * 100.0 / discoveryMaxPoints), 0) : 0;
                collectionProgress.Progress = collectionMaxPoints > 0 ? Math.Round((collectionCurrentPoints * 100.0 / collectionMaxPoints), 0) : 0;
                globalLevelProgress.Progress = globalMaxPoints > 0 ? Math.Round((globalCurrentPoints * 100.0 / globalMaxPoints), 0) : 0;
            }
        }
        private void UpdatePointsAndRecalculateProgress(string subcategoryName)
        {
            // After updating an objective, recalculate the points and progress for the subcategory and category
            LoadPointsIntoDictionaries();  // Recalculate dictionaries

            // Assuming the category is known here, update the UI for the subcategory
            foreach (UIElement child in Tab1SubCategoryGrid.Children) // Replace with the relevant tab's grid
            {
                if (child is Border card && card.Tag is string tag && tag == subcategoryName)
                {
                    var grid = (Grid)card.Child;
                    var middleStackPanel = (StackPanel)grid.Children[1];
                    // Update points and progress bar for the subcategory
                    int currentPoints = subcategoryProgress[subcategoryName].currentPoints;
                    int maxPoints = subcategoryProgress[subcategoryName].maxPoints;
                    double progress = maxPoints > 0 ? (double)currentPoints / maxPoints * 100 : 0;

                    var pointsTextBlock = (TextBlock)middleStackPanel.Children[1]; // Assuming the points text is at index 1
                    pointsTextBlock.Text = $"Points: {currentPoints} / {maxPoints}";

                    var progressBar = (ProgressBar)middleStackPanel.Children[2]; // Assuming progress bar is at index 2
                    progressBar.Value = progress;
                }
            }
        }

        private void LoadSubCategories(string parentCategory, string type, UniformGrid targetGrid)
        {
            targetGrid.Children.Clear();  // Clear previous items

            string query = @"SELECT Icon, SubcategoryName, StatusCheck 
                             FROM objectives_subcategories 
                             WHERE ParentCategory = @ParentCategory AND Type = @Type";

            List<UIElement> cards = new List<UIElement>();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ParentCategory", parentCategory);
                    command.Parameters.AddWithValue("@Type", type);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var subCategoryCard = CreateSubCategoryCard(
                                reader.GetString(0),  // Icon path
                                reader.GetString(1),  // Subcategory Name
                                reader.GetString(2)); // StatusCheck

                            cards.Add(subCategoryCard);
                        }
                    }
                }
            }
            int cardCount = cards.Count;
            int emptyCardCount = 6 - cardCount;

            for (int i = 0; i < emptyCardCount; i++)
            {
                var emptyCard = CreateEmptySubCategoryCard();
                cards.Add(emptyCard);
            }

            foreach (var card in cards)
            {
                targetGrid.Children.Add(card);
            }
        }

        private UIElement CreateEmptySubCategoryCard()
        {
            var card = new Border
            {
                Visibility = Visibility.Hidden,
                Margin = new Thickness(5),
                Padding = new Thickness(10),
                Background = new SolidColorBrush(Color.FromRgb(42, 42, 42)),
                CornerRadius = new CornerRadius(5),
                BorderBrush = new SolidColorBrush(Color.FromRgb(85, 85, 85)),
                BorderThickness = new Thickness(1),
                Child = new StackPanel() // Empty stack panel
            };

            return card;
        }

        private UIElement CreateSubCategoryCard(string iconFileName, string subcategoryName, string status)
        {
            string iconPath = $"/Images/ico/objectives/{iconFileName}";

            var card = new Border
            {
                Margin = new Thickness(5),
                Padding = new Thickness(10),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF161B1E")),
                CornerRadius = new CornerRadius(5),
                BorderBrush = new SolidColorBrush(Color.FromRgb(85, 85, 85)),
                BorderThickness = new Thickness(1)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });

            var image = new Image
            {
                Source = new BitmapImage(new Uri(iconPath, UriKind.RelativeOrAbsolute)),
                Width = 77,
                Height = 77,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0)
            };
            Grid.SetColumn(image, 0);
            grid.Children.Add(image);

            var middleStackPanel = new StackPanel { VerticalAlignment = VerticalAlignment.Center };

            // Fetch subcategory points and progress from the dictionary
            int currentPoints = 0, maxPoints = 0;
            double progress = 0;

            if (subcategoryProgress.ContainsKey(subcategoryName))
            {
                currentPoints = subcategoryProgress[subcategoryName].currentPoints;
                maxPoints = subcategoryProgress[subcategoryName].maxPoints;
                progress = maxPoints > 0 ? (double)currentPoints / maxPoints * 100 : 0;
            }

            // Display Subcategory Name
            var nameTextBlock = new TextBlock
            {
                Text = subcategoryName,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                FontSize = 16,
                Margin = new Thickness(0, 0, 0, 5)
            };
            middleStackPanel.Children.Add(nameTextBlock);

            // Display CurrentPoints / MaxPoints
            var pointsTextBlock = new TextBlock
            {
                Text = $"Points: {currentPoints} / {maxPoints}",
                Foreground = Brushes.LightGray,
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 5)
            };
            middleStackPanel.Children.Add(pointsTextBlock);

            // Create a grid for progress bar and text
            var progressGrid = new Grid();

            // Progress bar
            var progressBar = new ProgressBar
            {
                Minimum = 0,
                Maximum = 100,
                Height = 15,
                Value = progress,
                Foreground = Brushes.Green,
                Background = new SolidColorBrush(Color.FromRgb(42, 42, 42)),
                Padding = new Thickness(2,2,2,2),
                Margin = new Thickness(0, 5, 0, 5)
            };
            progressGrid.Children.Add(progressBar);

            // TextBlock overlay for progress percentage
            var progressTextBlock = new TextBlock
            {
                Text = $"{progress:F0}%",  // Display progress percentage
                Foreground = Brushes.White,
                FontSize = 12,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.Bold
            };
            progressGrid.Children.Add(progressTextBlock);

            // Add the progress grid to the middle stack panel
            middleStackPanel.Children.Add(progressGrid);

            Grid.SetColumn(middleStackPanel, 1);
            grid.Children.Add(middleStackPanel);

            // Add a checkbox for status
            var checkBox = new CheckBox
            {
                IsChecked = status.ToLower() == "true",
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                IsHitTestVisible = false,
                Focusable = false
            };
            Grid.SetColumn(checkBox, 2);
            grid.Children.Add(checkBox);

            card.Child = grid;
            card.Tag = subcategoryName;
            card.MouseLeftButtonUp += SubCategoryCard_Click;

            return card;
        }

        private void SubCategoryCard_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Border clickedCard && clickedCard.Tag is string subcategoryName)
            {
                var selectedTab = ObjectivesTabControl.SelectedItem as TabItem;

                if (selectedTab != null)
                {
                    string category = null;
                    string subcategory = null;

                    if (selectedTab == Tab1Grid.Parent as TabItem)
                    {
                        category = Tab1.Text;
                        subcategory = Tab1Type.Text;
                    }
                    else if (selectedTab == Tab2Grid.Parent as TabItem)
                    {
                        category = Tab2.Text;
                        subcategory = Tab2Type.Text;
                    }
                    else if (selectedTab == Tab3Grid.Parent as TabItem)
                    {
                        category = Tab3.Text;
                        subcategory = Tab3Type.Text;
                    }
                    else if (selectedTab == Tab4Grid.Parent as TabItem)
                    {
                        category = Tab4.Text;
                        subcategory = Tab4Type.Text;
                    }
                    else if (selectedTab == Tab5Grid.Parent as TabItem)
                    {
                        category = Tab5.Text;
                        subcategory = Tab5Type.Text;
                    }
                    else if (selectedTab == Tab6Grid.Parent as TabItem)
                    {
                        category = Tab6.Text;
                        subcategory = Tab6Type.Text;
                    }
                    else if (selectedTab == Tab7Grid.Parent as TabItem)
                    {
                        category = Tab7.Text;
                        subcategory = Tab7Type.Text;
                    }
                    else if (selectedTab == Tab8Grid.Parent as TabItem)
                    {
                        category = Tab8.Text;
                        subcategory = Tab8Type.Text;
                    }

                    if (!string.IsNullOrEmpty(category) && !string.IsNullOrEmpty(subcategory))
                    {
                        LoadObjectivesForSubCategory(category, subcategoryName);
                    }
                    else
                    {
                        MessageBox.Show("Category or Subcategory not found.");
                    }
                }
            }
        }

        private void LoadObjectivesForSubCategory(string category, string subcategoryName)
        {
            ObjectiveScrollViewerContent.Children.Clear();

            string query = @"SELECT id, Status, Description, Island, Points, MaximumPoints
                     FROM objectives_list
                     WHERE ParentCategory = @ParentCategory AND Subcategory = @SubcategoryName";

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ParentCategory", category);
                    command.Parameters.AddWithValue("@SubcategoryName", subcategoryName);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var objectiveCard = CreateObjectiveCard(
                                reader.GetInt32(0),   // Objective ID (INTEGER)
                                reader.IsDBNull(1) ? "false" : reader.GetString(1),  // Status (TEXT) (Check for null)
                                reader.IsDBNull(2) ? "" : reader.GetString(2),  // Description (TEXT)
                                reader.IsDBNull(3) ? "" : reader.GetString(3),  // Island (TEXT)
                                reader.GetInt32(4),   // CurrentPoints (INTEGER)
                                reader.GetInt32(5));  // MaximumPoints (INTEGER)

                            ObjectiveScrollViewerContent.Children.Add(objectiveCard);
                        }
                    }
                }
            }
        }
        private UIElement CreateObjectiveCard(int objectiveId, string status, string description, string island, int currentPoints, int maximumPoints)
        {
            var card = new Border
            {
                Margin = new Thickness(5),
                Padding = new Thickness(10),
                Background = new SolidColorBrush(Color.FromRgb(42, 42, 42)),
                CornerRadius = new CornerRadius(5),
                BorderBrush = new SolidColorBrush(Color.FromRgb(85, 85, 85)),
                BorderThickness = new Thickness(1)
            };

            var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };

            var statusCheckBox = new CheckBox
            {
                Margin = new Thickness(5),
                IsChecked = Convert.ToBoolean(status)
            };
            statusCheckBox.Checked += (s, e) =>
            {
                UpdateStatusInDatabase(objectiveId, true, maximumPoints);  // Checked -> true
                UpdatePointsAndRecalculateProgress();                       // Recalculate progress for subcategories
            };
            statusCheckBox.Unchecked += (s, e) =>
            {
                UpdateStatusInDatabase(objectiveId, false, maximumPoints);  // Unchecked -> false
                UpdatePointsAndRecalculateProgress();                        // Recalculate progress for subcategories
            };
            stackPanel.Children.Add(statusCheckBox);

            var descriptionTextBlock = new TextBlock
            {
                Text = description,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 14,
                Margin = new Thickness(0, 0, 5, 0)
            };
            stackPanel.Children.Add(descriptionTextBlock);

            var islandTextBlock = new TextBlock
            {
                Text = island,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 0)
            };
            stackPanel.Children.Add(islandTextBlock);

            var pointsTextBlock = new TextBlock
            {
                Text = $"| Points: {currentPoints} / {maximumPoints}",
                Foreground = Brushes.LightGray,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 14,
                Margin = new Thickness(5, 0, 5, 0)
            };
            stackPanel.Children.Add(pointsTextBlock);

            card.Child = stackPanel;
            // Add click event to toggle the checkbox when the card is clicked
            card.MouseLeftButtonUp += (s, e) =>
            {
                // Toggle the checkbox state
                statusCheckBox.IsChecked = !statusCheckBox.IsChecked;
            };
            return card;
        }
        private void UpdatePointsAndRecalculateProgress()
        {
            // Recalculate dictionaries
            LoadPointsIntoDictionaries();
            LoadProgressFromDatabase();

            // Loop through all tabs to update progress for each subcategory
            foreach (TabItem tab in ObjectivesTabControl.Items)
            {
                // Get the grid for the current tab
                if (tab.Content is Grid tabGrid)
                {
                    foreach (UIElement element in tabGrid.Children)
                    {
                        if (element is UniformGrid subCategoryGrid)
                        {
                            // Loop through each card in the current tab's grid
                            foreach (UIElement child in subCategoryGrid.Children)
                            {
                                if (child is Border card && card.Tag is string tag && subcategoryProgress.ContainsKey(tag))
                                {
                                    var grid = (Grid)card.Child;
                                    var middleStackPanel = (StackPanel)grid.Children[1];

                                    // Update points and progress bar for the subcategory
                                    var progressInfo = subcategoryProgress[tag];
                                    int currentPoints = progressInfo.currentPoints;
                                    int maxPoints = progressInfo.maxPoints;
                                    double progress = maxPoints > 0 ? (double)currentPoints / maxPoints * 100 : 0;

                                    // Update the points text
                                    var pointsTextBlock = (TextBlock)middleStackPanel.Children[1]; // Points text block assumed to be at index 1
                                    pointsTextBlock.Text = $"Points: {currentPoints} / {maxPoints}";

                                    // Find the progress bar grid
                                    var progressGrid = (Grid)middleStackPanel.Children[2]; // Assuming the grid with the progress bar and text is at index 2

                                    // Update the progress bar
                                    var progressBar = (ProgressBar)progressGrid.Children[0]; // Assuming ProgressBar is the first child of the progress grid
                                    progressBar.Value = progress;

                                    // Update the progress percentage text
                                    var progressTextBlock = (TextBlock)progressGrid.Children[1]; // Assuming the percentage text block is the second child
                                    progressTextBlock.Text = $"{progress:F0}%"; // Display progress as a percentage

                                    // Check if all objectives are completed
                                    if (AllObjectivesCompleted(tag))
                                    {
                                        // Find the checkbox and set it to checked
                                        var checkBox = (CheckBox)grid.Children[2]; // Assuming the checkbox is the third child
                                        checkBox.IsChecked = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool AllObjectivesCompleted(string subcategoryName)
        {
            // Query to check if all objectives in the subcategory are complete
            string query = @"SELECT COUNT(*) FROM objectives_list 
                     WHERE Subcategory = @Subcategory AND Status = 'false'";

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Subcategory", subcategoryName);
                    int incompleteCount = Convert.ToInt32(command.ExecuteScalar());

                    // If incompleteCount is 0, it means all objectives are complete
                    return incompleteCount == 0;
                }
            }
        }


        private void UpdateStatusInDatabase(int objectiveId, bool status, int maximumPoints)
        {
            string updateObjectiveQuery;
            if (status)
            {
                updateObjectiveQuery = $"UPDATE objectives_list SET Points = {maximumPoints}, Status = 'true' WHERE id = {objectiveId}; ";
            }
            else
            {
                updateObjectiveQuery = $"UPDATE objectives_list SET Points = 0, Status = 'false' WHERE id = {objectiveId};";
            }

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (var command = new SQLiteCommand(updateObjectiveQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }

            // Update the subcategory checkbox after changing the objective status
            UpdateSubCategoryCheckboxStatus(objectiveId);
        }

        private void UpdateSubCategoryCheckboxStatus(int objectiveId)
        {
            // Query to find the subcategory name based on the objective ID
            string subcategoryQuery = @"SELECT Subcategory FROM objectives_list WHERE id = @ObjectiveId";

            string subcategoryName = string.Empty;

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (var command = new SQLiteCommand(subcategoryQuery, connection))
                {
                    command.Parameters.AddWithValue("@ObjectiveId", objectiveId);
                    subcategoryName = (string)command.ExecuteScalar();
                }
            }

            // Check the status of all objectives in the subcategory
            bool allObjectivesCompleted = true;
            string statusQuery = @"SELECT Status FROM objectives_list WHERE Subcategory = @SubcategoryName";

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (var command = new SQLiteCommand(statusQuery, connection))
                {
                    command.Parameters.AddWithValue("@SubcategoryName", subcategoryName);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader.GetString(0) == "false")
                            {
                                allObjectivesCompleted = false;
                                break;
                            }
                        }
                    }
                }
            }

            // Update the checkbox status
            foreach (var tab in ObjectivesTabControl.Items)
            {
                if (tab is TabItem tabItem && tabItem.Content is Grid tabGrid)
                {
                    foreach (UIElement element in tabGrid.Children)
                    {
                        if (element is UniformGrid subCategoryGrid)
                        {
                            foreach (UIElement child in subCategoryGrid.Children)
                            {
                                if (child is Border card && card.Tag is string tag && tag == subcategoryName)
                                {
                                    var grid = (Grid)card.Child;
                                    var checkBox = (CheckBox)grid.Children[2]; // Assuming the checkbox is at index 2
                                    checkBox.IsChecked = allObjectivesCompleted;
                                }
                            }
                        }
                    }
                }
            }
        }


        private void ObjectivesTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ObjectivesTabControl.SelectedIndex == 0)
            {
                ObjectiveScrollViewer.Visibility = Visibility.Collapsed;
            }
            else
            {
                ObjectiveScrollViewer.Visibility = Visibility.Visible;
            }
            ObjectiveScrollViewerContent.Children.Clear();
        }
    }
}
