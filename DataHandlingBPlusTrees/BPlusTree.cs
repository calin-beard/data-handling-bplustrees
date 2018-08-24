using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataHandlingBPlusTrees
{
    class BPlusTree<K>
    {
        private const int DEGREE = 6;

        private static int Degree { get; set; }

        public Node Root { get; set; }

        public BPlusTree() : this(DEGREE) { }

        public BPlusTree(int _degree)
        {
            if (_degree <= 2)
            {
                throw new ArgumentException("The degree of the B+ tree is " + _degree + ".It should be >= 3");
            }
            BPlusTree<K>.Degree = _degree;
            this.Root = new LeafNode(this);
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

        public abstract class Node
        {
            public BPlusTree<K> BPTree { get; set; }
            public Node Parent { get; set; }
            public K[] Keys { get; set; }

            public abstract Tuple<Node, int> GetValue(K value);

            public abstract void InsertValue(K value, RecordPointer rp);

            public abstract void DeleteValue(K value);

            public void MakeNewRoot(Node brother)
            {
                InternalNode newRoot = new InternalNode(this.BPTree);
                newRoot.Keys[0] = brother.GetFirstLeafKey();
                newRoot.Pointers[0] = this;
                newRoot.Pointers[1] = brother;
                this.Parent = newRoot;
                brother.Parent = newRoot;
                this.BPTree.Root = newRoot;
            }

            public abstract K GetFirstLeafKey();

            protected abstract void Merge(Node brother);

            protected abstract Node Split();

            protected abstract int MinKeys();
            protected abstract int MaxKeys();

            protected abstract int MinPointers();
            protected abstract int MaxPointers();

            public abstract bool IsUnderflow();

            public abstract bool IsOverflow();

            public bool IsRoot()
            {
                return this.Parent == null;
            }

            public override string ToString()
            {
                return (this.IsRoot() ? "Root " : "") + "Node with first key " + this.Keys[0].ToString(); ;
            }
        }

        private class InternalNode : Node
        {
            public Node[] Pointers { get; set; }

            public InternalNode(BPlusTree<K> _bptree)
            {
                this.BPTree = _bptree;
                this.Keys = new K[this.MaxKeys()];
                this.Pointers = new Node[this.MaxPointers()];
            }

            public override Tuple<Node, int> GetValue(K value)
            {
                return this.GetChild(value).GetValue(value);
            }

            public override void InsertValue(K value, RecordPointer rp)
            {
                Node child = this.GetChild(value);
                child.InsertValue(value, rp);
                if (child.IsOverflow())
                {
                    Node brother = this.Split();
                    brother.Parent = this.Parent;
                    this.InsertChild(brother.GetFirstLeafKey(), brother);
                }
                if (this.BPTree.Root.IsOverflow())
                {
                    Node brother = this.Split();
                    this.MakeNewRoot(brother);
                }
            }

            public override void DeleteValue(K value)
            {
                // TO DO
                throw new NotImplementedException();
            }

            public override K GetFirstLeafKey()
            {
                return Pointers[0].GetFirstLeafKey();
            }

            protected override void Merge(Node brother)
            {
                // TO DO
                throw new NotImplementedException();
            }

            protected override Node Split()
            {
                // TO DO
                throw new NotImplementedException();
            }

            protected override int MinKeys()
            {
                return this.IsRoot() ? 1 : (int)Math.Ceiling((decimal)BPlusTree<K>.Degree / 2) - 1;
            }

            protected override int MaxKeys()
            {
                return BPlusTree<K>.Degree - 1;
            }

            protected override int MinPointers()
            {
                return this.IsRoot() ? 2 : (int)Math.Ceiling((decimal)BPlusTree<K>.Degree / 2);
            }

            protected override int MaxPointers()
            {
                return BPlusTree<K>.Degree;
            }

            public override bool IsUnderflow()
            {
                // TO DO
                throw new NotImplementedException();
            }

            public override bool IsOverflow()
            {
                return ArrayHandler.GetIndexOfLastElement(this.Pointers) + 1 > this.MaxPointers();
            }

            protected Node GetChild(K value)
            {
                int where = Array.BinarySearch(this.Keys, value);
                int childIndex = where >= 0 ? where + 1 : -where;
                return this.Pointers[childIndex];
            }

            protected void InsertChild(K value, Node child)
            {
                int where = Array.BinarySearch(this.Keys, value);
                int childIndex = where >= 0 ? where + 1 : where;
                if (where >= 0)
                {
                    this.Pointers[childIndex] = child;
                }
                else
                {
                    this.Keys[childIndex] = value;
                    this.Pointers[childIndex + 1] = child;
                }
            }
        }

        private class LeafNode : Node
        {
            RecordPointer[] RecordPointers { get; set; }

            LeafNode Next { get; set; }

            public LeafNode(BPlusTree<K> _bptree)
            {
                this.BPTree = _bptree;
                this.Keys = new K[this.MaxKeys()];
                this.RecordPointers = new RecordPointer[this.MaxPointers()];
            }

            public override Tuple<Node, int> GetValue(K value)
            {
                int where = Array.BinarySearch(this.Keys, value);
                return new Tuple<Node, int>(this, where);
            }

            public override void InsertValue(K value, RecordPointer rp)
            {
                int where = Array.BinarySearch(this.Keys, value);
                int valueIndex = where >= 0 ? where : -where;
                if (where >= 0)
                {
                    this.RecordPointers[valueIndex] = rp;
                }
                else
                {
                    this.Keys[valueIndex] = value;
                    this.RecordPointers[valueIndex] = rp;
                }
                if (this.BPTree.Root.IsOverflow())
                {
                    Node brother = this.Split();
                    this.MakeNewRoot(brother);
                }
            }

            public override void DeleteValue(K value)
            {
                // TO DO
                throw new NotImplementedException();
            }

            public override K GetFirstLeafKey()
            {
                return this.Keys[0];
            }

            protected override void Merge(Node brother)
            {
                // TO DO
                throw new NotImplementedException();
            }

            protected override Node Split()
            {
                // TO DO
                throw new NotImplementedException();
            }

            protected override int MinKeys()
            {
                return this.IsRoot() ? 1 : (int)Math.Ceiling((decimal)(BPlusTree<K>.Degree - 1) / 2);
            }

            protected override int MaxKeys()
            {
                return BPlusTree<K>.Degree - 1;
            }

            protected override int MinPointers()
            {
                return this.IsRoot() ? 2 : (int)Math.Ceiling((decimal)(BPlusTree<K>.Degree - 1) / 2);
            }

            protected override int MaxPointers()
            {
                return BPlusTree<K>.Degree - 1;
            }

            public override bool IsUnderflow()
            {
                // TO DO
                throw new NotImplementedException();
            }

            public override bool IsOverflow()
            {
                return ArrayHandler.GetIndexOfLastElement(this.RecordPointers) + 1 > this.MaxPointers();
            }
        }
    }
}