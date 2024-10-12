using System.Windows.Input;
using System.Windows.Controls;

namespace TDU2_Track_Records
{
    public partial class SubCategoryCard : UserControl
    {
        public string SubcategoryNameText
        {
            get => SubcategoryName.Text;
            set => SubcategoryName.Text = value;
        }

        public string PointsText
        {
            get => Points.Text;
            set => Points.Text = value;
        }


        public SubCategoryCard()
        {
            InitializeComponent();
            this.MouseLeftButtonUp += SubCategoryCard_Click;
        }
        public double ProgressValue
        {
            get => Progress.Value;
            set
            {
                Progress.Value = value;
                ProgressText.Text = $"{value:F0}%"; // Update the percentage text
            }
        }

        public void SetProgress(double value, int maxPoints)
        {
            Progress.Value = value;
            ProgressText.Text = $"{value}/{maxPoints}"; // Display current progress out of maximum points
            MaximumPoints.Text = maxPoints.ToString(); // Assuming you have a MaximumPoints TextBlock
        }
        private void SubCategoryCard_Click(object sender, MouseButtonEventArgs e)
        {
            //// Raise an event or handle the click directly
            //if (this.DataContext is SubcategoryInfo info)
            //{
            //    // Assuming info contains category and subcategory data
            //    ObjectivesWindow.LoadObjectivesForSubCategory(info.Category, info.Subcategory);
            //}
        }
    }
}
