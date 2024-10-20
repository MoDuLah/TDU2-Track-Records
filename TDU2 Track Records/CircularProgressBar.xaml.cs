using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TDU2_Track_Records
{
    public partial class CircularProgressBar : UserControl, INotifyPropertyChanged
    {
        // Dependency Properties
        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register("Progress", typeof(double), typeof(CircularProgressBar), new PropertyMetadata(0.0, OnProgressChanged));

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(CircularProgressBar), new PropertyMetadata(100.0));

        public static readonly DependencyProperty StrokeColorProperty =
            DependencyProperty.Register("StrokeColor", typeof(Brush), typeof(CircularProgressBar), new PropertyMetadata(Brushes.Blue));

        public static readonly DependencyProperty PathColorProperty =
            DependencyProperty.Register("PathColor", typeof(Brush), typeof(CircularProgressBar), new PropertyMetadata(Brushes.Silver));


        // Public properties
        public double Progress
        {
            get { return (double)GetValue(ProgressProperty); }
            set { SetValue(ProgressProperty, value); }
        }

        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public Brush StrokeColor
        {
            get { return (Brush)GetValue(StrokeColorProperty); }
            set { SetValue(StrokeColorProperty, value); }
        }

        public Brush PathColor
        {
            get { return (Brush)GetValue(PathColorProperty); }
            set { SetValue(PathColorProperty, value); }
        }

        public string ProgressText
        {
            get
            {
                // Calculate percentage
                double percentage = Maximum > 0 ? Math.Round((Progress * 100.0 / Maximum), 0) : 0;
                return $"{percentage}%"; // Returns percentage with a '%' sign
            }
        }

        public Geometry ArcData
        {
            get
            {
                double angle = (Progress / Maximum) * 360;
                return CreateArcGeometry(angle);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public CircularProgressBar()
        {
            InitializeComponent();
            DataContext = this;
        }

        // Handles progress changes
        private static void OnProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CircularProgressBar progressBar = d as CircularProgressBar;
            if (progressBar != null)
            {
                progressBar.OnPropertyChanged("ProgressText");
                progressBar.OnPropertyChanged("ArcData");
            }
        }

        // Helper to create the arc based on the current progress
        private Geometry CreateArcGeometry(double angle)
        {
            // Reducing the radius by multiplying it by a factor (e.g., 0.75 for 75% of the original size)
            double radiusFactor = 0.65; // Adjust this factor as needed to decrease the arc's radius
            double radius = (Math.Min(ActualWidth, ActualHeight) / 2) * radiusFactor; // Scale down the radius
            Point startPoint = new Point(radius, 0);
            Point endPoint = ComputeCartesianCoordinate(angle, radius);
            endPoint.X += radius;
            endPoint.Y += radius;

            bool largeArc = angle > 180.0;

            Size outerArcSize = new Size(radius, radius);

            PathFigure figure = new PathFigure
            {
                StartPoint = startPoint,
                IsClosed = false,
                IsFilled = false,
                Segments = new PathSegmentCollection
        {
            new ArcSegment
            {
                Point = endPoint,
                Size = outerArcSize,
                SweepDirection = SweepDirection.Clockwise,
                IsLargeArc = largeArc
            }
        }
            };

            PathGeometry geometry = new PathGeometry();
            geometry.Figures.Add(figure);
            return geometry;
        }


        private Point ComputeCartesianCoordinate(double angle, double radius)
        {
            // Convert angle to radians
            double angleRad = (Math.PI / 180.0) * (angle - 90);
            double x = radius * Math.Cos(angleRad);
            double y = radius * Math.Sin(angleRad);
            return new Point(x, y);
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}