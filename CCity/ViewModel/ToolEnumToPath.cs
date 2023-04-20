using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.TextFormatting;

namespace CCity.ViewModel
{
    [ValueConversion(typeof(Tool), typeof(string))]
    public class ToolEnumToPath : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is Tool tool)
            {
                return GetPath(tool);
            }
            return "Images/Textures/Unhandled.png";
        }

        private string GetPath(Tool tool)
        {
            string textureString = tool.ToString();
            string fileString = textureString.Substring(0, 1).ToLower() + textureString.Substring(1);
            Trace.WriteLine("Images/Textures/Tools/" + fileString + ".png");
            return "Images/Textures/Tools/" + fileString + ".png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
