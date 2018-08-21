using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataHandlingBPlusTrees
{
    class RecordPointer
    {
        public Dictionary<string, string> Pointer;

        public RecordPointer() { }

        public RecordPointer(Dictionary<string, string> pointer)
        {
            this.Pointer = new Dictionary<string, string>(pointer);
        }
    }
}
