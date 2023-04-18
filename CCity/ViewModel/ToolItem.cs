using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCity.ViewModel
{
    public class ToolItem : ViewModelBase
    {

        public Tool Tool { get; set; }
        public string Text { get => Tool.ToString(); }
        public DelegateCommand? ClickCommand { get; set; }

    }
}
