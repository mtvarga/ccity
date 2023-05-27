using CCity.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using CCity.ViewModel.Enums;

namespace CCity.ViewModel.Converters
{
    [ValueConversion(typeof(Texture), typeof(bool))]
    public class TextureEnumToIsOversized : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Texture texture) throw new ArgumentException();
            return texture switch
            {
                Texture.FireDepartment => true,

                Texture.PoliceDepartment => true,

                Texture.Stadium_0_1 => true,
                Texture.Stadium_1_1 => true,

                Texture.PowerPlant_0_1 => true,
                Texture.PowerPlant_1_1 => true,

                Texture.ForestFull => true,

                Texture.ResidentialZoneAdvancedHalf => true, Texture.ResidentialZoneAdvancedFull => true,
                Texture.CommercialZoneAdvancedHalf => true, Texture.CommercialZoneAdvancedFull => true,
                Texture.IndustrialZoneAdvancedHalf => true, Texture.IndustrialZoneAdvancedFull => true,

                _ => false
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
