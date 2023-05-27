using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CCity.ViewModel.Enums;

namespace CCity.ViewModel.Items
{
    public class ToolItem : ViewModelBase
    {

        private bool _isSelected = false;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public Tool Tool { get; set; }
        public int Number { get; set; }
        public DelegateCommand? ClickCommand { get; set; }

    }
}
