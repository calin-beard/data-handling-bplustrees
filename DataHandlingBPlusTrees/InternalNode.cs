using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataHandlingBPlusTrees
{
    class InternalNode : Node
    {
        //Decided to use one class for all types of nodes
        //in order to avoid the 'is' operator and cast operations
        public List<Node> Children { get; set; }

        public InternalNode(string k, int n, bool root = false) :
            base(k, n, root)
        {
            
        }
    }
}
