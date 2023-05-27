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
    /// Interaction logic for CustomSwitchButton.xaml
    /// </summary>
    public partial class CustomSwitchButton : UserControl
    {
        public static readonly DependencyProperty CustomContentProperty =
            DependencyProperty.Register("CustomContent", typeof(object), typeof(CustomSwitchButton));

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(CustomSwitchButton));

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object), typeof(CustomSwitchButton));

        public static readonly DependencyProperty IsToggledProperty =
            DependencyProperty.Register("IsToggled", typeof(object), typeof(CustomSwitchButton));

        public object CustomContent
        {
            get { return GetValue(CustomContentProperty); }
            set { SetValue(CustomContentProperty, value); }
        }

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        public object IsToggled
        {
            get { return GetValue(IsToggledProperty); }
            set { SetValue(IsToggledProperty, value); }
        }

        public CustomSwitchButton()
        {
            InitializeComponent();
        }
    }
}
