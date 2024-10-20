using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TDU2_Track_Records
{
    public partial class CircularProgressBarExample : Window
    {
        public CircularProgressBarExample()
        {
            InitializeComponent();
            UpdateProgress(0.20); // Example: Set progress to 75%
        }

        public void UpdateProgress(double progressPercentage)
        {
            // Ensure the progress percentage is between 0 and 1
            progressPercentage = Math.Max(0, Math.Min(progressPercentage, 1));

            // Calculate the angle for the arc
            double angle = progressPercentage * 360;

            // Define the arc segment
            var arcSegment = new ArcSegment
            {
                Point = new Point(67, 0), // End point of the arc
                Size = new Size(67, 67), // Size of the arc
                SweepDirection = SweepDirection.Clockwise,
                IsLargeArc = angle > 180,
            };

            // Create a path figure for the arc
            var pathFigure = new PathFigure
            {
                StartPoint = new Point(67, 0), // Start at the top of the circle
                Segments = new PathSegmentCollection { arcSegment },
                IsClosed = false
            };

            // Create a path geometry for the arc
            var pathGeometry = new PathGeometry
            {
                Figures = new PathFigureCollection { pathFigure }
            };

            // Bind the geometry to the Path
            ProgressArc = pathGeometry; // Update the ProgressArc property
        }

        // Property to bind the ProgressArc to the Path's Data
        public PathGeometry ProgressArc { get; set; }
    }
}
