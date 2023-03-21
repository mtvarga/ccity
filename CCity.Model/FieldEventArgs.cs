using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model
{
    public class FieldEventArgs
    {
        private List<Field> _fields;

        public List<Field> Fields { get { return _fields; } }

        public FieldEventArgs(List<Field> fields)
        {
            _fields = fields;
        }
    }
}
