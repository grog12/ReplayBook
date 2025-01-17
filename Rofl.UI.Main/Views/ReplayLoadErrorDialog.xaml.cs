﻿using ModernWpf.Controls;
using Rofl.Files.Models;
using Rofl.UI.Main.Models;
using System.Windows.Documents;

namespace Rofl.UI.Main.Views
{
    /// <summary>
    /// Interaction logic for ErrorDisplayDialog.xaml
    /// </summary>
    public partial class ReplayLoadErrorDialog : ContentDialog
    {
        public ReplayLoadErrorDialog()
        {
            InitializeComponent();
        }

        private void ContentDialog_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!(DataContext is StatusBar context)) { return; }

            FlowDocument errorDetails = new FlowDocument();
            foreach (FileErrorResult error in context.Errors)
            {
                Bold filePath = new Bold(new Run(error.FilePath + "\n"));

                Paragraph errorParagraph = new Paragraph();
                errorParagraph.Inlines.Add(filePath);
                errorParagraph.Inlines.Add(new Run(error.Exception.Message));
                errorDetails.Blocks.Add(errorParagraph);
            }
            ErrorDetailsTextBox.Document = errorDetails;
        }
    }
}
