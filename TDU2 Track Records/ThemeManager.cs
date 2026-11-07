using System;
using System.Windows;
using System.Windows.Controls;

namespace TDU2_Track_Records
{
    internal static class ThemeManager
    {
        public static void ApplyScopedControlStyles(FrameworkElement scope)
        {
            if (scope == null || Application.Current == null)
                return;

            ApplyScopedStyle(scope, typeof(GroupBox), "AppScopedGroupBoxStyle");
            ApplyScopedStyle(scope, typeof(System.Windows.Controls.ComboBoxItem), "AppScopedComboBoxItemStyle");
            ApplyScopedStyle(scope, typeof(ComboBox), "AppScopedComboBoxStyle");
            ApplyScopedStyle(scope, typeof(TextBox), "AppScopedTextBoxStyle");
            ApplyScopedStyle(scope, typeof(Button), "AppScopedButtonStyle");
            ApplyScopedStyle(scope, typeof(CheckBox), "AppScopedCheckBoxStyle");
        }

        private static void ApplyScopedStyle(FrameworkElement scope, Type controlType, string resourceKey)
        {
            Style style = Application.Current.TryFindResource(resourceKey) as Style;
            if (style != null)
                scope.Resources[controlType] = style;
        }

    }
}
