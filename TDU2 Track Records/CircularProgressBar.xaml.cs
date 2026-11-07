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
            DependencyProperty.Register("Maximum", typeof(double), typeof(CircularProgressBar), new PropertyMetadata(100.0, OnProgressChanged));

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(nameof(Icon), typeof(ImageSource), typeof(CircularProgressBar));

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

        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public string ProgressText
        {
            get
            {
                // Calculate percentage
                double percentage = Maximum > 0 ? Math.Round(Math.Max(0, Math.Min(Progress / Maximum, 1)) * 100) : 0;
                return $"{percentage}%"; // Returns percentage with a '%' sign
            }
        }

        public Geometry ArcData
        {
            get
            {
                double angle = Maximum > 0 ? Math.Max(0, Math.Min(Progress / Maximum, 1)) * 360 : 0;
                return CreateArcGeometry(angle);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public CircularProgressBar()
        {
            InitializeComponent();
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
            if (angle <= 0)
                return Geometry.Empty;

            const double center = 32;
            const double radius = 29;
            if (angle >= 360)
                return new EllipseGeometry(new Point(center, center), radius, radius);
            Point startPoint = new Point(center, center - radius);
            Point endPoint = ComputeCartesianCoordinate(angle, radius);
            endPoint.X += center;
            endPoint.Y += center;

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
