using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model
{
    public class FieldEventArgs
    {
        public List<Field>? Fields { get; }
        public bool Successful { get => Fields == null; }

        public FieldEventArgs(List<Field>? fields)
        {
            Fields = fields;
        }
    }
}
