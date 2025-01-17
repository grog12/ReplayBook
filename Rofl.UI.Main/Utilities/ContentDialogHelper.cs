﻿using ModernWpf.Controls;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Rofl.UI.Main.Utilities
{
    public static class ContentDialogHelper
    {
        /// <summary>
        /// Creates a <see cref="ContentDialog"/> using the parameters
        /// </summary>
        /// <param name="includeCloseButton"></param>
        /// <returns></returns>
        public static ContentDialog CreateContentDialog(bool includeSecondaryButton = false)
        {
            #region Grid Definitions
            Grid contentPanel = new Grid();

            ColumnDefinition columnOne = new ColumnDefinition
            {
                Width = new GridLength(1, GridUnitType.Star)
            };

            RowDefinition rowOne = new RowDefinition
            {
                Height = new GridLength(1, GridUnitType.Auto)
            };
            RowDefinition rowTwo = new RowDefinition
            {
                Height = new GridLength(1, GridUnitType.Star)
            };

            contentPanel.ColumnDefinitions.Add(columnOne);
            contentPanel.RowDefinitions.Add(rowOne);
            contentPanel.RowDefinitions.Add(rowTwo);
            #endregion

            TextBlock label = new TextBlock
            {
                Name = "LabelTextBlock",
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new Thickness(0, 0, 0, 0)
            };
            Grid.SetRow(label, 0);
            Grid.SetColumn(label, 0);

            _ = contentPanel.Children.Add(label);

            ContentDialog dialog = new ContentDialog
            {
                Content = contentPanel,
                IsPrimaryButtonEnabled = true,
                IsSecondaryButtonEnabled = includeSecondaryButton,
                Background = Application.Current.FindResource("TabBackground") as Brush
            };

            _ = dialog.ApplyTemplate();
            return dialog;
        }

        /// <summary>
        /// Only works for <see cref="ContentDialog"/>s made by <see cref="ContentDialogHelper"/>
        /// </summary>
        /// <param name="dialog"></param>
        /// <returns></returns>
        public static TextBlock GetContentDialogLabel(this ContentDialog dialog)
        {
            return dialog == null
                ? throw new ArgumentNullException(nameof(dialog))
                : !(dialog.Content is Grid content)
                    ? null
                    : LogicalTreeHelper.FindLogicalNode(content, "LabelTextBlock") as TextBlock;
        }

        /// <summary>
        /// Only works for <see cref="ContentDialog"/>s made by <see cref="ContentDialogHelper"/>
        /// </summary>
        /// <param name="dialog"></param>
        /// <param name="text"></param>
        public static void SetLabelText(this ContentDialog dialog, string text)
        {
            if (dialog == null) { throw new ArgumentNullException(nameof(dialog)); }
            if (!(dialog.Content is Grid content)) { throw new ArgumentException("ContentDialog content is not as expected"); }

            (LogicalTreeHelper.FindLogicalNode(content, "LabelTextBlock") as TextBlock).Text = text;
        }

        /// <summary>
        /// Set the overlay smoke color and appears behind the dialog.
        /// </summary>
        /// <param name="dialog"></param>
        /// <param name="color"></param>
        public static void SetBackgroundSmokeColor(this ContentDialog dialog, Brush color)
        {
            if (dialog == null) { throw new ArgumentNullException(nameof(dialog)); }

            Grid root = WindowHelper.FindVisualChildren<Grid>(dialog)
                                    .First(x => x.Name.Equals("LayoutRoot", StringComparison.OrdinalIgnoreCase));

            root.Background = color;
        }
    }
}
