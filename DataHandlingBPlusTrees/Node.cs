using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataHandlingBPlusTrees
{
    class Node
    {
        // Properties for "magic numbers" of the node
        public static int Degree { get; set; }

        public int MinKeys { get; private set; }
        public int MaxKeys { get; private set; }

        /// <summary>
        /// Minimum number of pointers a node can carry
        /// </summary>
        public int MinPointers { get; set; }

        /// <summary>
        /// Maximum number of pointers a node can carry
        /// </summary>
        public int MaxPointers { get; set; }

        // Index from which the keys are moved when splitting a node
        public int SecondHalfFirstIndex { get; } = (int)Math.Ceiling((double)Node.Degree / 2);
        // Index where the keys are inserted in the brother when splitting a node
        public int FirstInsertionIndex { get; } = (int)Math.Floor((double)Node.Degree / 2) - 1;

        // Parent and keys
        public Node Parent { get; set; }

        public string[] Keys { get; set; }

        //Internal node-specific props
        public Node[] Pointers { get; set; }

        //Leaf-specific props
        public RecordPointer[] RecordPointers { get; set; }
        public Node NextNode { get; set; }

        // Basic constructor
        public Node()
        {
            this.MinKeys = this.IsRoot() ? 1 : (this.IsLeaf() ? (int)Math.Ceiling((decimal)(Node.Degree - 1) / 2) : (int)Math.Ceiling((decimal)Node.Degree / 2) - 1);
            this.MaxKeys = Node.Degree - 1;
            this.MinPointers = this.IsRoot() ? 2 : (this.IsLeaf() ? (int)Math.Ceiling((decimal)(Node.Degree - 1) / 2) : (int)Math.Ceiling((decimal)Node.Degree / 2));
            this.MaxPointers = this.IsLeaf() ? Node.Degree - 1 : Node.Degree;
        }

        // Copy constructor
        public Node(Node n, int extraroom = 0) : this()
        {
            if (extraroom > 0)
            {
                this.MaxKeys += extraroom;
                this.MaxPointers += extraroom;
            }
            this.Parent = n.Parent;
            this.Keys = new string[this.MaxKeys];
            Array.Copy(n.Keys, this.Keys, n.Keys.Length);
            if (n.IsLeaf())
            {
                this.RecordPointers = new RecordPointer[this.MaxPointers];
                Array.Copy(n.RecordPointers, this.RecordPointers, n.RecordPointers.Length);
            }
            else
            {
                this.Pointers = new Node[this.MaxPointers];
                Array.Copy(n.Pointers, this.Pointers, n.Pointers.Length);
            }
            this.NextNode = n.NextNode;
        }

        // Constructor called by BPlusTree.SplitNode(target)
        public Node(Node parent, bool isLeaf = false) : this()
        {
            this.Parent = parent;
            this.Keys = new string[this.MaxKeys];
            if (isLeaf)
            {
                this.RecordPointers = new RecordPointer[this.MaxPointers];
            }
            else
            {
                this.Pointers = new Node[this.MaxPointers];
            }
        }

        // Constructor that creates root node
        public Node(string k, RecordPointer r) : this()
        {
            this.Keys = new string[this.MaxKeys];
            this.Keys[0] = k;
            this.RecordPointers = new RecordPointer[this.MaxPointers];
            this.RecordPointers[0] = r;
        }

        // Constructor called by BPlusTree.InsertInParent(...) when creating new root
        public Node(Node parent, string k, Node child1, Node child2) : this()
        {
            this.Parent = parent;
            //child1.Parent = this;
            //child2.Parent = this;
            this.Keys = new string[this.MaxKeys];
            this.Keys[0] = k;
            this.Pointers = new Node[this.MaxPointers];
            this.Pointers[0] = child1;
            this.Pointers[1] = child2;
        }

        // Destructor
        ~Node()
        {
            Console.WriteLine("The destructor for " + this + " is called");
        }

        public void AdjustMinThresholds()
        {
            this.MinKeys = this.IsRoot() ? 1 : (this.IsLeaf() ? (int)Math.Ceiling((decimal)(Node.Degree - 1) / 2) : (int)Math.Ceiling((decimal)Node.Degree / 2) - 1);
            this.MinPointers = this.IsRoot() ? 2 : (this.IsLeaf() ? (int)Math.Ceiling((decimal)(Node.Degree - 1) / 2) : (int)Math.Ceiling((decimal)Node.Degree / 2));
        }

        // Method that tests if the node is the root node
        // true if it has no parent
        public bool IsRoot()
        {
            return this.Parent == null;
        }

        // Method that tests if the node is a leaf node
        // true if it does not have Pointers
        public bool IsLeaf()
        {
            return this.RecordPointers != null;
        }

        // ToString override for debugging
        public override string ToString()
        {
            string result = "";
            if (this.IsRoot())
            {
                result += "Root ";
            }
            if (this.IsLeaf())
            {
                result += "Leaf ";
            }
            result += "Node with first key " + this.Keys[0];

            return result;
        }
    }
}
