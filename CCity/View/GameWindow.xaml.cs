using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CCity.View
{
    /// <summary>
    /// Interaction logic for GameWindow.xaml
    /// </summary>
    public partial class GameWindow : UserControl
    {
        public GameWindow()
        {
            InitializeComponent();
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            ((Grid)sender).Background = new SolidColorBrush(Color.FromArgb(20, 255, 255, 255));
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            ((Grid)sender).Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
        }

        private Point _lastMouseDownPos;
        private bool _isDragging;

        private void MapScrollViewer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                _isDragging = true;
                _lastMouseDownPos = e.GetPosition((UIElement)sender);
                Mouse.Capture((UIElement)sender);
            }
        }

        private void SetMinimapBorder(ScrollViewer scrollViewer)
        {
            var contentWidth = scrollViewer.ExtentWidth;
            var contentHeight = scrollViewer.ExtentHeight;
            var viewportWidth = scrollViewer.ViewportWidth;
            var viewportHeight = scrollViewer.ViewportHeight;
            var borderWidth = Minimap.ActualWidth * (viewportWidth / contentWidth);
            var borderHeight = Minimap.ActualHeight * (viewportHeight / contentHeight);
            MinimapBorder.Width = borderWidth;
            MinimapBorder.Height = borderHeight;

            var leftPercentage = scrollViewer.HorizontalOffset / (scrollViewer.ExtentWidth - scrollViewer.ViewportWidth);
            var topPercentage = scrollViewer.VerticalOffset / (scrollViewer.ExtentHeight - scrollViewer.ViewportHeight);
            var leftPos = (Minimap.ActualWidth - MinimapBorder.Width - 4) * leftPercentage;
            var topPos = (Minimap.ActualHeight - MinimapBorder.Height - 4) * topPercentage;
            Canvas.SetLeft(MinimapBorder, leftPos);
            Canvas.SetTop(MinimapBorder, topPos);
        }

        private void MapScrollViewer_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                var scrollViewer = (ScrollViewer)sender;
                var currentMousePos = e.GetPosition(scrollViewer);

                var delta = currentMousePos - _lastMouseDownPos;

                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - delta.X);
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - delta.Y);

                SetMinimapBorder(scrollViewer);
                _lastMouseDownPos = currentMousePos;
            }
        }

        private void MapScrollViewer_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                _isDragging = false;
                SetMinimapBorder((ScrollViewer)sender);
                Mouse.Capture(null);
            }
        }

        private void MapScrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;

            // Set vertical offset to bottom
            scrollViewer.ScrollToBottom();

            var contentWidth = scrollViewer.ExtentWidth;
            var viewportWidth = scrollViewer.ViewportWidth;

            scrollViewer.ScrollToHorizontalOffset((contentWidth - viewportWidth) / 2);

            SetMinimapBorder(scrollViewer);
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (MapScrollViewer.IsLoaded)
            {
                MapScrollViewer.InvalidateScrollInfo();
            }
        }

        private void MapScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;
            SetMinimapBorder(scrollViewer);
        }

        private void MapScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;
            SetMinimapBorder(scrollViewer);
        }
    }
}
