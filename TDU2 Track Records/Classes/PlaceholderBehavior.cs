﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TDU2_Track_Records.Classes
{
    public static class PlaceholderBehavior
    {
        public static readonly DependencyProperty PlaceholderTextProperty =
            DependencyProperty.RegisterAttached(
                "PlaceholderText",
                typeof(string),
                typeof(PlaceholderBehavior),
                new PropertyMetadata(string.Empty, OnPlaceholderTextChanged));

        public static readonly DependencyProperty PlaceholderForegroundProperty =
            DependencyProperty.RegisterAttached(
                "PlaceholderForeground",
                typeof(Brush),
                typeof(PlaceholderBehavior),
                new PropertyMetadata(Brushes.Gray));

        public static string GetPlaceholderText(DependencyObject obj)
        {
            return (string)obj.GetValue(PlaceholderTextProperty);
        }

        public static void SetPlaceholderText(DependencyObject obj, string value)
        {
            obj.SetValue(PlaceholderTextProperty, value);
        }

        public static Brush GetPlaceholderForeground(DependencyObject obj)
        {
            return (Brush)obj.GetValue(PlaceholderForegroundProperty);
        }

        public static void SetPlaceholderForeground(DependencyObject obj, Brush value)
        {
            obj.SetValue(PlaceholderForegroundProperty, value);
        }

        private static void OnPlaceholderTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                textBox.GotFocus -= RemovePlaceholder;
                textBox.LostFocus -= ShowPlaceholder;

                textBox.GotFocus += RemovePlaceholder;
                textBox.LostFocus += ShowPlaceholder;

                ShowPlaceholder(textBox, null);
            }
        }

        private static void RemovePlaceholder(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (textBox.Text == GetPlaceholderText(textBox))
                {
                    textBox.Text = string.Empty;
                    textBox.Foreground = Brushes.White;
                }
            }
        }

        private static void ShowPlaceholder(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.Text = GetPlaceholderText(textBox);
                    textBox.Foreground = GetPlaceholderForeground(textBox);
                }
            }
        }
    }
}
