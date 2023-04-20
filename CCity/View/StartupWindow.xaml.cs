using System;
using System.Collections.Generic;
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
    /// Interaction logic for StartupWindow.xaml
    /// </summary>
    public partial class StartupWindow : UserControl
    {
        public StartupWindow()
        {
            InitializeComponent();
        }

        private void VideoPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            videoPlayer.Position = TimeSpan.Zero;
            videoPlayer.Play();
        }
    }
}
