using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataHandlingBPlusTrees
{
    public class ViewNode
    {
        public ViewNode()
        {
            this.Children = new ObservableCollection<ViewNode>();
            this.Keys = new ObservableCollection<string>();
        }

        public string Title { get; set; }

        public ObservableCollection<ViewNode> Children { get; set; }
        public ObservableCollection<string> Keys { get; set; }
    }
}
