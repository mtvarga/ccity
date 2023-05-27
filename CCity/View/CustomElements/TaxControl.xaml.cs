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

namespace CCity.View.CustomElements
{
    /// <summary>
    /// Interaction logic for TaxControl.xaml
    /// </summary>
    public partial class TaxControl : UserControl
    {
        public static readonly DependencyProperty TaxCommandProperty =
            DependencyProperty.Register("TaxCommand", typeof(ICommand), typeof(TaxControl));

        public static readonly DependencyProperty TaxValueProperty =
            DependencyProperty.Register("TaxValue", typeof(object), typeof(TaxControl));

        public ICommand TaxCommand
        {
            get { return (ICommand)GetValue(TaxCommandProperty); }
            set { SetValue(TaxCommandProperty, value); }
        }

        public object TaxValue
        {
            get { return GetValue(TaxValueProperty); }
            set { SetValue(TaxValueProperty, value); }
        }

        public TaxControl()
        {
            InitializeComponent();
        }
    }
}
