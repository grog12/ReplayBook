﻿using ModernWpf.Controls;
using Rofl.UI.Main.Utilities;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Rofl.UI.Main.Views
{
    /// <summary>
    /// Interaction logic for ExportPresetLoadDialog.xaml
    /// </summary>
    public partial class ExportPresetLoadDialog : ContentDialog
    {
        public ExportPresetLoadDialog()
        {
            InitializeComponent();

            PresetNamesBox.ItemsSource = ExportHelper.FindAllPresets();
        }

        private void PresetNamesBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string presetName = PresetNamesBox.SelectedItem as string;

            try
            {
                DataContext = ExportHelper.LoadPreset(presetName);
                PresetNamesBox.BorderBrush = TryFindResource("ComboBoxItemForeground") as Brush;
            }
            catch (FileNotFoundException)
            {
                // Could not find preset with name, highlight error
                PresetNamesBox.BorderBrush = TryFindResource("SystemControlErrorTextForegroundBrush") as Brush;
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            // is the delete called on an empty file?
            if (string.IsNullOrEmpty(PresetNamesBox.SelectedItem as string)) { return; }

            // create the flyout
            Flyout flyout = FlyoutHelper.CreateFlyout(includeButton: true, includeCustom: false);

            // set the flyout texts
            flyout.SetFlyoutButtonText(TryFindResource("DeleteReplayFile") as string);
            flyout.SetFlyoutLabelText(TryFindResource("DeleteFlyoutLabel") as string);

            // set button click function
            flyout.GetFlyoutButton().Click += (object eSender, RoutedEventArgs eConfirm) =>
            {
                ExportHelper.DeletePresetFile(PresetNamesBox.SelectedItem as string);

                // reload presets
                PresetNamesBox.ItemsSource = ExportHelper.FindAllPresets();

                // Hide the flyout
                Dispatcher.Invoke(() =>
                {
                    flyout.Hide();
                });
            };

            // Show the flyout and focus it
            flyout.ShowAt(DeleteButton);
            _ = flyout.GetFlyoutButton().Focus();
        }
    }
}
