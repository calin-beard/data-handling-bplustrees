using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataHandlingBPlusTrees
{
    class Pointer
    {
        public Dictionary<string, string> Pointerr;

        public Pointer() { }

        public Pointer(Dictionary<string, string> pointer)
        {
            this.Pointerr = new Dictionary<string, string>(pointer);
        }
    }
}
