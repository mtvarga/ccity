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
    }
}
