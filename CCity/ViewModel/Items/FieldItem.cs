using CCity.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using CCity.ViewModel.Enums;

namespace CCity.ViewModel.Items
{
    public class FieldItem : ViewModelBase
    {

        #region Fields

        private Texture _texture;
        private Color _overlayColor;
        private Texture _additionalTexture;

        #endregion

        #region Properties

        public Texture Texture {
            get { return _texture; }
            set
            {
                if(_texture != value)
                {
                    _texture = value;
                    OnPropertyChanged();
                }
            } 
        }

        public Color OverlayColor
        {
            get { return _overlayColor; }
            set
            {
                if(_overlayColor != value)
                {
                    _overlayColor = value;
                    OnPropertyChanged();
                }
            }
        }

        public Texture AdditionalTexture
        {
            get { return _additionalTexture; }
            set
            {
                if (_additionalTexture != value)
                {
                    _additionalTexture = value;
                    OnPropertyChanged();
                }
            }
        }

        public int X { get; set; }
        public int Y { get; set; }
        public int Number { get; set; }
        public DelegateCommand? ClickCommand { get; set; }

        #endregion

    }
}
