using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                    case Texture.None: return "Images/Textures/transparent.png";
                    default:
                        return GetPath(texture);
                }
            }

            return "Images/Textures/transparent.png";
        }

        private string GetPath(Texture texture)
        {
            string textureString = texture.ToString();
            string fileString = textureString.Substring(0, 1).ToLower() + textureString.Substring(1);
            if (fileString.StartsWith("road")) return "Images/Textures/Roads/" + fileString + ".png";
            else if (fileString.Contains("Zone")) return "Images/Textures/Zones/" + fileString + ".png";
            else return "Images/Textures/" + fileString + ".png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
