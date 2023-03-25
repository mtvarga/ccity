using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCity.ViewModel
{
    public class ViewModelBase : INotifyPropertyChanged
    {

        #region Fields



        #endregion

        #region Properties



        #endregion

        #region Constructors

        

        #endregion

        #region Public methods



        #endregion

        #region Private methods



        #endregion

        #region Events

        event PropertyChangedEventHandler? INotifyPropertyChanged.PropertyChanged
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}
