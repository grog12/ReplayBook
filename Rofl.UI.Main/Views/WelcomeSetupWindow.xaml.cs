﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Rofl.UI.Main.Models;
using Rofl.UI.Main.Pages;
using Rofl.UI.Main.ViewModels;

namespace Rofl.UI.Main.Views
{
    /// <summary>
    /// Interaction logic for WelcomeSetupWindow.xaml
    /// </summary>
    public partial class WelcomeSetupWindow : Window
    {
        private readonly IList<Page> _welcomeSetupPages;
        private int _pageIndex;

        public WelcomeSetupSettings SetupSettings { get; set; }

        public WelcomeSetupWindow()
        {
            InitializeComponent();
            _welcomeSetupPages = new List<Page>();
            SetupSettings = new WelcomeSetupSettings();
        }

        public void MoveToNextPage()
        {
            int maxPage = _welcomeSetupPages.Count - 1;
            if (_pageIndex == maxPage) { return; }

            int newIndex = ++_pageIndex;

            // Show next page
            Page selectedPage = _welcomeSetupPages[newIndex];
            SetupFrame.Content = selectedPage;
            PageNameTextBlock.Text = GetPageTitle(selectedPage);

            // Change sizes of the navigation dots
            ((ModernWpf.Controls.PathIcon)NavigationDotsPanel.Children[newIndex]).Width = 8;
            ((ModernWpf.Controls.PathIcon)NavigationDotsPanel.Children[newIndex - 1]).Width = 5;
        }

        public void MoveToPreviousPage()
        {
            if (_pageIndex == 0) { return; }

            int newIndex = --_pageIndex;

            // Show previous page
            Page selectedPage = _welcomeSetupPages[newIndex];
            SetupFrame.Content = selectedPage;
            PageNameTextBlock.Text = GetPageTitle(selectedPage);

            // Change sizes of the navigation dots
            ((ModernWpf.Controls.PathIcon)NavigationDotsPanel.Children[newIndex]).Width = 8;
            ((ModernWpf.Controls.PathIcon)NavigationDotsPanel.Children[newIndex + 1]).Width = 5;
        }

        private void WelcomeSetupWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            _welcomeSetupPages.Add(new WelcomeSetupIntroduction
            {
                DataContext = this
            });
            //_welcomeSetupPages.Add(new WelcomeSetupRegion
            //{
            //    DataContext = this
            //});
            _welcomeSetupPages.Add(new WelcomeSetupExecutables
            {
                DataContext = this
            });
            _welcomeSetupPages.Add(new WelcomeSetupReplays
            {
                DataContext = this
            });
            _welcomeSetupPages.Add(new WelcomeSetupDownload
            {
                DataContext = this
            });
            _welcomeSetupPages.Add(new WelcomeSetupFinish
            {
                DataContext = this
            });

            Page firstPage = _welcomeSetupPages[0];
            SetupFrame.Content = firstPage;
            PageNameTextBlock.Text = GetPageTitle(firstPage);

            InitializeNavigationDots();
        }

        private void InitializeNavigationDots()
        {
            Grid markerPanel = NavigationDotsPanel;

            int pageCount = _welcomeSetupPages.Count;

            for (int i = 0; i < pageCount; i++)
            {
                markerPanel.ColumnDefinitions.Add(new ColumnDefinition()
                {
                    Width = new GridLength(20)
                });

                ModernWpf.Controls.PathIcon dotIcon = new ModernWpf.Controls.PathIcon()
                {
                    Data = (Geometry)TryFindResource("CirclePathIcon"),
                    Width = 5,
                    Margin = new Thickness(5, 0, 5, 0)
                };

                if (i == 0) { dotIcon.Width = 8; }

                Grid.SetColumn(dotIcon, i);

                _ = markerPanel.Children.Add(dotIcon);
            }
        }

        private string GetPageTitle(Page page)
        {
            switch (page)
            {
                case WelcomeSetupDownload _:
                    return (string)TryFindResource("WswDownloadFrameTitle");
                case WelcomeSetupExecutables _:
                    return (string)TryFindResource("WswExecutablesFrameTitle");
                case WelcomeSetupFinish _:
                    return (string)TryFindResource("WswFinishedFrameTitle");
                case WelcomeSetupIntroduction _:
                    return (string)TryFindResource("WswIntroFrameTitle");
                case WelcomeSetupRegion _:
                    return (string)TryFindResource("WswRegionFrameTitle");
                case WelcomeSetupReplays _:
                    return (string)TryFindResource("WswReplaysFrameTitle");
                default:
                    return "Title";
            }
        }

        private void WelcomeSetupWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (!(DataContext is MainWindowViewModel context)) { return; }

            context.WriteSkipWelcome();
        }
    }
}
