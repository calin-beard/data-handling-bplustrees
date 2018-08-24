using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataHandlingBPlusTrees
{
    class BPlusTree<K>
    {
        private const int _DEGREE = 6;

        private int Degree { get; set; }

        private Node Root { get; set; }

        public BPlusTree() : this(_DEGREE) { }

        public BPlusTree(int degree)
        {
            if (degree <= 2)
            {
                throw new ArgumentException("The degree of the B+ tree is " + degree + ".It should be >= 3");
            }
            this.Degree = degree;
        }

        public Tuple<Node, int> Find(K value)
        {
            return Root.GetValue(value);
        }

        public void Insert(K value, RecordPointer rp)
        {
            Root.InsertValue(value, rp);
        }

        public void Delete(K value)
        {
            Root.DeleteValue(value);
        }

        private abstract class Node
        {
            public K[] keys { get; set; }

            protected abstract Tuple<Node, int> GetValue(K value);

            protected abstract void InsertValue(K value, RecordPointer rp);

            protected abstract void DeleteValue(K value);

            protected abstract void Merge(Node brother);

            protected abstract Node split();

            protected abstract int MinKeys();
            protected abstract int MaxKeys();

            protected abstract int MinPointers();
            protected abstract int MaxPointers();

            public string ToString()
            {

            }
        }
    }
}