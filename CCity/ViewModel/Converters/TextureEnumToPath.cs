using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Navigation;
using CCity.ViewModel.Enums;

namespace CCity.ViewModel.Converters
{
    [ValueConversion(typeof(Texture), typeof(string))]
    public class TextureEnumToPath : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Texture texture) throw new ArgumentException();
            return texture switch
            {
                Texture.None => "Images/Textures/transparent.png",
                _ => GetPath(texture)
            };
        }

        private static string GetPath(Texture texture)
        {
            string textureString = texture.ToString();
            string fileString = string.Concat(textureString.Substring(0, 1).ToLower(), textureString.AsSpan(1));
            if (fileString.StartsWith("road")) return "Images/Textures/Roads/" + fileString + ".png";
            else if (fileString.Contains("Zone")) return "Images/Textures/Zones/" + fileString + ".png";
            else return "Images/Textures/" + fileString + ".png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
