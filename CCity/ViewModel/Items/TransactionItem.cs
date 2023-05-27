using CCity.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CCity.ViewModel.Items
{
    public class TransactionItem : ViewModelBase
    {

        #region Properties

        public string TransactionName { get; set; }
        public uint Amount { get; set; }
        public bool Add { get; set; }

        #endregion

    }
}
