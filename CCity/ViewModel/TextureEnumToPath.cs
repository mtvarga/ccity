using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CCity.ViewModel
{
    [ValueConversion(typeof(Texture), typeof(string))]
    public class TextureEnumToPath : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Texture texture)
            {
                switch (texture)
                {
                    case Texture.PoliceDepartment: return "Images/Textures/policeDepartment.png";
                    case Texture.FireDepartment: return "Images/Textures/fireDepartment.png";
                    default: return "Images/Textures/transparent.png";
                }
            }

            return "Images/Textures/transparent.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
