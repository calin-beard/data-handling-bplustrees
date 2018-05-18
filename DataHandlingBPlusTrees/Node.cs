using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataHandlingBPlusTrees
{
    class Node
    {
        //properties for "magic numbers" for the node
        public static int Degree { get; set; }
        public int MinKeys { get; private set; }
        public int MaxKeys { get; private set; }
        private int minChildren;
        private int minRecords;
        public int MinPointers
        {
            get
            {
                if (this.isLeaf())
                {
                    return this.minRecords;
                }
                else
                {
                    return this.minChildren;
                }
            }
            private set
            {
                if (this.isLeaf())
                {
                    this.minRecords = value;
                }
                else
                {
                    this.minChildren = value;
                }
            }
        }
        private int maxChildren;
        private int maxRecords;
        public int MaxPointers
        {
            get
            {
                if (this.isLeaf())
                {
                    return this.maxRecords;
                }
                else
                {
                    return this.maxChildren;
                }
            }
            set
            {
                if (this.isLeaf())
                {
                    this.maxRecords = value;
                }
                else
                {
                    this.maxChildren = value;
                }
            }
        }
        public int SecondHalfFirstIndex { get; } = (int)((double)Node.Degree / 2 + 1);

        //parent and keys
        public Node Parent { get; set; }
        public List<string> Keys { get; set; }

        //Internal node- specific props
        public List<Node> Children { get; set; }

        //Leaf- specific props
        public List<Record> Records { get; }
        public Node NextNode { get; set; }

        public Node()
        {
            this.MinKeys = this.isLeaf() ? (int)Math.Ceiling(((decimal)Node.Degree - 1) / 2) : (int)Math.Ceiling((decimal)Node.Degree / 2);
            this.MaxKeys = this.isLeaf() ? Node.Degree - 1 : Node.Degree;
            this.MinPointers = this.isLeaf() ? this.MinKeys : this.MinKeys + 1;
            this.MaxPointers = this.isLeaf() ? this.MaxKeys : this.MaxKeys + 1;
        }

        public Node(Node parent, bool isLeaf = false) : this()
        {
            this.Parent = parent;
            this.Keys = new List<string>();
            if (isLeaf)
            {
                this.Records = new List<Record>();
            }
        }

        public Node(string k, Record r) : this()
        {
            this.Keys = new List<string>();
            this.Keys.Add(k);
            this.Records = new List<Record>();
            this.Records.Add(r);
        }

        public Node(Node parent, string k, Node child1, Node child2) : this()
        {
            //TO DO
            this.Parent = parent;
            this.Keys = new List<string>();
            this.Keys.Add(k);
            this.Children = new List<Node>();
            this.Children.Add(child1);
            this.Children.Add(child2);
        }

        ~Node()
        {
            Console.WriteLine("The destructor for " + this + " is called");
        }

        public bool isRoot()
        {
            return this.Parent == null;
        }

        public bool isLeaf()
        {
            return this.Records != null ? true : false;
        }

        public override string ToString()
        {
            string result = "";
            if (this.isRoot())
            {
                result += "Root ";
            }
            if (this.isLeaf())
            {
                result += "Leaf ";
            }
            result += "Node with first key " + this.Keys[0];

            return result;
        }
    }
}
