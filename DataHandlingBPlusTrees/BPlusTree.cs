using System;
using System.Collections;
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

        public BPlusTree(K maxvalue) : this(DEGREE) { }

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
        
        public void Insert(K value, RecordPointer<K> rp)
        {
            Root.InsertValue(value, rp);
        }
        public void InsertMultiple(Dictionary<K, RecordPointer<K>> elements)
        {
            foreach (KeyValuePair<K, RecordPointer<K>> element in elements)
            {
                this.Insert(element.Key, element.Value);
            }
        }

        public void Delete(K value)
        {
            Root.DeleteValue(value);
        }

        public abstract class Node
        {
            public static BPlusTree<K> BPTree { get; set; }
            public Node Parent { get; set; }
            public List<K> Keys { get; set; }

            public abstract Tuple<Node, int> GetValue(K value);

            public abstract void InsertValue(K value, RecordPointer<K> rp);

            public abstract void DeleteValue(K value);

            public void CreateNewRoot(Node brother)
            {
                InternalNode newRoot = new InternalNode(BPTree);
                newRoot.Keys.Add(brother.GetFirstLeafKey());
                newRoot.Pointers.Add(this);
                newRoot.Pointers.Add(brother);
                this.Parent = newRoot;
                brother.Parent = newRoot;
                BPTree.Root = newRoot;
            }

            public abstract K GetFirstLeafKey();

            public abstract void Merge(Node brother);

            public abstract Node Split();

            protected abstract int MinKeys();
            protected abstract int MaxKeys();

            protected abstract int MinPointers();
            protected abstract int MaxPointers();

            public abstract bool IsUnderflow();

            public abstract bool IsOverflow();

            public bool IsRoot()
            {
                return BPTree.Root == this;
            }

            public override string ToString()
            {
                return (this.IsRoot() ? "Root " : "") + "Node with first key " + this.Keys.ElementAt(0).ToString(); ;
            }
        }

        private class InternalNode : Node
        {
            public List<Node> Pointers { get; set; }

            public InternalNode(BPlusTree<K> _bptree)
            {
                BPTree = _bptree;
                this.Keys = new List<K>(this.MaxKeys());
                this.Pointers = new List<Node>(this.MaxPointers());
            }

            public override Tuple<Node, int> GetValue(K value)
            {
                return this.GetChild(value).GetValue(value);
            }

            public override void InsertValue(K value, RecordPointer<K> rp)
            {
                Node child = this.GetChild(value);
                child.InsertValue(value, rp);
                if (child.IsOverflow())
                {
                    Node brother = child.Split();
                    brother.Parent = child.Parent;
                    this.InsertChild(brother.GetFirstLeafKey(), brother);
                }
                if (BPTree.Root.IsOverflow())
                {
                    Node brother = this.Split();
                    this.CreateNewRoot(brother);
                }
            }

            public override void DeleteValue(K value)
            {
                Node child = this.GetChild(value);
                child.DeleteValue(value);
                if (child.IsUnderflow())
                {
                    Node childLeftBrother = this.GetChildLeftBrother(value);
                    Node childRightBrother = this.GetChildRightBrother(value);
                    Node left = childLeftBrother != null ? childLeftBrother : child;
                    Node right = childLeftBrother != null ? child : childRightBrother;
                    left.Merge(right);
                    this.DeleteChild(right.GetFirstLeafKey());
                    if (left.IsOverflow())
                    {
                        Node brother = left.Split();
                        this.InsertChild(brother.GetFirstLeafKey(), brother);
                    }
                    if (BPTree.Root.IsUnderflow())
                    {
                        BPTree.Root = left;
                        BPTree.Root.Parent = null;
                    }
                }
            }

            public override K GetFirstLeafKey()
            {
                return Pointers.ElementAt(0).GetFirstLeafKey();
            }

            public override void Merge(Node brother)
            {
                InternalNode node = (InternalNode)brother;
                this.Keys.Add(node.GetFirstLeafKey());
                this.Keys.AddRange(node.Keys);
                this.Pointers.AddRange(node.Pointers);
            }

            public override Node Split()
            {
                InternalNode brother = new InternalNode(BPTree);
                int from = this.Keys.Count / 2 + 1;
                int count = this.Keys.Count - from;
                brother.Keys.AddRange(this.Keys.GetRange(from, count));
                brother.Pointers.AddRange(this.Pointers.GetRange(from, count + 1));
                foreach (BPlusTree<K>.Node child in brother.Pointers)
                {
                    child.Parent = brother;
                }

                this.Keys.RemoveRange(from, count);
                this.Pointers.RemoveRange(from, count + 1);
                return brother;
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
                return this.Pointers.Count < this.MinPointers();
            }

            public override bool IsOverflow()
            {
                return this.Pointers.Count > this.MaxPointers();
            }

            protected Node GetChild(K value)
            {
                int where = this.Keys.BinarySearch(value);
                int childIndex = where >= 0 ? where + 1 : -where - 1;
                return this.Pointers.ElementAt(childIndex);
            }

            protected void InsertChild(K value, Node child)
            {
                int where = this.Keys.BinarySearch(value);
                int childIndex = where >= 0 ? where + 1 : -where - 1;
                if (where >= 0)
                {
                    this.Pointers.Insert(childIndex, child);
                }
                else
                {
                    this.Keys.Insert(childIndex, value);
                    this.Pointers.Insert(childIndex + 1, child);
                }
            }

            protected void DeleteChild(K value)
            {
                int where = this.Keys.BinarySearch(value);
                if (where >= 0)
                {
                    this.Keys.RemoveAt(where);
                    this.Pointers.RemoveAt(where + 1);
                }
                else
                {
                    this.Pointers.Remove(this.GetChild(value));
                }
            }

            public Node GetChildLeftBrother(K value)
            {
                int where = this.Keys.BinarySearch(value);
                int childIndex = where >= 0 ? where + 1 : -where - 1;
                if (childIndex > 0)
                {
                    return this.Pointers.ElementAtOrDefault(childIndex - 1);
                }
                return null;
            }

            public Node GetChildRightBrother(K value)
            {
                int where = this.Keys.BinarySearch(value);
                int childIndex = where >= 0 ? where + 1 : -where - 1;
                if (childIndex < this.Keys.Capacity)
                {
                    return this.Pointers.ElementAtOrDefault(childIndex + 1);
                }
                return null;
            }

            public override string ToString()
            {
                return (this.IsRoot() ? "Root " : "") + "InternalNode with first key " + this.Keys.ElementAt(0).ToString(); ;
            }
        }

        private class LeafNode : Node
        {
            List<RecordPointer<K>> RecordPointers { get; set; }

            LeafNode Next { get; set; }

            public LeafNode(BPlusTree<K> _bptree)
            {
                BPTree = _bptree;
                this.Keys = new List<K>(this.MaxKeys());
                this.RecordPointers = new List<RecordPointer<K>>(this.MaxPointers());
            }

            public override Tuple<Node, int> GetValue(K value)
            {
                int where = this.Keys.BinarySearch(value);
                return new Tuple<Node, int>(this, where);
            }

            public override void InsertValue(K value, RecordPointer<K> rp)
            {
                int where = this.Keys.BinarySearch(value);
                int valueIndex = where >= 0 ? where : -where - 1;
                if (where >= 0)
                {
                    this.RecordPointers[valueIndex] = rp;
                }
                else
                {
                    this.Keys.Insert(valueIndex, value);
                    this.RecordPointers.Insert(valueIndex, rp);
                }
                if (BPTree.Root.IsOverflow())
                {
                    Node brother = this.Split();
                    this.CreateNewRoot(brother);
                }
            }

            public override void DeleteValue(K value)
            {
                int where = this.Keys.BinarySearch(value);
                if (where >= 0)
                {
                    this.Keys.RemoveAt(where);
                    this.RecordPointers.RemoveAt(where);
                }
            }

            public override K GetFirstLeafKey()
            {
                return this.Keys.ElementAt(0);
            }

            public override void Merge(Node brother)
            {
                LeafNode node = (LeafNode)brother;
                this.Keys.AddRange(node.Keys);
                this.RecordPointers.AddRange(node.RecordPointers);
                this.Next = node.Next;
            }

            public override Node Split()
            {
                LeafNode brother = new LeafNode(BPTree);
                brother.Parent = this.Parent;
                int from = (this.Keys.Count + 1) / 2;
                int count = this.Keys.Count - from;
                brother.Keys.AddRange(this.Keys.GetRange(from, count));
                brother.RecordPointers.AddRange(this.RecordPointers.GetRange(from, count));

                this.Keys.RemoveRange(from, count);
                this.RecordPointers.RemoveRange(from, count);

                brother.Next = this.Next;
                this.Next = brother;

                return brother;
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
                return this.RecordPointers.Count < this.MinPointers();
            }

            public override bool IsOverflow()
            {
                return this.RecordPointers.Count > this.MaxPointers();
            }

            public override string ToString()
            {
                return (this.IsRoot() ? "Root " : "") + "LeafNode with first key " + this.Keys.ElementAt(0).ToString(); ;
            }
        }
    }
}