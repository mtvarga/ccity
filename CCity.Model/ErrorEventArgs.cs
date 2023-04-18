using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model
{
    public class ErrorEventArgs
    {
        public string ErrorCode { get; }

        public ErrorEventArgs(string code)
        {
            ErrorCode = code;
        }
    }
}
