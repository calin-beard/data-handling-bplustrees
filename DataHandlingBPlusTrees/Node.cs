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
        private int minkeys;
        public int MinKeys
        {
            get
            {
                return this.minkeys;
            }
            set
            {
                if (this.isLeaf())
                {
                    this.minkeys = (int)Math.Ceiling((decimal)Node.Degree - 1 / 2);
                }
                else
                {
                    this.minkeys = (int)Math.Ceiling((decimal)Node.Degree / 2);
                }
            }
        }
        private int maxKeys;
        public int MaxKeys
        {
            get
            {
                return this.maxKeys;
            }
            set
            {
                if (this.isLeaf())
                {
                    this.maxKeys = Node.Degree - 1;
                }
                else
                {
                    this.maxKeys = Node.Degree;
                }
            }
        }
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
            set
            {
                if (this.isLeaf())
                {
                    this.minRecords = this.MinKeys;
                }
                else
                {
                    this.minChildren = this.MinKeys + 1;
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
                    this.maxRecords = this.MaxKeys;
                }
                else
                {
                    this.maxChildren = this.MaxKeys + 1;
                }
            }
        }
        
        private static int secondHalfFirstIndex;
        public static int SecondHalfFirstIndex
        {
            get
            {
                return secondHalfFirstIndex;
            }
            set
            {
                secondHalfFirstIndex = (int)Math.Ceiling((decimal)Node.Degree / 2 + 1);
            }
        }

        //parent and keys
        public Node Parent { get; set; }
        public List<string> Keys { get; set; }

        //Internal node- specific props
        public List<Node> Children { get; set; }

        //Leaf- specific props
        public List<Record> Records { get; }
        public Node NextNode { get; set; }

        public Node() { }

        public Node(Node parent)
        {
            this.Parent = parent;
        }

        public Node(Node parent, string k, Node child1, Node child2)
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
            return Records.Any();
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
