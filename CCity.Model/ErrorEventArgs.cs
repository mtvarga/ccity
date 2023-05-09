using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model
{
    public class ErrorEventArgs
    {
        public GameErrorType ErrorType { get; }

        public ErrorEventArgs(GameErrorType errorType)
        {
            ErrorType = errorType;
        }
    }
}
